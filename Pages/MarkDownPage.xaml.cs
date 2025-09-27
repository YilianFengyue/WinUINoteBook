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
            // �����µĹ�����ӳ�� - 4���ϲ���Ĺ�����
            _toolbars = new Dictionary<string, FrameworkElement>
            {
                { "file", FileEditToolbar },      // �ļ�+�༭
                { "edit", FileEditToolbar },      // ͬ��ӳ�䵽�ļ�+�༭
                { "format", FormatStyleToolbar }, // ��ʽ+��ʽ
                { "insert", InsertMediaToolbar }, // ����+ý��
                { "view", ViewToolsToolbar },     // ��ͼ+����
                { "tools", ViewToolsToolbar }     // ͬ��ӳ�䵽��ͼ+����
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
            FileEditToolbar.Visibility = Visibility.Collapsed;
            FormatStyleToolbar.Visibility = Visibility.Collapsed;
            InsertMediaToolbar.Visibility = Visibility.Collapsed;
            ViewToolsToolbar.Visibility = Visibility.Collapsed;

            // ��ʾѡ�еĹ�����
            if (!string.IsNullOrEmpty(activeTag) && _toolbars.ContainsKey(activeTag))
            {
                _toolbars[activeTag].Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"�л���������: {activeTag}");
            }
        }
    }
}