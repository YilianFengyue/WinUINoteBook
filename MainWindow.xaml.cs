using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2
{
    /// <summary>
    /// ����ִ���WinUI3������ - ��OneNote����
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
        /// ��ʼ����������
        /// </summary>
        private void InitializeWindow()
        {
            // ���ñ�������չ
            this.ExtendsContentIntoTitleBar = true;
        }

        /// <summary>
        /// �����¼�������
        /// </summary>
        private void SetupEventHandlers()
        {
            // ������ť�¼�
            AddContentButton.Click += AddContentButton_Click;

            // �ӳ����ý��㵽�༭��
            this.DispatcherQueue.TryEnqueue(() =>
            {
                ContentEditor.Focus(FocusState.Programmatic);
            });
        }

        /// <summary>
        /// ��������ݰ�ť����¼�
        /// </summary>
        private void AddContentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ����ʾ���ڱ༭���в���������
                ContentEditor.Document.Selection.SetText(Microsoft.UI.Text.TextSetOptions.None,
                    "\n\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " ��������\n");
                System.Diagnostics.Debug.WriteLine("���������");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�������ʱ����: {ex.Message}");
            }
        }
    }
}