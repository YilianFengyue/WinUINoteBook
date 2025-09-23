using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace App2.Pages
{
    public sealed partial class PdfViewerPage : Page
    {
        private IntPtr _ownerHwnd;
        private readonly string _webRoot;

        public PdfViewerPage()
        {
            InitializeComponent();
            // 仅确保初始化，不做任何目录映射
            _ = Viewer.EnsureCoreWebView2Async();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Window owner)
            {
                _ownerHwnd = WindowNative.GetWindowHandle(owner);
            }
        }

        

        private async void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            if (_ownerHwnd != IntPtr.Zero)
                WinRT.Interop.InitializeWithWindow.Initialize(picker, _ownerHwnd);

            picker.FileTypeFilter.Add(".pdf");
            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            // 确保 WebView2 已初始化
            if (Viewer.CoreWebView2 == null)
                await Viewer.EnsureCoreWebView2Async();

            // ⬇️ 把“该 PDF 所在文件夹”映射为虚拟主机 picked
            var folder = System.IO.Path.GetDirectoryName(file.Path)!;
            var fileName = System.IO.Path.GetFileName(file.Path);
            Viewer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "picked", folder, Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            // 通过 https://picked/xxx.pdf 打开（使用 Edge 内置 PDF 查看器）
            var url = $"https://picked/{Uri.EscapeDataString(fileName)}";
            Viewer.Source = new Uri(url);
        }
        private void Reload_Click(object sender, RoutedEventArgs e) => Viewer.Reload();
    }
}
