using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.DataTransfer;
using DevWinUI;
using System;
using System.Diagnostics;

namespace App2.Pages
{
    /// <summary>
    /// PPT页面，集成MainLandingPage控件和拖拽功能
    /// </summary>
    public sealed partial class PPTPage : Page
    {
        private int dragCounter = 0;

        public PPTPage()
        {
            this.InitializeComponent();
            Debug.WriteLine("🎯 PPTPage初始化完成");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                // 加载JSON数据
                mainLandingPage.GetDataAsync("DataModel/AppData.json");
                Debug.WriteLine("✅ JSON数据加载成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ JSON数据加载失败: {ex.Message}");
            }
        }

        private void mainLandingPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("🚀 MainLandingPage加载完成");
            ShowDragHint();
        }

        private void mainLandingPage_OnItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var args = (ItemClickEventArgs)e;
                var item = (DataItem)args.ClickedItem;

                Debug.WriteLine($"🔗 点击项目: {item.Title}");

                var dialog = new ContentDialog()
                {
                    Title = $"📋 {item.Title}",
                    Content = CreateItemInfoContent(item),
                    CloseButtonText = "关闭",
                    PrimaryButtonText = "查看详情",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ 导航错误: {ex.Message}");
            }
        }

        #region 拖拽功能实现

        private void HeaderTile_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            if (sender is FrameworkElement tile)
            {
                var title = GetTileTitle(tile);

                // 设置拖拽数据
                args.Data.Properties.Add("TileTitle", title);
                args.Data.Properties.Add("TileType", "HeaderTile");
                args.Data.Properties.Add("DragTime", DateTime.Now.ToString("HH:mm:ss"));

                args.Data.RequestedOperation = DataPackageOperation.Copy;

                Debug.WriteLine($"🎯 开始拖拽: {title}");
                ShowDragHint(true);
            }
        }

        private void HeaderTile_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey("TileTitle"))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "交换位置";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsGlyphVisible = true;
            }
        }

        private async void HeaderTile_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey("TileTitle"))
            {
                var draggedTitle = e.DataView.Properties["TileTitle"].ToString();
                var targetTitle = GetTileTitle(sender as FrameworkElement);

                Debug.WriteLine($"🔄 执行卡片交换: {draggedTitle} <-> {targetTitle}");

                var dialog = new ContentDialog()
                {
                    Title = "🔄 卡片交换",
                    Content = $"已将 '{draggedTitle}' 与 '{targetTitle}' 交换位置！\n\n" +
                             "这演示了WinUI3的拖拽功能：\n" +
                             "• DataPackage数据传输\n" +
                             "• 拖拽事件生命周期\n" +
                             "• 自定义拖拽UI效果",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();

                dragCounter++;
                UpdateDropStatus($"完成第 {dragCounter} 次拖拽操作！");
            }
        }

        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey("TileTitle"))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "放置到演示区域";
                e.DragUIOverride.IsCaptionVisible = true;

                if (sender is Border border)
                {
                    border.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.LightBlue)
                    { Opacity = 0.3 };
                }

                UpdateDropStatus("准备接收拖拽的内容...");
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.White)
                { Opacity = 0.2 };
            }

            UpdateDropStatus("将上面的卡片拖拽到这里试试！");
        }

        private async void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey("TileTitle"))
            {
                var title = e.DataView.Properties["TileTitle"].ToString();
                var dragTime = e.DataView.Properties["DragTime"].ToString();

                dragCounter++;

                Debug.WriteLine($"✅ 拖拽放置成功: {title} (第{dragCounter}次)");

                if (sender is Border border)
                {
                    border.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Green)
                    { Opacity = 0.3 };
                }

                UpdateDropStatus($"🎉 成功接收: {title}\n累计拖拽 {dragCounter} 次");

                var content = new StackPanel();
                content.Children.Add(new TextBlock
                {
                    Text = $"拖拽的卡片: {title}",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                });
                content.Children.Add(new TextBlock
                {
                    Text = $"拖拽时间: {dragTime}",
                    Margin = new Thickness(0, 0, 0, 10)
                });
                content.Children.Add(new TextBlock
                {
                    Text = $"这是第 {dragCounter} 次拖拽操作",
                    Margin = new Thickness(0, 0, 0, 10)
                });
                content.Children.Add(new TextBlock
                {
                    Text = "WinUI3拖拽功能特点:\n• 支持应用内拖拽\n• 支持跨应用拖拽\n• 自定义数据传输\n• 丰富的视觉反馈",
                    TextWrapping = TextWrapping.Wrap
                });

                var dialog = new ContentDialog()
                {
                    Title = "🎯 拖拽操作详情",
                    Content = content,
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();

                // 延迟恢复外观
                await System.Threading.Tasks.Task.Delay(1000);
                if (sender is Border b)
                {
                    b.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.White)
                    { Opacity = 0.2 };
                }
            }

            ShowDragHint(false);
        }

        #endregion

        #region 辅助方法

        private string GetTileTitle(FrameworkElement tile)
        {
            if (tile != null)
            {
                return tile.Tag?.ToString() ?? $"卡片_{tile.GetHashCode() % 1000}";
            }
            return "未知卡片";
        }

        private StackPanel CreateItemInfoContent(DataItem item)
        {
            var content = new StackPanel() { Spacing = 10 };

            content.Children.Add(new TextBlock
            {
                Text = $"📝 描述: {item.Subtitle}",
                TextWrapping = TextWrapping.Wrap
            });

            if (!string.IsNullOrEmpty(item.Description))
            {
                content.Children.Add(new TextBlock
                {
                    Text = $"ℹ️ 详情: {item.Description}",
                    TextWrapping = TextWrapping.Wrap
                });
            }

            content.Children.Add(new TextBlock
            {
                Text = $"🆔 唯一ID: {item.UniqueId}",
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                FontSize = 12
            });

            if (item.IsNew)
                content.Children.Add(new TextBlock { Text = "🆕 新功能", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green) });

            if (item.IsUpdated)
                content.Children.Add(new TextBlock { Text = "🔄 已更新", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange) });

            return content;
        }

        private void UpdateDropStatus(string status)
        {
            DropStatusText.Text = status;
        }

        private void ShowDragHint(bool isDragging = false)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation()
            {
                To = isDragging ? 1.0 : 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new QuadraticEase()
            };

            Storyboard.SetTarget(animation, DragHint);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        #endregion
    }
}