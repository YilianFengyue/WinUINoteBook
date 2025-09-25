using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media; // 为 VisualTreeHelper 提供命名空间

namespace App2.Pages
{
    public sealed partial class TestPage : Page
    {
        public ObservableCollection<TileItem> Tiles { get; } = new();

        // 全局“间距”与“尺寸”
        private double _tileGap = 6;
        // ★新增：拖拽过程里记录源索引
        private int _dragSourceIndex = -1;
        public TestPage()
        {
            InitializeComponent();
            Loaded += TestPage_Loaded;
        }

        private void TestPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Tiles.Count == 0) ResetSamples();
            ApplyItemSize(SizeSlider.Value);
            ApplyGap(_tileGap);
        }

        private void ResetSamples()
        {
            Tiles.Clear();

            // 精选一组彩色样张（picsum 固定 id，稳定且色彩丰富）
            // 你以后可替换为自己的 CDN / 封面图
            int[] ids = new int[]
            {
                1011, 1025, 1031, 1050, 1069, 1074, 1084, 1080, 1081, 1082,
                1083, 1085, 1089, 109, 110, 111, 112, 113, 114, 115
            };

            for (int i = 0; i < ids.Length; i++)
            {
                Tiles.Add(new TileItem
                {
                    Title = $"笔记 {i + 1}",
                    Subtitle = "双击打开 · 拖拽可排序",
                    ImageUrl = $"https://picsum.photos/id/{ids[i]}/800/800" // 更高分辨率，显示更清晰
                });
            }
        }

        private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ApplyItemSize(e.NewValue);
        }

        private void ApplyItemSize(double size)
        {
            if (TileGrid?.ItemsPanelRoot is VariableSizedWrapGrid panel)
            {
                panel.ItemWidth = size;
                panel.ItemHeight = size;
            }
        }
        private void ResizeTile_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🔧 ResizeTile_Click 被调用");

            if (sender is MenuFlyoutItem menuItem)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 MenuFlyoutItem Tag: {menuItem.Tag}");
                System.Diagnostics.Debug.WriteLine($"🔧 MenuFlyoutItem DataContext: {menuItem.DataContext?.GetType().Name}");

                if (menuItem.DataContext is TileItem tileItem && menuItem.Tag is string sizeTag)
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 找到 TileItem: {tileItem.Title}");
                    System.Diagnostics.Debug.WriteLine($"🔧 当前大小: {tileItem.ColumnSpan}x{tileItem.RowSpan}");

                    var sizes = sizeTag.Split(',');
                    if (sizes.Length == 2 &&
                        int.TryParse(sizes[0], out int colSpan) &&
                        int.TryParse(sizes[1], out int rowSpan))
                    {
                        System.Diagnostics.Debug.WriteLine($"🔧 准备改变大小到: {colSpan}x{rowSpan}");

                        // 更新磁贴大小
                        tileItem.ColumnSpan = colSpan;
                        tileItem.RowSpan = rowSpan;

                        System.Diagnostics.Debug.WriteLine($"🔧 大小已更新到: {tileItem.ColumnSpan}x{tileItem.RowSpan}");

                        // 应用到对应的容器
                        ApplyTileSize(tileItem);

                        // 额外的布局刷新确保变化立即可见
                        this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                        {
                            ForceGridLayoutRefresh();
                        });

                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"🔧 ❌ 解析大小失败: {sizeTag}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 ❌ DataContext 或 Tag 有问题");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔧 ❌ sender 不是 MenuFlyoutItem: {sender?.GetType().Name}");
            }
        }

        private void ApplyTileSize(TileItem tileItem)
        {
            int index = Tiles.IndexOf(tileItem);
            System.Diagnostics.Debug.WriteLine($"🔧 ApplyTileSize - 磁贴索引: {index}");

            if (index >= 0)
            {
                var container = TileGrid.ContainerFromIndex(index) as GridViewItem;
                System.Diagnostics.Debug.WriteLine($"🔧 找到容器: {container != null}");

                if (container != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 设置容器大小: {tileItem.ColumnSpan}x{tileItem.RowSpan}");

                    VariableSizedWrapGrid.SetRowSpan(container, tileItem.RowSpan);
                    VariableSizedWrapGrid.SetColumnSpan(container, tileItem.ColumnSpan);

                    // 更强制的布局刷新
                    if (TileGrid.ItemsPanelRoot is VariableSizedWrapGrid panel)
                    {
                        panel.InvalidateMeasure();
                        panel.InvalidateArrange();
                        panel.UpdateLayout(); // 关键：强制立即更新布局
                    }

                    container.InvalidateMeasure();
                    container.InvalidateArrange();
                    container.UpdateLayout(); // 关键：强制立即更新容器布局

                    TileGrid.InvalidateMeasure();
                    TileGrid.InvalidateArrange();
                    TileGrid.UpdateLayout(); // 关键：强制立即更新GridView布局

                    System.Diagnostics.Debug.WriteLine($"🔧 ✅ 布局已强制刷新");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 ❌ 容器未找到，尝试延迟应用");
                    // 使用 DispatcherQueue 确保在UI线程上执行
                    this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, () =>
                    {
                        ApplyTileSizeDelayed(tileItem, index);
                    });
                }
            }
        }

        private void ApplyTileSizeDelayed(TileItem tileItem, int index)
        {
            var delayedContainer = TileGrid.ContainerFromIndex(index) as GridViewItem;
            if (delayedContainer != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 延迟应用成功");
                VariableSizedWrapGrid.SetRowSpan(delayedContainer, tileItem.RowSpan);
                VariableSizedWrapGrid.SetColumnSpan(delayedContainer, tileItem.ColumnSpan);

                // 同样的强制布局更新
                if (TileGrid.ItemsPanelRoot is VariableSizedWrapGrid panel)
                {
                    panel.UpdateLayout();
                }
                delayedContainer.UpdateLayout();
                TileGrid.UpdateLayout();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔧 ❌ 延迟应用也失败了");
            }
        }

        private void GapSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _tileGap = e.NewValue;
            ApplyGap(_tileGap);
        }



        /// <summary>
        /// 按当前“间距滑杆”实时设置每个 GridViewItem 的 Margin。
        /// 官方样式里 Margin 是 Setter，无法绑定；因此直接在容器上赋值最稳妥。
        /// </summary>
        private void ApplyGap(double gap)
        {
            if (TileGrid == null) return;
            int count = Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                if (TileGrid.ContainerFromIndex(i) is GridViewItem gvi)
                {
                    gvi.Margin = new Thickness(gap);
                }
            }
        }

        // 新生成/回收容器时也应用当前间距
        private void TileGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer is GridViewItem gvi && args.Item is TileItem tileItem)
            {
                System.Diagnostics.Debug.WriteLine($"🔧 ContainerContentChanging - {tileItem.Title}: {tileItem.ColumnSpan}x{tileItem.RowSpan}");

                // 设置间距
                gvi.Margin = new Thickness(_tileGap);

                // 只在容器准备阶段设置大小，避免干扰拖拽
                if (args.Phase == 0)
                {
                    // 应用磁贴大小
                    VariableSizedWrapGrid.SetRowSpan(gvi, tileItem.RowSpan);
                    VariableSizedWrapGrid.SetColumnSpan(gvi, tileItem.ColumnSpan);

                    System.Diagnostics.Debug.WriteLine($"🔧 容器大小已设置: RowSpan={VariableSizedWrapGrid.GetRowSpan(gvi)}, ColumnSpan={VariableSizedWrapGrid.GetColumnSpan(gvi)}");
                }

                // 确保拖拽相关属性正确设置
                gvi.CanDrag = true;
                gvi.AllowDrop = true;
            }
        }

        public Visibility ContextualItem { get; } = Visibility.Visible;

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSamples();
            ApplyGap(_tileGap);
        }

        /// <summary>
        /// 强制刷新整个网格布局
        /// </summary>
        private void ForceGridLayoutRefresh()
        {
            if (TileGrid?.ItemsPanelRoot is VariableSizedWrapGrid panel)
            {
                // 临时改变一个无关紧要的属性来触发重新布局
                var currentOrientation = panel.Orientation;
                panel.Orientation = currentOrientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                panel.UpdateLayout();
                panel.Orientation = currentOrientation;
                panel.UpdateLayout();
            }
        }

        // ★新增：向上查找最近的 GridViewItem（不用扩展方法）
        private static GridViewItem FindAncestorGridViewItem(DependencyObject d)
        {
            while (d != null && d is not GridViewItem)
            {
                d = VisualTreeHelper.GetParent(d);
            }
            return d as GridViewItem;
        }

        private void TileGrid_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            // 仅支持单个条目拖拽（你也可扩展为多选）
            if (e.Items is { Count: > 0 })
            {
                var item = e.Items[0] as TileItem;
                _dragSourceIndex = Tiles.IndexOf(item);
                // 写入 DataPackage 供 Drop 端备用（可选）
                e.Data.SetText(_dragSourceIndex.ToString());
                e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            }
        }

        private void TileGrid_DragOver(object sender, DragEventArgs e)
        {
            // 告诉系统是 Move 语义，并允许放置
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }

        private void TileGrid_Drop(object sender, DragEventArgs e)
        {
            // 计算目标索引：通过命中容器确定目标位置
            if (_dragSourceIndex < 0) return;

            int targetIndex = Tiles.Count - 1;

            // 命中检测：找到鼠标下的容器
            if (e.OriginalSource is FrameworkElement fe)
            {
                // 一路向上找 GridViewItem
                var container = FindAncestorGridViewItem(fe);
                if (container != null)
                {
                    int idx = TileGrid.IndexFromContainer(container);
                    if (idx >= 0) targetIndex = idx;
                }
            }

            // 源与目标相同则忽略
            if (targetIndex == _dragSourceIndex) return;

            // 安全移动：考虑从前移到后/从后移到前的索引变化
            MoveItemInObservableCollection(Tiles, _dragSourceIndex, targetIndex);

            // 更新完后，重置状态并刷新当前间距即可
            _dragSourceIndex = -1;
            ApplyGap(_tileGap);         // 你的原方法
            ForceGridLayoutRefresh();   // 你的原方法，确保立刻重排
        }

        // ★新增：ObservableCollection 安全移动
        private static void MoveItemInObservableCollection<T>(ObservableCollection<T> collection, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            if (oldIndex < 0 || oldIndex >= collection.Count) return;
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= collection.Count) newIndex = collection.Count - 1;

            var item = collection[oldIndex];
            collection.RemoveAt(oldIndex);
            collection.Insert(newIndex, item);
        }

       
        
    }

    public class TileItem : System.ComponentModel.INotifyPropertyChanged
    {
        private int _rowSpan = 1;
        private int _columnSpan = 1;

        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string ImageUrl { get; set; } = "";

        public int RowSpan
        {
            get => _rowSpan;
            set
            {
                if (_rowSpan != value)
                {
                    _rowSpan = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SizeText));
                }
            }
        }

        public int ColumnSpan
        {
            get => _columnSpan;
            set
            {
                if (_columnSpan != value)
                {
                    _columnSpan = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SizeText));
                }
            }
        }

        public string SizeText => $"{ColumnSpan}x{RowSpan}";

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
