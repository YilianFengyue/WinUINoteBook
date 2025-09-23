using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2.Pages
{
    /// <summary>
    /// �ʼ�ҳ�� - ��MainWindow��ȡ�ıʼǹ���
    /// </summary>
    public sealed partial class NotePage : Page
    {
        public NotePage()
        {
            this.InitializeComponent();
            InitializePage();
        }

        /// <summary>
        /// ��ʼ��ҳ��
        /// </summary>
        private void InitializePage()
        {
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

        /// <summary>
        /// ����Pivot�л��¼�
        /// </summary>
        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ������Ը��ݲ�ͬ��Pivotѡ���л���ͬ�Ĺ���������
            if (TopPivot.SelectedItem is PivotItem pi)
            {
                System.Diagnostics.Debug.WriteLine($"�л���: {pi.Header}");
            }
        }
    }
}