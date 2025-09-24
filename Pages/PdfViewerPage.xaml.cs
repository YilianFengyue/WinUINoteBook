using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Windows.Storage.Streams;  // IRandomAccessStream / InMemoryRandomAccessStream
using WinRT;                    // Stream.AsInputStream() / AsRandomAccessStream()
namespace App2.Pages
{
    public sealed partial class PdfViewerPage : Page
    {
        private IntPtr _ownerHwnd;
        private string? _pickedFolder;         // 映射的 PDF 所在目录（本次会话）
        private string? _currentPdfName;       // 当前打开的文件名（不含路径）
        private bool _viewerReady;             // pdf.js viewer 是否已初始化
        
        // pdf.js 静态资源根目录：AppBase\wwwroot\pdfjs
        private static string WebRoot =>
            Path.Combine(AppContext.BaseDirectory, "wwwroot");
        private static string PickedCacheDir =>
            Path.Combine(WebRoot, "picked_cache");
        public PdfViewerPage()
        {
            InitializeComponent();
            Loaded += PdfViewerPage_Loaded;
        }

        private async void PdfViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            await Viewer.EnsureCoreWebView2Async();

            // ★新增【网络调试日志】——同步属性，不要 await
            Viewer.CoreWebView2.WebResourceResponseReceived += (s, ev) =>
            {
                try
                {
                    var req = ev.Request;
                    var resp = ev.Response;
                    System.Diagnostics.Debug.WriteLine(
                        $"[NET] {req.Method} {resp.StatusCode} {req.Uri}");
                }
                catch { }
            };
            //// ★可选【兜底】：禁用 WebView2 默认下载气泡（已隐藏下载按钮，正常不会触发，此处防误触）
            //Viewer.CoreWebView2.DownloadStarting += (s, ev) =>
            //{
            //    try
            //    {
            //        ev.Handled = true; // 屏蔽系统下载弹窗；如需自定义保存逻辑，可在这里处理
            //    }
            //    catch { }
            //};

            // ★删除：跨域拦截不再需要（我们改为同源方案）
            // Viewer.CoreWebView2.AddWebResourceRequestedFilter(
            //     "https://picked.local/*", CoreWebView2WebResourceContext.All);
            // Viewer.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;

            // 映射静态资源主机名 appassets.local -> wwwroot
            if (Directory.Exists(WebRoot))
            {
                Viewer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "appassets.local",
                    WebRoot,
                    CoreWebView2HostResourceAccessKind.Allow);
            }

            // ★新增：确保同源缓存目录存在（wwwroot/picked_cache）
            Directory.CreateDirectory(PickedCacheDir);

            // WebView2 基本设置
            Viewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            Viewer.CoreWebView2.Settings.IsZoomControlEnabled = false;

            Viewer.NavigationCompleted += Viewer_NavigationCompleted;

