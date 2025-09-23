using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2.Pages
{
    /// <summary>
    /// 笔记页面 - 从MainWindow提取的笔记功能
    /// </summary>
    public sealed partial class NotePage : Page
    {
        public NotePage()
        {
            this.InitializeComponent();
            InitializePage();
        }

        /// <summary>
        /// 初始化页面
        /// </summary>
        private void InitializePage()
        {
            // 延迟设置焦点到编辑器
            this.DispatcherQueue.TryEnqueue(() =>
            {
                ContentEditor.Focus(FocusState.Programmatic);
            });
        }

        /// <summary>
        /// 添加新内容按钮点击事件
        /// </summary>
        private void AddContentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 简单演示：在编辑器中插入新内容
                ContentEditor.Document.Selection.SetText(Microsoft.UI.Text.TextSetOptions.None,
                    "\n\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " 新增内容\n");
                System.Diagnostics.Debug.WriteLine("添加新内容");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"添加内容时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 顶部Pivot切换事件
        /// </summary>
        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 这里可以根据不同的Pivot选项切换不同的工具栏或功能
            if (TopPivot.SelectedItem is PivotItem pi)
            {
                System.Diagnostics.Debug.WriteLine($"切换到: {pi.Header}");
            }
        }
    }
}