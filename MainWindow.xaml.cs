using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2
{
    /// <summary>
    /// �ع���������� - ʹ��NavigationView���ж�ҳ�浼��
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        private void InitializeWindow()
        {
            // ���ñ�������չ
            this.ExtendsContentIntoTitleBar = true;
        }

        /// <summary>
        /// NavigationView��������¼�
        /// </summary>
        private void MainNavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ĭ�ϵ������ʼ�ҳ��
            if (MainNavigationView.MenuItems.Count > 0)
            {
                var firstItem = MainNavigationView.MenuItems[0] as NavigationViewItem;
                if (firstItem != null)
                {
                    MainNavigationView.SelectedItem = firstItem;
                    NavigateToPage(firstItem);
                }
            }
        }

        /// <summary>
        /// NavigationViewѡ��ı��¼�
        /// </summary>
        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item)
            {
                NavigateToPage(item);
            }
        }

        /// <summary>
        /// ������ָ��ҳ��
        /// </summary>
        private void NavigateToPage(NavigationViewItem item)
        {
            if (item?.Tag is string tag)
            {
                try
                {
                    // ����Tag��ȡҳ������
                    Type pageType = Type.GetType(tag);
                    if (pageType != null)
                    {
                        // ������ҳ�棬����PDFҳ����Ҫ���ݴ��ڲ���
                        object parameter = pageType.Name == "PdfViewerPage" ? this : null;
                        ContentFrame.Navigate(pageType, parameter);

                        System.Diagnostics.Debug.WriteLine($"������: {item.Content} ({tag})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"�޷��ҵ�ҳ������: {tag}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"����ʧ��: {ex.Message}");
                }
            }
        }
    }
}