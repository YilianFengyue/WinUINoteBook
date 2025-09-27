using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace App2.Pages
{
    public sealed partial class MarkDownPage : Page
    {
        private Dictionary<string, FrameworkElement> _toolbars;

        public MarkDownPage()
        {
            InitializeComponent();
            InitializeToolbars();
        }

        private void InitializeToolbars()
        {
            // 创建新的工具栏映射 - 4个合并后的工具栏
            _toolbars = new Dictionary<string, FrameworkElement>
            {
                { "file", FileEditToolbar },      // 文件+编辑
                { "edit", FileEditToolbar },      // 同样映射到文件+编辑
                { "format", FormatStyleToolbar }, // 格式+样式
                { "insert", InsertMediaToolbar }, // 插入+媒体
                { "view", ViewToolsToolbar },     // 视图+工具
                { "tools", ViewToolsToolbar }     // 同样映射到视图+工具
            };
        }

        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopPivot.SelectedItem is PivotItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();
                SwitchToolbar(tag);
            }
        }

        private void SwitchToolbar(string activeTag)
        {
            // 隐藏所有工具栏
            FileEditToolbar.Visibility = Visibility.Collapsed;
            FormatStyleToolbar.Visibility = Visibility.Collapsed;
            InsertMediaToolbar.Visibility = Visibility.Collapsed;
            ViewToolsToolbar.Visibility = Visibility.Collapsed;

            // 显示选中的工具栏
            if (!string.IsNullOrEmpty(activeTag) && _toolbars.ContainsKey(activeTag))
            {
                _toolbars[activeTag].Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"切换到工具栏: {activeTag}");
            }
        }
    }
}