using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2
{
    /// <summary>
    /// 重构后的主窗口 - 使用NavigationView进行多页面导航
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        /// <summary>
        /// 初始化窗口设置
        /// </summary>
        private void InitializeWindow()
        {
            // 启用标题栏扩展
            this.ExtendsContentIntoTitleBar = true;
        }

        /// <summary>
        /// NavigationView加载完成事件
        /// </summary>
        private void MainNavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            // 默认导航到笔记页面
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
        /// NavigationView选择改变事件
        /// </summary>
        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item)
            {
                NavigateToPage(item);
            }
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        private void NavigateToPage(NavigationViewItem item)
        {
            if (item?.Tag is string tag)
            {
                try
                {
                    // 根据Tag获取页面类型
                    Type pageType = Type.GetType(tag);
                    if (pageType != null)
                    {
                        // 导航到页面，对于PDF页面需要传递窗口参数
                        object parameter = pageType.Name == "PdfViewerPage" ? this : null;
                        ContentFrame.Navigate(pageType, parameter);

                        System.Diagnostics.Debug.WriteLine($"导航到: {item.Content} ({tag})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"无法找到页面类型: {tag}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"导航失败: {ex.Message}");
                }
            }
        }
    }
}