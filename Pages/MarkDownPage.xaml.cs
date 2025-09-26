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
            // 创建工具栏映射
            _toolbars = new Dictionary<string, FrameworkElement>
            {
                { "file", FileToolbar },
                { "edit", EditToolbar },
                { "format", FormatToolbar },
                { "insert", InsertToolbar },
                { "view", ViewToolbar },
                { "tools", ToolsToolbar }
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
            foreach (var toolbar in _toolbars.Values)
            {
                toolbar.Visibility = Visibility.Collapsed;
            }

            // 显示选中的工具栏
            if (!string.IsNullOrEmpty(activeTag) && _toolbars.ContainsKey(activeTag))
            {
                _toolbars[activeTag].Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"切换到工具栏: {activeTag}");
            }
        }
    }
}