using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2
{
    /// <summary>
    /// 简洁现代的WinUI3主窗口 - 仿OneNote布局
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            SetupEventHandlers();
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
        /// 设置事件处理器
        /// </summary>
        private void SetupEventHandlers()
        {
            // 悬浮按钮事件
            AddContentButton.Click += AddContentButton_Click;

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
    }
}