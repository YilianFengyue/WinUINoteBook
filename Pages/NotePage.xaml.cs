using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace App2.Pages
{
    public sealed partial class NotePage : Page
    {
        private bool _bridgeReady;
        private IntPtr _ownerHwnd;

        public NotePage()
        {
            InitializeComponent();
            InitializePage();
            Loaded += NotePage_Loaded;
        }

        private async void NotePage_Loaded(object sender, RoutedEventArgs e)
        {
            // 取 HWND 以便文件对话框工作
            if (App.m_window is not null)
                _ownerHwnd = WindowNative.GetWindowHandle(App.m_window);

            await InitWebViewAsync();
        }

        private async void InitializePage()
        {
            await Task.Delay(50);
            _ = DispatcherQueue.TryEnqueue(() => EditorView.Focus(FocusState.Programmatic));
        }

        // ====== WebView2 初始化与映射 ======
        private async Task InitWebViewAsync()
        {
            await EditorView.EnsureCoreWebView2Async();

            // 只订一次
            EditorView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            // 找 wwwroot（输出目录优先，找不到就向上探测）
            var baseDir = AppContext.BaseDirectory;
            var assetsDir = Path.Combine(baseDir, "wwwroot");
            if (!File.Exists(Path.Combine(assetsDir, "index.html")))
            {
                var probe = baseDir;
                for (int i = 0; i < 8; i++)
                {
                    probe = Path.GetFullPath(Path.Combine(probe, ".."));
                    var candidate = Path.Combine(probe, "wwwroot", "index.html");
                    if (File.Exists(candidate))
                    {
                        assetsDir = Path.Combine(probe, "wwwroot");
                        break;
                    }
                }
            }

            // 映射为虚拟主机
            EditorView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets", assetsDir, CoreWebView2HostResourceAccessKind.Allow);

            // 注入：DOM ready 时通知宿主
            await EditorView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                "window.__host = window.chrome && window.chrome.webview;" +
                "if (window.__host){" +
                " document.addEventListener('DOMContentLoaded', ()=>{" +
                "  window.__host.postMessage(JSON.stringify({type:'evt',name:'dom-ready'}));" +
                " });" +
                "}");

            EditorView.DefaultBackgroundColor = Colors.Transparent;
            EditorView.Source = new Uri("https://appassets/index.html");
        }

        // ====== Host ← Page ======
        private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                var json = args.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(json)) return;
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString();
                var name = root.GetProperty("name").GetString();

                if (type == "evt" && name == "dom-ready")
                {
                    _bridgeReady = true;
                    PostToWeb(new { type = "evt", name = "host-ready", payload = new { time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } });
                }
                else if (type == "evt" && name == "ack")
                {
                    System.Diagnostics.Debug.WriteLine("[ACK] " + root.GetProperty("payload").ToString());
                }
                else if (type == "resp" && name == "markdown")
                {
                    // 保存回调
                    var md = root.GetProperty("payload").GetProperty("text").GetString() ?? "";
                    _ = SaveMarkdownToUserAsync(md);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Host] WebMessage error: " + ex);
            }
        }

        // ====== Host → Page ======
        private void PostToWeb(object obj)
        {
            if (!_bridgeReady || EditorView?.CoreWebView2 is null) return;
            var json = JsonSerializer.Serialize(obj);
            EditorView.CoreWebView2.PostWebMessageAsJson(json);
        }

        // ====== 顶部 Pivot（原样保留） ======
        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopPivot.SelectedItem is PivotItem pi)
                System.Diagnostics.Debug.WriteLine($"切换到: {pi.Header}");
        }

        // ====== 工具栏：Markdown / 编辑命令 ======
        private void NewFile_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "new-doc" });

        private async void OpenMarkdown_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            if (_ownerHwnd != IntPtr.Zero) InitializeWithWindow.Initialize(picker, _ownerHwnd);
            picker.FileTypeFilter.Add(".md");
            picker.FileTypeFilter.Add(".markdown");
            var file = await picker.PickSingleFileAsync();
            if (file is null) return;
            var text = await File.ReadAllTextAsync(file.Path, Encoding.UTF8);
            PostToWeb(new { type = "cmd", name = "set-markdown", payload = new { text } });
        }

        private void SaveMarkdown_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "req", name = "get-markdown" });

        private async Task SaveMarkdownToUserAsync(string md)
        {
            var picker = new FileSavePicker();
            if (_ownerHwnd != IntPtr.Zero) InitializeWithWindow.Initialize(picker, _ownerHwnd);
            picker.SuggestedFileName = "note";
            picker.FileTypeChoices.Add("Markdown", new System.Collections.Generic.List<string> { ".md" });
            var file = await picker.PickSaveFileAsync();
            if (file is null) return;
            await File.WriteAllTextAsync(file.Path, md, Encoding.UTF8);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "print" });

        private void Undo_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "undo" });

        private void Redo_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "redo" });

        private void Bold_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-bold" });

        private void Italic_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-italic" });

        private void Underline_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-underline" });

        private void CodeBlock_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-codeblock" });

        private void Blockquote_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-blockquote" });

        private void BulletList_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-bullet-list" });

        private void OrderedList_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "toggle-ordered-list" });

        private async void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            if (_ownerHwnd != IntPtr.Zero) InitializeWithWindow.Initialize(picker, _ownerHwnd);
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");
            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            byte[] bytes = await File.ReadAllBytesAsync(file.Path);
            string mime = Path.GetExtension(file.Path).ToLower() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
            string dataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
            PostToWeb(new { type = "cmd", name = "insert-image", payload = new { src = dataUrl, alt = Path.GetFileName(file.Path) } });
        }

        private async void InsertLink_Click(object sender, RoutedEventArgs e)
        {
            // 简易输入对话框
            ContentDialog dlg = new()
            {
                Title = "插入链接",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                XamlRoot = this.XamlRoot,
                Content = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBox{ Header="文本", Name="TxtText"},
                        new TextBox{ Header="URL", Name="TxtUrl", PlaceholderText="https://example.com" }
                    }
                }
            };
            var result = await dlg.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            var panel = (StackPanel)dlg.Content;
            var text = ((TextBox)panel.Children[0]).Text?.Trim() ?? "";
            var href = ((TextBox)panel.Children[1]).Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(href)) return;

            PostToWeb(new { type = "cmd", name = "insert-link", payload = new { href, text } });
        }

        private void AddContentButton_Click(object sender, RoutedEventArgs e)
            => PostToWeb(new { type = "cmd", name = "insert-timestamp", payload = new { text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } });
    }
}
