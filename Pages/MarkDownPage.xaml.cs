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
            // ����������ӳ��
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
            // �������й�����
            foreach (var toolbar in _toolbars.Values)
            {
                toolbar.Visibility = Visibility.Collapsed;
            }

            // ��ʾѡ�еĹ�����
            if (!string.IsNullOrEmpty(activeTag) && _toolbars.ContainsKey(activeTag))
            {
                _toolbars[activeTag].Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"�л���������: {activeTag}");
            }
        }
    }
}