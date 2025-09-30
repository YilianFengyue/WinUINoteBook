using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

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
            if (EditorWebView == null) return;

            EditorWebView.NavigationCompleted += EditorWebView_NavigationCompleted;

            // ȷ�� CoreWebView2 ��ʼ��
            await EditorWebView.EnsureCoreWebView2Async();

            // ����ű�������
            EditorWebView.CoreWebView2.Settings.IsScriptEnabled = true;
            EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;

            // ��������ǰ�˵���Ϣ���� JSON ͨ����
            EditorWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            Debug.WriteLine("WebView2 ��ʼ���ɹ�����Ϣ������������");
        }

        private void EditorWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Debug.WriteLine("TipTap Editor ���سɹ�");

                // ��ǰ��һ��ʱ���ʼ����Ȼ����һ����ͨ�Բ���
                _ = Task.Delay(1000).ContinueWith(_ =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        // ��ֱ�ӷ�����ȴ� ready��ֻ����־
                        Debug.WriteLine("�ȴ�ǰ�� ready...");
                    });
                });
            }
        }

        /// <summary>�������� Vue ����Ϣ��ʹ�� WebMessageAsJson �������/�ַ�����</summary>
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson; // ͳһʹ�� JSON �ַ���
                var message = JsonSerializer.Deserialize<EditorMessage>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
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

                case "ping":
                    Debug.WriteLine("�յ� ping");
                    break;
            }
        }

        private void OnEditorReady(JsonElement payload)
        {
            isEditorReady = true;

            try
            {
                var version = payload.GetProperty("version").GetString();
                var commandCount = payload.GetProperty("supportedCommands").GetArrayLength();
                Debug.WriteLine($"�༭������ - �汾: {version}, ֧��������: {commandCount}");
            }
            catch { /* ���Խ����쳣 */ }

            DispatcherQueue.TryEnqueue(() =>
            {
                SavedText.Text = "�Ѿ���";
            });
        }

        private void OnContentChanged(JsonElement payload)
        {
            try
            {
                currentContent = payload.GetProperty("html").GetString();
                var words = payload.GetProperty("words").GetInt32();
                var characters = payload.GetProperty("characters").GetInt32();
                var isEmpty = payload.GetProperty("isEmpty").GetBoolean();

                Debug.WriteLine($"���ݱ仯 - ����: {words}, �ַ���: {characters}, ��: {isEmpty}");

                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateStatusBar(words, characters, isEmpty);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"���� content-changed ʧ��: {ex.Message}");
            }
        }

        private void OnSelectionChanged(JsonElement payload)
        {
            try
            {
                var from = payload.GetProperty("from").GetInt32();
                var to = payload.GetProperty("to").GetInt32();
                var activeFormats = payload.GetProperty("activeFormats");

                var isBold = activeFormats.TryGetProperty("bold", out var b) && b.GetBoolean();
                var isItalic = activeFormats.TryGetProperty("italic", out var i) && i.GetBoolean();

                Debug.WriteLine($"ѡ���仯 - λ��: {from}-{to}, ����: {isBold}, б��: {isItalic}");

                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateToolbarState(activeFormats);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"���� selection-changed ʧ��: {ex.Message}");
            }
        }

        private void OnEditorError(JsonElement payload)
        {
            var message = payload.TryGetProperty("message", out var m) ? m.GetString() : "(no message)";
            Debug.WriteLine($"�༭������: {message}");
            DispatcherQueue.TryEnqueue(() =>
            {
                SavedText.Text = $"����{message}";
            });
        }

        /// <summary>��������༭����JSON ͨ����</summary>
        private async Task SendEditorCommand(string command, object payload = null)
        {
            if (!isEditorReady)
            {
                Debug.WriteLine("�༭��δ�������޷���������");
                return;
            }

            var msg = new
            {
                type = "editor-command",
                command,
                payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var json = JsonSerializer.Serialize(msg);

            try
            {
                EditorWebView.CoreWebView2.PostWebMessageAsJson(json);
                Debug.WriteLine($"��������: {command}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"��������ʧ��: {ex.Message}");
            }
        }

        // ========== �������¼�����ȫ�����ϣ� ==========

        // ������ʽ
        private async void OnBold(object sender, RoutedEventArgs e) => await SendEditorCommand("bold");
        private async void OnItalic(object sender, RoutedEventArgs e) => await SendEditorCommand("italic");
        private async void OnUnderline(object sender, RoutedEventArgs e) => await SendEditorCommand("underline");
        private async void OnStrike(object sender, RoutedEventArgs e) => await SendEditorCommand("strike");
        private async void OnCode(object sender, RoutedEventArgs e) => await SendEditorCommand("code");

        // ���� / ����
        private async void OnParagraph(object sender, RoutedEventArgs e) => await SendEditorCommand("paragraph");

        private async void HeadingLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeadingLevelCombo.SelectedIndex < 0) return;
            var level = HeadingLevelCombo.SelectedIndex + 1; // 1..6
            await SendEditorCommand($"heading{level}");
        }

        // ����
        private async void OnAlignLeft(object sender, RoutedEventArgs e) => await SendEditorCommand("alignLeft");
        private async void OnAlignCenter(object sender, RoutedEventArgs e) => await SendEditorCommand("alignCenter");
        private async void OnAlignRight(object sender, RoutedEventArgs e) => await SendEditorCommand("alignRight");
        private async void OnAlignJustify(object sender, RoutedEventArgs e) => await SendEditorCommand("alignJustify");

        // �б�
        private async void OnBulletList(object sender, RoutedEventArgs e) => await SendEditorCommand("bulletList");
        private async void OnOrderedList(object sender, RoutedEventArgs e) => await SendEditorCommand("orderedList");
        private async void OnTaskList(object sender, RoutedEventArgs e) => await SendEditorCommand("taskList");

        // ������ע�⣺TipTap ����δ������indent/outdent ������ sink/lift �Ե���
        private async void OnIndent(object sender, RoutedEventArgs e) => await SendEditorCommand("indent");
        private async void OnOutdent(object sender, RoutedEventArgs e) => await SendEditorCommand("outdent");

        // ����
        private async void OnInsertLink(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertLink", new { url = "https://example.com", text = "ʾ������" });
        }
        private async void OnInsertImage(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertImage", new { src = "https://via.placeholder.com/300x200", alt = "ʾ��ͼƬ" });
        }
        private async void OnInsertVideo(object sender, RoutedEventArgs e)
        {
            await SendEditorCommand("insertVideo", new { src = "https://www.w3schools.com/html/mov_bbb.mp4" });
        }
        private async void OnInsertTable(object sender, RoutedEventArgs e) => await SendEditorCommand("insertTable"); // ��ȷ��ǰ��ʵ��

        // ����Ԫ�� / ����� / �ָ��� / ����
        private async void OnBlockquote(object sender, RoutedEventArgs e) => await SendEditorCommand("insertBlockquote");
        private async void OnHorizontalRule(object sender, RoutedEventArgs e) => await SendEditorCommand("insertHorizontalRule");
        private async void OnCodeBlock(object sender, RoutedEventArgs e) => await SendEditorCommand("insertCodeBlock");

        // ��ʽ / ��ʷ
        private async void OnClearFormat(object sender, RoutedEventArgs e) => await SendEditorCommand("clearFormat");
        private async void OnUndo(object sender, RoutedEventArgs e) => await SendEditorCommand("undo");
        private async void OnRedo(object sender, RoutedEventArgs e) => await SendEditorCommand("redo");

        // ��ͼ
        private async void OnToggleFullscreen(object sender, RoutedEventArgs e) => await SendEditorCommand("toggleFullscreen");

        // ʾ������ɫ/������ǰ����ʵ�� setColor / setHighlight��
        private async void OnSetColorDemo(object sender, RoutedEventArgs e)
            => await SendEditorCommand("setColor", new { color = "#ff4d4f" });
        private async void OnSetHighlightDemo(object sender, RoutedEventArgs e)
            => await SendEditorCommand("setHighlight", new { color = "#ffe58f" });

        // ========== UI ���·�������ѡ���ƣ� ==========

        private void UpdateStatusBar(int words, int characters, bool isEmpty)
        {
            WordsText.Text = $"����: {words}";
            CharsText.Text = $"�ַ�: {characters}";
            SavedText.Text = isEmpty ? "���ĵ�" : "�Ѹ���";
        }

        private void UpdateToolbarState(JsonElement activeFormats)
        {
            try
            {
                if (BtnBold != null && activeFormats.TryGetProperty("bold", out var b))
                    BtnBold.IsChecked = b.GetBoolean();

                if (BtnItalic != null && activeFormats.TryGetProperty("italic", out var i))
                    BtnItalic.IsChecked = i.GetBoolean();

                if (BtnUnderline != null && activeFormats.TryGetProperty("underline", out var u))
                    BtnUnderline.IsChecked = u.GetBoolean();

                if (BtnStrike != null && activeFormats.TryGetProperty("strike", out var s))
                    BtnStrike.IsChecked = s.GetBoolean();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateToolbarState error: " + ex);
            }
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
