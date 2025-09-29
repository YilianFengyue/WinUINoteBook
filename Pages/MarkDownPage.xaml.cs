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

                // 确保 CoreWebView2 初始化
                await EditorWebView.EnsureCoreWebView2Async();

                // 设置消息监听器
                EditorWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                // 允许脚本访问
                EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = true; // 开发阶段启用
                EditorWebView.CoreWebView2.Settings.IsScriptEnabled = true;

                Debug.WriteLine("WebView2 初始化成功，消息监听器已设置");
            }
        }

        private void EditorWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Debug.WriteLine("TipTap Editor 加载成功");

                // 延迟等待编辑器初始化
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
        /// 处理来自 Vue 的消息
        /// </summary>
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // 关键：无论网页发的是对象还是字符串，这里都拿 JSON 字符串
                var json = e.WebMessageAsJson;

                var message = JsonSerializer.Deserialize<EditorMessage>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true } // 小写字段也能匹配
                );

                if (message == null)
                {
                    Debug.WriteLine("收到空消息或反序列化失败");
                    return;
                }

                Debug.WriteLine($"收到消息: {message.Type} - {message.Event}");
                HandleEditorMessage(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理消息失败: {ex}");
            }
        }

        /// <summary>
        /// 处理编辑器消息
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
                    Debug.WriteLine("编辑器获得焦点");
                    break;

                case "blur":
                    Debug.WriteLine("编辑器失去焦点");
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

            Debug.WriteLine($"编辑器就绪 - 版本: {version}, 支持命令数: {commandCount}");

            // 更新 UI 状态
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // 可以在这里更新状态栏或其他 UI 元素
            });
        }

        private void OnContentChanged(JsonElement payload)
        {
            currentContent = payload.GetProperty("html").GetString();
            var words = payload.GetProperty("words").GetInt32();
            var characters = payload.GetProperty("characters").GetInt32();
            var isEmpty = payload.GetProperty("isEmpty").GetBoolean();

            Debug.WriteLine($"内容变化 - 字数: {words}, 字符数: {characters}, 空: {isEmpty}");

            // 更新状态栏
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

            Debug.WriteLine($"选区变化 - 位置: {from}-{to}, 粗体: {isBold}, 斜体: {isItalic}");

            // 更新工具栏按钮状态
            this.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateToolbarState(activeFormats);
            });
        }

        private void OnEditorError(JsonElement payload)
        {
            var message = payload.GetProperty("message").GetString();
            Debug.WriteLine($"编辑器错误: {message}");
        }

        /// <summary>
        /// 发送命令到编辑器
        /// </summary>
        private async Task SendEditorCommand(string command, object payload = null)
        {
            if (!isEditorReady)
            {
                Debug.WriteLine("编辑器未就绪，无法发送命令");
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
                // 使用 PostWebMessageAsString 而不是 PostWebMessageAsJsonAsync
                EditorWebView.CoreWebView2.PostWebMessageAsJson(json);
                Debug.WriteLine($"发送命令: {command}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发送命令失败: {ex.Message}");
            }
        }

        // ========== 工具栏事件处理 ==========

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
            // 可以弹出对话框获取链接信息
            await SendEditorCommand("insertLink", new { url = "https://example.com", text = "示例链接" });
        }

        private async void OnInsertImage(object sender, RoutedEventArgs e)
        {
            // 可以弹出文件选择器
            await SendEditorCommand("insertImage", new { src = "https://via.placeholder.com/300x200", alt = "示例图片" });
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

        // ========== UI 更新方法 ==========

        private void UpdateStatusBar(int words, int characters, bool isEmpty)
        {
            // 更新状态栏显示
            // 需要在 XAML 中添加对应的 TextBlock 引用
        }

        private void UpdateToolbarState(JsonElement activeFormats)
        {
            // 根据激活状态更新工具栏按钮
            // 可以改变按钮的 IsChecked 状态
        }

        // ========== 测试方法 ==========

        private async void TestBridgeConnection()
        {
            Debug.WriteLine("开始测试桥接连接...");

            // 等待一段时间再测试
            await Task.Delay(1000);

            // 测试基础命令
            await SendEditorCommand("bold");
            await Task.Delay(500);
            await SendEditorCommand("italic");
            await Task.Delay(500);
            await SendEditorCommand("bold"); // 取消粗体

            Debug.WriteLine("桥接测试完成");
        }

        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopPivot.SelectedItem is PivotItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();
                Debug.WriteLine($"切换到标签: {tag}");
            }
        }

        // ========== 数据类 ==========

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