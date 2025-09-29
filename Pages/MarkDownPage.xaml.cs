using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;

namespace App2.Pages
{
    public sealed partial class MarkDownPage : Page
    {
        private bool isEditorReady = false;
        private string currentContent = "";
        private EditorStatus editorStatus = new();

        public MarkDownPage()
        {
            this.InitializeComponent();
            InitializeEditor();
        }

        private async void InitializeEditor()
        {
            if (EditorWebView != null)
            {
                EditorWebView.NavigationCompleted += EditorWebView_NavigationCompleted;

                // ȷ�� CoreWebView2 ��ʼ��
                await EditorWebView.EnsureCoreWebView2Async();

                // ������Ϣ������
                EditorWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                // ����ű�����
                EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = true; // �����׶�����
                EditorWebView.CoreWebView2.Settings.IsScriptEnabled = true;

                Debug.WriteLine("WebView2 ��ʼ���ɹ�����Ϣ������������");
            }
        }

        private void EditorWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Debug.WriteLine("TipTap Editor ���سɹ�");

                // �ӳٵȴ��༭����ʼ��
                Task.Delay(2000).ContinueWith(_ =>
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        TestBridgeConnection();
                    });
                });
            }
        }

        /// <summary>
        /// �������� Vue ����Ϣ
        /// </summary>
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // �ؼ���������ҳ�����Ƕ������ַ��������ﶼ�� JSON �ַ���
                var json = e.WebMessageAsJson;

                var message = JsonSerializer.Deserialize<EditorMessage>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true } // Сд�ֶ�Ҳ��ƥ��
                );

                if (message == null)
                {
                    Debug.WriteLine("�յ�����Ϣ�����л�ʧ��");
                    return;
                }

                Debug.WriteLine($"�յ���Ϣ: {message.Type} - {message.Event}");
                HandleEditorMessage(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������Ϣʧ��: {ex}");
            }
        }

        /// <summary>
        /// ����༭����Ϣ
        /// </summary>
        private void HandleEditorMessage(EditorMessage message)
        {
            switch (message.Event)
            {
                case "ready":
                    OnEditorReady(message.Payload);
                    break;

                case "content-changed":
                    OnContentChanged(message.Payload);
                    break;

                case "selection-changed":
                    OnSelectionChanged(message.Payload);
                    break;

                case "focus":
                    Debug.WriteLine("�༭����ý���");
                    break;

                case "blur":
                    Debug.WriteLine("�༭��ʧȥ����");
                    break;

                case "error":
                    OnEditorError(message.Payload);
                    break;
            }
        }

        private void OnEditorReady(JsonElement payload)
        {
            isEditorReady = true;
            var version = payload.GetProperty("version").GetString();
            var commandCount = payload.GetProperty("supportedCommands").GetArrayLength();

            Debug.WriteLine($"�༭������ - �汾: {version}, ֧��������: {commandCount}");

            // ���� UI ״̬
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // �������������״̬�������� UI Ԫ��
            });
        }

        private void OnContentChanged(JsonElement payload)
        {
            currentContent = payload.GetProperty("html").GetString();
            var words = payload.GetProperty("words").GetInt32();
            var characters = payload.GetProperty("characters").GetInt32();
            var isEmpty = payload.GetProperty("isEmpty").GetBoolean();

            Debug.WriteLine($"���ݱ仯 - ����: {words}, �ַ���: {characters}, ��: {isEmpty}");

            // ����״̬��
            this.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateStatusBar(words, characters, isEmpty);
            });
        }

        private void OnSelectionChanged(JsonElement payload)
        {
            var from = payload.GetProperty("from").GetInt32();
            var to = payload.GetProperty("to").GetInt32();
            var activeFormats = payload.GetProperty("activeFormats");

            var isBold = activeFormats.GetProperty("bold").GetBoolean();
            var isItalic = activeFormats.GetProperty("italic").GetBoolean();

            Debug.WriteLine($"ѡ���仯 - λ��: {from}-{to}, ����: {isBold}, б��: {isItalic}");

            // ���¹�������ť״̬
            this.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateToolbarState(activeFormats);
            });
        }

        private void OnEditorError(JsonElement payload)
        {
            var message = payload.GetProperty("message").GetString();
            Debug.WriteLine($"�༭������: {message}");
        }

        /// <summary>
        /// ��������༭��
        /// </summary>
        private async Task SendEditorCommand(string command, object payload = null)
        {
            if (!isEditorReady)
            {
                Debug.WriteLine("�༭��δ�������޷���������");
                return;
            }

            var message = new
            {
                type = "editor-command",
                command = command,
                payload = payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var json = JsonSerializer.Serialize(message);

            try
            {
                // ʹ�� PostWebMessageAsString ������ PostWebMessageAsJsonAsync
                EditorWebView.CoreWebView2.PostWebMessageAsJson(json);
                Debug.WriteLine($"��������: {command}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"��������ʧ��: {ex.Message}");
            }
        }

        // ========== �������¼����� ==========

        private async void OnBold(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("bold");
        }

        private async void OnItalic(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("italic");
        }

        private async void OnUnderline(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("underline");
        }

        private async void OnStrike(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("strike");
        }

        private async void OnCode(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("code");
        }

        private async void OnHeading1(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("heading1");
        }

        private async void OnHeading2(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("heading2");
        }

        private async void OnParagraph(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("paragraph");
        }

        private async void OnAlignLeft(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("alignLeft");
        }

        private async void OnAlignCenter(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("alignCenter");
        }

        private async void OnAlignRight(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("alignRight");
        }

        private async void OnBulletList(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("bulletList");
        }

        private async void OnOrderedList(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("orderedList");
        }

        private async void OnTaskList(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("taskList");
        }

        private async void OnInsertLink(object sender, RoutedEventArgs e)
        {
            // ���Ե����Ի����ȡ������Ϣ
            await SendEditorCommand("insertLink", new { url = "https://example.com", text = "ʾ������" });
        }

        private async void OnInsertImage(object sender, RoutedEventArgs e)
        {
            // ���Ե����ļ�ѡ����
            await SendEditorCommand("insertImage", new { src = "https://via.placeholder.com/300x200", alt = "ʾ��ͼƬ" });
        }

        private async void OnInsertTable(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertTable");
        }

        private async void OnBlockquote(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertBlockquote");
        }

        private async void OnHorizontalRule(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertHorizontalRule");
        }

        private async void OnCodeBlock(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertCodeBlock");
        }

        private async void OnClearFormat(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("clearFormat");
        }

        private async void OnUndo(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("undo");
        }

        private async void OnRedo(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("redo");
        }

        // ========== UI ���·��� ==========

        private void UpdateStatusBar(int words, int characters, bool isEmpty)
        {
            // ����״̬����ʾ
            // ��Ҫ�� XAML ����Ӷ�Ӧ�� TextBlock ����
        }

        private void UpdateToolbarState(JsonElement activeFormats)
        {
            // ���ݼ���״̬���¹�������ť
            // ���Ըı䰴ť�� IsChecked ״̬
        }

        // ========== ���Է��� ==========

        private async void TestBridgeConnection()
        {
            Debug.WriteLine("��ʼ�����Ž�����...");

            // �ȴ�һ��ʱ���ٲ���
            await Task.Delay(1000);

            // ���Ի�������
            await SendEditorCommand("bold");
            await Task.Delay(500);
            await SendEditorCommand("italic");
            await Task.Delay(500);
            await SendEditorCommand("bold"); // ȡ������

            Debug.WriteLine("�ŽӲ������");
        }

        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopPivot.SelectedItem is PivotItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();
                Debug.WriteLine($"�л�����ǩ: {tag}");
            }
        }

        // ========== ������ ==========

        public class EditorMessage
        {
            public string Type { get; set; }
            public string Event { get; set; }
            public JsonElement Payload { get; set; }
            public long Timestamp { get; set; }
        }

        public class EditorStatus
        {
            public bool IsReady { get; set; }
            public int WordCount { get; set; }
            public int CharacterCount { get; set; }
            public bool IsEmpty { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
        }
    }
}