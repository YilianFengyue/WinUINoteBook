using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace App2.Pages
{
    public sealed partial class MarkDownPage : Page
    {
        private bool isEditorReady = false;
        private string currentContent = "";
        private EditorStatus editorStatus = new();

        public MarkDownPage()
        {
            this.InitializeComponent();
            InitializeEditor();
        }

        private async void InitializeEditor()
        {
            if (EditorWebView == null) return;

            EditorWebView.NavigationCompleted += EditorWebView_NavigationCompleted;

            // 确保 CoreWebView2 初始化
            await EditorWebView.EnsureCoreWebView2Async();

            // 允许脚本、调试
            EditorWebView.CoreWebView2.Settings.IsScriptEnabled = true;
            EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;

            // 监听来自前端的消息（用 JSON 通道）
            EditorWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            Debug.WriteLine("WebView2 初始化成功，消息监听器已设置");
        }

        private void EditorWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Debug.WriteLine("TipTap Editor 加载成功");

                // 给前端一点时间初始化，然后做一次连通性测试
                _ = Task.Delay(1000).ContinueWith(_ =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        // 不直接发命令，等待 ready；只是日志
                        Debug.WriteLine("等待前端 ready...");
                    });
                });
            }
        }

        /// <summary>接收来自 Vue 的消息（使用 WebMessageAsJson 适配对象/字符串）</summary>
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson; // 统一使用 JSON 字符串
                var message = JsonSerializer.Deserialize<EditorMessage>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (message == null)
                {
                    Debug.WriteLine("收到空消息或反序列化失败");
                    return;
                }

                Debug.WriteLine($"收到消息: {message.Type} - {message.Event}");
                HandleEditorMessage(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理消息失败: {ex}");
            }
        }

        private void HandleEditorMessage(EditorMessage message)
        {
            switch (message.Event)
            {
                case "ready":
                    OnEditorReady(message.Payload);
                    break;

                case "content-changed":
                    OnContentChanged(message.Payload);
                    break;

                case "selection-changed":
                    OnSelectionChanged(message.Payload);
                    break;

                case "focus":
                    Debug.WriteLine("编辑器获得焦点");
                    break;

                case "blur":
                    Debug.WriteLine("编辑器失去焦点");
                    break;

                case "error":
                    OnEditorError(message.Payload);
                    break;

                case "ping":
                    Debug.WriteLine("收到 ping");
                    break;
            }
        }

        private void OnEditorReady(JsonElement payload)
        {
            isEditorReady = true;

            try
            {
                var version = payload.GetProperty("version").GetString();
                var commandCount = payload.GetProperty("supportedCommands").GetArrayLength();
                Debug.WriteLine($"编辑器就绪 - 版本: {version}, 支持命令数: {commandCount}");
            }
            catch { /* 忽略解析异常 */ }

            DispatcherQueue.TryEnqueue(() =>
            {
                SavedText.Text = "已就绪";
            });
        }

        private void OnContentChanged(JsonElement payload)
        {
            try
            {
                currentContent = payload.GetProperty("html").GetString();
                var words = payload.GetProperty("words").GetInt32();
                var characters = payload.GetProperty("characters").GetInt32();
                var isEmpty = payload.GetProperty("isEmpty").GetBoolean();

                Debug.WriteLine($"内容变化 - 字数: {words}, 字符数: {characters}, 空: {isEmpty}");

                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateStatusBar(words, characters, isEmpty);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析 content-changed 失败: {ex.Message}");
            }
        }

        private void OnSelectionChanged(JsonElement payload)
        {
            try
            {
                var from = payload.GetProperty("from").GetInt32();
                var to = payload.GetProperty("to").GetInt32();
                var activeFormats = payload.GetProperty("activeFormats");

                var isBold = activeFormats.TryGetProperty("bold", out var b) && b.GetBoolean();
                var isItalic = activeFormats.TryGetProperty("italic", out var i) && i.GetBoolean();

                Debug.WriteLine($"选区变化 - 位置: {from}-{to}, 粗体: {isBold}, 斜体: {isItalic}");

                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateToolbarState(activeFormats);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析 selection-changed 失败: {ex.Message}");
            }
        }

        private void OnEditorError(JsonElement payload)
        {
            var message = payload.TryGetProperty("message", out var m) ? m.GetString() : "(no message)";
            Debug.WriteLine($"编辑器错误: {message}");
            DispatcherQueue.TryEnqueue(() =>
            {
                SavedText.Text = $"错误：{message}";
            });
        }

        /// <summary>发送命令到编辑器（JSON 通道）</summary>
        private async Task SendEditorCommand(string command, object payload = null)
        {
            if (!isEditorReady)
            {
                Debug.WriteLine("编辑器未就绪，无法发送命令");
                return;
            }

            var msg = new
            {
                type = "editor-command",
                command,
                payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var json = JsonSerializer.Serialize(msg);

            try
            {
                EditorWebView.CoreWebView2.PostWebMessageAsJson(json);
                Debug.WriteLine($"发送命令: {command}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发送命令失败: {ex.Message}");
            }
        }

        // ========== 工具栏事件处理（全部接上） ==========

        // 基础格式
        private async void OnBold(object sender, RoutedEventArgs e) => await SendEditorCommand("bold");
        private async void OnItalic(object sender, RoutedEventArgs e) => await SendEditorCommand("italic");
        private async void OnUnderline(object sender, RoutedEventArgs e) => await SendEditorCommand("underline");
        private async void OnStrike(object sender, RoutedEventArgs e) => await SendEditorCommand("strike");
        private async void OnCode(object sender, RoutedEventArgs e) => await SendEditorCommand("code");

        // 段落 / 标题
        private async void OnParagraph(object sender, RoutedEventArgs e) => await SendEditorCommand("paragraph");

        private async void HeadingLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeadingLevelCombo.SelectedIndex < 0) return;
            var level = HeadingLevelCombo.SelectedIndex + 1; // 1..6
            await SendEditorCommand($"heading{level}");
        }

        // 对齐
        private async void OnAlignLeft(object sender, RoutedEventArgs e) => await SendEditorCommand("alignLeft");
        private async void OnAlignCenter(object sender, RoutedEventArgs e) => await SendEditorCommand("alignCenter");
        private async void OnAlignRight(object sender, RoutedEventArgs e) => await SendEditorCommand("alignRight");
        private async void OnAlignJustify(object sender, RoutedEventArgs e) => await SendEditorCommand("alignJustify");

        // 列表
        private async void OnBulletList(object sender, RoutedEventArgs e) => await SendEditorCommand("bulletList");
        private async void OnOrderedList(object sender, RoutedEventArgs e) => await SendEditorCommand("orderedList");
        private async void OnTaskList(object sender, RoutedEventArgs e) => await SendEditorCommand("taskList");

        // 缩进（注意：TipTap 侧若未修正，indent/outdent 可能与 sink/lift 对调）
        private async void OnIndent(object sender, RoutedEventArgs e) => await SendEditorCommand("indent");
        private async void OnOutdent(object sender, RoutedEventArgs e) => await SendEditorCommand("outdent");

        // 插入
        private async void OnInsertLink(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertLink", new { url = "https://example.com", text = "示例链接" });
        }
        private async void OnInsertImage(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertImage", new { src = "https://via.placeholder.com/300x200", alt = "示例图片" });
        }
        private async void OnInsertVideo(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertVideo", new { src = "https://www.w3schools.com/html/mov_bbb.mp4" });
        }
        private async void OnInsertTable(object sender, RoutedEventArgs e) => await SendEditorCommand("insertTable"); // 需确保前端实现

        // 特殊元素 / 代码块 / 分隔线 / 引用
        private async void OnBlockquote(object sender, RoutedEventArgs e) => await SendEditorCommand("insertBlockquote");
        private async void OnHorizontalRule(object sender, RoutedEventArgs e) => await SendEditorCommand("insertHorizontalRule");
        private async void OnCodeBlock(object sender, RoutedEventArgs e) => await SendEditorCommand("insertCodeBlock");

        // 格式 / 历史
        private async void OnClearFormat(object sender, RoutedEventArgs e) => await SendEditorCommand("clearFormat");
        private async void OnUndo(object sender, RoutedEventArgs e) => await SendEditorCommand("undo");
        private async void OnRedo(object sender, RoutedEventArgs e) => await SendEditorCommand("redo");

        // 视图
        private async void OnToggleFullscreen(object sender, RoutedEventArgs e) => await SendEditorCommand("toggleFullscreen");

        // 示例：颜色/高亮（前端已实现 setColor / setHighlight）
        private async void OnSetColorDemo(object sender, RoutedEventArgs e)
            => await SendEditorCommand("setColor", new { color = "#ff4d4f" });
        private async void OnSetHighlightDemo(object sender, RoutedEventArgs e)
            => await SendEditorCommand("setHighlight", new { color = "#ffe58f" });

        // ========== UI 更新方法（可选完善） ==========

        private void UpdateStatusBar(int words, int characters, bool isEmpty)
        {
            WordsText.Text = $"字数: {words}";
            CharsText.Text = $"字符: {characters}";
            SavedText.Text = isEmpty ? "空文档" : "已更新";
        }

        private void UpdateToolbarState(JsonElement activeFormats)
        {
            try
            {
                if (BtnBold != null && activeFormats.TryGetProperty("bold", out var b))
                    BtnBold.IsChecked = b.GetBoolean();

                if (BtnItalic != null && activeFormats.TryGetProperty("italic", out var i))
                    BtnItalic.IsChecked = i.GetBoolean();

                if (BtnUnderline != null && activeFormats.TryGetProperty("underline", out var u))
                    BtnUnderline.IsChecked = u.GetBoolean();

                if (BtnStrike != null && activeFormats.TryGetProperty("strike", out var s))
                    BtnStrike.IsChecked = s.GetBoolean();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateToolbarState error: " + ex);
            }
        }

        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopPivot.SelectedItem is PivotItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();
                Debug.WriteLine($"切换到标签: {tag}");
            }
        }

        // ========== 数据类 ==========

        public class EditorMessage
        {
            public string Type { get; set; }
            public string Event { get; set; }
            public JsonElement Payload { get; set; }
            public long Timestamp { get; set; }
        }

        public class EditorStatus
        {
            public bool IsReady { get; set; }
            public int WordCount { get; set; }
            public int CharacterCount { get; set; }
            public bool IsEmpty { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
        }
    }
}