            // 先加载空 viewer（不带 file），验证静态资源 OK
            Viewer.Source = new Uri("https://appassets.local/pdfjs/web/viewer.html");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Window owner)
            {
                _ownerHwnd = WindowNative.GetWindowHandle(owner);
            }
        }

        // 作用：每次加载/刷新 viewer.html 后，先做 ready 轮询，再注入隐藏样式。
        private void Viewer_NavigationCompleted(
            Microsoft.UI.Xaml.Controls.WebView2 sender,
            Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            // 原有：等待 viewer 初始化
            _ = EnsureViewerReadyAsync();

            // ★新增：注入隐藏样式（不依赖 initialized，也能先隐藏掉外观，避免闪一下）
            _ = InjectViewerChromeHiderAsync();
        }

        #region 文件打开/重新载入

        private async void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            if (_ownerHwnd != IntPtr.Zero)
                InitializeWithWindow.Initialize(picker, _ownerHwnd);

            picker.FileTypeFilter.Add(".pdf");
            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            // 确保 Core 创建
            if (Viewer.CoreWebView2 == null)
                await Viewer.EnsureCoreWebView2Async();

            // ★同源方案：复制到 wwwroot/picked_cache
            var safeName = Path.GetFileName(file.Path); // 保留原始文件名
            var targetPath = Path.Combine(PickedCacheDir, safeName);
            try
            {
                File.Copy(file.Path, targetPath, overwrite: true);
            }
            catch (IOException)
            {
                // 被占用等情况，尝试改名复制
                var name = Path.GetFileNameWithoutExtension(safeName);
                var ext = Path.GetExtension(safeName);
                var alt = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                targetPath = Path.Combine(PickedCacheDir, alt);
                File.Copy(file.Path, targetPath, overwrite: true);
                safeName = Path.GetFileName(targetPath);
            }

            // ★关键：用 appassets.local（同源）访问复制后的文件 → 不会触发 CSP/CORS
            var fileUrl = $"https://appassets.local/picked_cache/{Uri.EscapeDataString(safeName)}";
            var viewerUrl = $"https://appassets.local/pdfjs/web/viewer.html?file={Uri.EscapeDataString(fileUrl)}";
            Viewer.Source = new Uri(viewerUrl);

            // 记录当前状态（可选）
            _pickedFolder = PickedCacheDir;
            _currentPdfName = safeName;
        }


        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            Viewer.Reload();
            _viewerReady = false;
        }

        #endregion

        #region 与 pdf.js 交互的公共工具

        // 轮询直到 viewer 初始化完毕（PDFViewerApplication.initialized == true）
        private async Task EnsureViewerReadyAsync()
        {
            if (Viewer.CoreWebView2 == null)
                await Viewer.EnsureCoreWebView2Async();

            for (int i = 0; i < 200; i++) // 最多等 ~20s
            {
                var ready = await Viewer.ExecuteScriptAsync(
                    "(()=> (typeof PDFViewerApplication!=='undefined' && PDFViewerApplication.initialized) ? '1':'0')()");
                if (TrimJsResult(ready) == "1")
                {
                    _viewerReady = true;

                    // 初始化总页数显示
                    _ = UpdatePageCountAsync();
                    return;
                }
                await Task.Delay(100);
            }
        }

        private static string TrimJsResult(string jsResult)
        {
            // WebView2 返回 JSON 字符串, 可能包含引号
            return jsResult?.Trim('"', ' ', '\n', '\r') ?? string.Empty;
        }

        private async Task<string> JsAsync(string script)
        {
            if (!_viewerReady)
                await EnsureViewerReadyAsync();
            return await Viewer.ExecuteScriptAsync(script);
        }

        private async Task UpdatePageCountAsync()
        {
            // 读总页数并更新 UI
            var count = await JsAsync("(()=>PDFViewerApplication.pdfViewer.pagesCount)()");
            var trimmed = TrimJsResult(count);
            if (int.TryParse(trimmed, out var pages))
            {
                PageCountText.Text = pages.ToString();
            }
        }

        #endregion

        #region 视图控制（缩放/适配/旋转）

        private async void ZoomIn_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.zoomIn();");

        private async void ZoomOut_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.zoomOut();");

        private async void FitWidth_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.pdfViewer.currentScaleValue='page-width';");

        private async void FitPage_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.pdfViewer.currentScaleValue='page-fit';");

        private async void RotateCW_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.rotateClockwise();");

        private async void RotateCCW_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.rotateCounterclockwise();");
        private void CoreWebView2_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            try
            {
                var req = e.Request;
                if (!req.Uri.StartsWith("https://picked.local/", StringComparison.OrdinalIgnoreCase))
                    return;
                if (string.IsNullOrEmpty(_pickedFolder))
                    return;

                var uri = new Uri(req.Uri);
                var fileName = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
                var fullPath = Path.Combine(_pickedFolder, fileName);
                if (!File.Exists(fullPath))
                    return;

                var fi = new FileInfo(fullPath);
                var total = fi.Length;

                // 统一要暴露/允许的头
                string commonCorsHeaders =
                    "Access-Control-Allow-Origin: https://appassets.local\r\n" +
                    "Access-Control-Allow-Credentials: true\r\n" +
                    "Access-Control-Expose-Headers: Accept-Ranges, Content-Length, Content-Range, Content-Type\r\n" +
                    "Cache-Control: no-cache\r\n";

                // 0) 先处理 CORS 预检（OPTIONS）
                if (string.Equals(req.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    // 预检里务必允许 Range 头；不建议用 *（与 Credentials 不兼容）
                    var headers =
                        "Access-Control-Allow-Origin: https://appassets.local\r\n" +
                        "Access-Control-Allow-Methods: GET, HEAD, OPTIONS\r\n" +
                        "Access-Control-Allow-Headers: Range, Content-Type\r\n" +
                        "Access-Control-Allow-Credentials: true\r\n";
                    e.Response = Viewer.CoreWebView2.Environment.CreateWebResourceResponse(
                        new InMemoryRandomAccessStream(), 204, "No Content", headers);
                    return;
                }

                // 1) 处理 HEAD（pdf.js 常用来探测是否支持 Range）
                if (string.Equals(req.Method, "HEAD", StringComparison.OrdinalIgnoreCase))
                {
                    var headers =
                        $"Content-Type: application/pdf\r\n" +
                        "Accept-Ranges: bytes\r\n" +
                        $"Content-Length: {total}\r\n" +
                        commonCorsHeaders;

                    e.Response = Viewer.CoreWebView2.Environment.CreateWebResourceResponse(
                        new InMemoryRandomAccessStream(), 200, "OK", headers); // 空体
                    return;
                }

                // 2) 处理 GET（含 Range）
                var rangeHeader = req.Headers.GetHeader("Range"); // 例如 "bytes=0-"
                long start = 0, end = total - 1;
                bool isPartial = false;

                if (!string.IsNullOrEmpty(rangeHeader) &&
                    rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = rangeHeader.Substring(6).Split('-');
                    if (long.TryParse(parts[0], out var s)) start = s;
                    if (parts.Length > 1 && long.TryParse(parts[1], out var epos)) end = epos;
                    if (end >= total) end = total - 1;
                    if (start < 0) start = 0;
                    if (start <= end) isPartial = true;
                }

                var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (isPartial)
                {
                    fs.Position = start;
                    var contentLength = end - start + 1;

                    var ms206 = new InMemoryRandomAccessStream();
                    using (var outStream = ms206.AsStreamForWrite())
                    {
                        var buf = new byte[81920];
                        long left = contentLength;
                        while (left > 0)
                        {
                            int toRead = (int)Math.Min(buf.Length, left);
                            int n = fs.Read(buf, 0, toRead);
                            if (n == 0) break;
                            outStream.Write(buf, 0, n);
                            left -= n;
                        }
                        outStream.Flush();
                    }
                    ms206.Seek(0);

                    var headers =
                        $"Content-Type: application/pdf\r\n" +
                        "Accept-Ranges: bytes\r\n" +
                        $"Content-Range: bytes {start}-{end}/{total}\r\n" +
                        $"Content-Length: {contentLength}\r\n" +
                        commonCorsHeaders;

                    e.Response = Viewer.CoreWebView2.Environment.CreateWebResourceResponse(
                        ms206, 206, "Partial Content", headers);
                }
                else
                {
                    fs.Position = 0;
                    var ms200 = new InMemoryRandomAccessStream();
                    RandomAccessStream.CopyAsync(fs.AsInputStream(), ms200).AsTask()
                        .GetAwaiter().GetResult();
                    ms200.Seek(0);

                    var headers =
                        $"Content-Type: application/pdf\r\n" +
                        "Accept-Ranges: bytes\r\n" +
                        $"Content-Length: {total}\r\n" +
                        commonCorsHeaders;

                    e.Response = Viewer.CoreWebView2.Environment.CreateWebResourceResponse(
                        ms200, 200, "OK", headers);
                }
            }
            catch
            {
                // 忽略错误，让 WebView2 走默认处理
            }
        }
        // ★新增函数：向 pdf.js 页面注入 CSS，隐藏其自带的工具栏/侧栏/按钮，并消除顶部空白。
        // 原理：viewer.html 使用 --toolbar-height 控制内容顶距；把它设为 0，同时隐藏相关容器。
            private async Task InjectViewerChromeHiderAsync()
            {
                // 这段 CSS 只做显示层面的隐藏，不改 pdf.js 逻辑。升级 pdf.js 基本不受影响。
                var css = @"
            :root { --toolbar-height: 0px !important; }

            /* 顶部工具栏、二级工具栏、查找栏、侧边栏切换按钮等统统隐藏 */
            #toolbarContainer,
            #secondaryToolbar,
            #findbar,
            #secondaryToolbarToggle,
            #openFile,
            #print, #secondaryPrint,
            #download, #secondaryDownload,
            #viewFind, #viewThumbnail, #viewOutline, #viewAttachments, #viewLayers {
                display: none !important;
            }

            /* 抹掉因为 toolbar 被隐藏而留下的顶边距 */
            #viewerContainer { top: 0 !important; }
            #sidebarContainer { top: 0 !important; }
            ";

                // 把 CSS 以 <style> 注入页面
                var script = $@"(function(){{
            try {{
                if (!document.querySelector('style[data-winui-hidechrome]')) {{
                    const s = document.createElement('style');
                    s.setAttribute('data-winui-hidechrome','1');
                    s.textContent = `{css}`;
                    document.documentElement.appendChild(s);
                }}
            }} catch(_){{
            }}
        }})();";

                // 如果 Core 尚未建立，这里会抛；外层调用前已有 EnsureCoreWebView2Async。
                await Viewer.CoreWebView2.ExecuteScriptAsync(script);
            }

        // 辅助子流：只暴露 length 个字节给 206 响应
        private sealed class SubStream : Stream
        {
            private readonly Stream _base; private long _left;
            public SubStream(Stream b, long len) { _base = b; _left = len; }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _left;
            public override long Position { get => 0; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_left <= 0) return 0;
                if (count > _left) count = (int)_left;
                var n = _base.Read(buffer, offset, count);
                _left -= n;
                return n;
            }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            protected override void Dispose(bool disposing) { try { if (disposing) _base.Dispose(); } finally { base.Dispose(disposing); } }
        }
        #endregion

        #region 页码跳转

        private async void PrevPage_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.pdfViewer.currentPageNumber = Math.max(1, PDFViewerApplication.pdfViewer.currentPageNumber - 1);");

        private async void NextPage_Click(object sender, RoutedEventArgs e)
            => await JsAsync("PDFViewerApplication.pdfViewer.currentPageNumber = Math.min(PDFViewerApplication.pdfViewer.pagesCount, PDFViewerApplication.pdfViewer.currentPageNumber + 1);");

        private async void GoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PageNumberBox.Text, out var n))
            {
                await JsAsync($"PDFViewerApplication.pdfViewer.currentPageNumber = {n};");
            }
        }

        private async void PageNumberBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                GoToPage_Click(sender!, e);
                // 同步显示，防止误差
                await UpdatePageCountAsync();
            }
        }

        #endregion

        // ★新增：保存为（当前打开的 PDF 原文件）
        // 说明：当前我们用“同源缓存”方案，文件位于 wwwroot/picked_cache；
        //      这里复制该文件到用户选定的位置（不含批注合并，见后文注释保存）。
        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentPdfName))
                return;

            var save = new Windows.Storage.Pickers.FileSavePicker();
            if (_ownerHwnd != IntPtr.Zero) InitializeWithWindow.Initialize(save, _ownerHwnd);
            save.SuggestedFileName = Path.GetFileNameWithoutExtension(_currentPdfName);
            save.FileTypeChoices.Add("PDF 文档", new System.Collections.Generic.List<string> { ".pdf" });
            var target = await save.PickSaveFileAsync();
            if (target is null) return;

            var src = Path.Combine(PickedCacheDir, _currentPdfName);
            await Task.Run(() => File.Copy(src, target.Path, overwrite: true));
        }

        // ★新增：打印（调用 pdf.js 的打印逻辑，弹系统打印对话框）
        private async void Print_Click(object sender, RoutedEventArgs e)
        {
            await JsAsync("PDFViewerApplication.triggerPrinting && PDFViewerApplication.triggerPrinting();");
        }

        // ★新增：查找 —— 回车 / 按钮触发
        private async void FindBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                await FindCoreAsync(FindBox.Text, previous: false);
            }
        }
        private async void FindNext_Click(object sender, RoutedEventArgs e)
            => await FindCoreAsync(FindBox.Text, previous: false);

        private async void FindPrev_Click(object sender, RoutedEventArgs e)
            => await FindCoreAsync(FindBox.Text, previous: true);

        // ★新增：调用 pdf.js 的 eventBus 执行查找（官方 viewer 支持）
        private async Task FindCoreAsync(string query, bool previous)
        {
            if (string.IsNullOrWhiteSpace(query)) return;

            // 用 JSON 字符串安全传参
            var q = System.Text.Json.JsonSerializer.Serialize(query);
            var js = $@"
        (function(){{
          const bus = PDFViewerApplication.eventBus;
          bus.dispatch('find', {{
            type: 'find',
            query: {q},
            caseSensitive: false,
            entireWord: false,
            highlightAll: true,
            findPrevious: {(previous ? "true" : "false")},
            phraseSearch: true
          }});
        }})();";
            await JsAsync(js);
        }

        // ★新增：高亮/手写/撤销（直接“点” viewer 里隐藏按钮）
        private async void ToggleHighlight_Click(object sender, RoutedEventArgs e)
            => await JsAsync("document.getElementById('editorHighlight')?.click();");

        private async void TogglePen_Click(object sender, RoutedEventArgs e)
            => await JsAsync("document.getElementById('editorInk')?.click();");

        private async void Erase_Click(object sender, RoutedEventArgs e)
            => await JsAsync("document.getElementById('editorUndo')?.click();");

        // ★新增：导出“含批注”的 PDF（使用 pdf.js 自带 download 打包 AnnotationStorage）
        private async void SaveAnnot_Click(object sender, RoutedEventArgs e)
        {
            // 1) 先拿 HWND，确保 FileSavePicker 能弹（你已有 EnsureOwnerHwnd）
            EnsureOwnerHwnd();

            // 2) 一次性订阅 DownloadStarting，只对这次“下载含批注”生效
            TaskCompletionSource<bool> tcs = new();

            void OneShot(object? s, CoreWebView2DownloadStartingEventArgs ev)
            {
                // 只用一次就退订，避免影响以后
                Viewer.CoreWebView2.DownloadStarting -= OneShot;

                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        var picker = new Windows.Storage.Pickers.FileSavePicker();
                        if (_ownerHwnd != IntPtr.Zero)
                            InitializeWithWindow.Initialize(picker, _ownerHwnd);

                        var baseName = string.IsNullOrEmpty(_currentPdfName)
                            ? "document"
                            : Path.GetFileNameWithoutExtension(_currentPdfName);

                        picker.SuggestedFileName = baseName + "_annotated";
                        picker.FileTypeChoices.Add("PDF 文档",
                            new System.Collections.Generic.List<string> { ".pdf" });

                        var file = await picker.PickSaveFileAsync();
                        if (file is null)
                        {
                            ev.Cancel = true;
                            tcs.TrySetResult(false);
                            return;
                        }

                        // 把 WebView2 的下载直接落到用户选的文件上，并屏蔽默认下载 UI
                        ev.ResultFilePath = file.Path;
                        ev.Handled = true;
                        tcs.TrySetResult(true);
                    }
                    catch
                    {
                        ev.Cancel = true;
                        tcs.TrySetResult(false);
                    }
                });
            }

            Viewer.CoreWebView2.DownloadStarting += OneShot;

            // 3) 让 pdf.js 执行“下载”（它会把批注写入存储并打包进导出的 PDF）
            await JsAsync("PDFViewerApplication.download && PDFViewerApplication.download();");

            // 4) 等待保存结果（主要是等 FileSavePicker 走完）
            await tcs.Task;
        }

        // ★新增：在 Page 内拿到宿主窗口 HWND（无需改 App.xaml.cs）
        private void EnsureOwnerHwnd()
        {
            if (_ownerHwnd != IntPtr.Zero) return;
            if (App.m_window is not null)
                _ownerHwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
        }

    }

}
