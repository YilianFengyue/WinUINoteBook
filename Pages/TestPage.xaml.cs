using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Windows.Foundation;

namespace App2.Pages
{
    public sealed partial class TestPage : Page
    {
        public ObservableCollection<TileItem> Tiles { get; } = new();

        private double _tileGap = 6;      // 间距
        private int _dragSourceIndex = -1;
        private TileItem? _dragItem = null;

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
            EnsureAllContainerSpans();     // ★初始时就同步一次跨度，避免容器复用造成差异
        }

        private void ResetSamples()
        {
            Tiles.Clear();

            int[] ids =
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
                    ImageUrl = $"https://picsum.photos/id/{ids[i]}/800/800",
                    // 演示：前几个给不同尺寸
                    ColumnSpan = (i == 0) ? 2 : 1,
                    RowSpan = (i == 0) ? 2 : 1
                });
            }
        }

        private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
            => ApplyItemSize(e.NewValue);

        private void ApplyItemSize(double size)
        {
            if (TileGrid?.ItemsPanelRoot is VariableSizedWrapGrid panel)
            {
                panel.ItemWidth = size;
                panel.ItemHeight = size;
            }
        }

        private void GapSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _tileGap = e.NewValue;
            ApplyGap(_tileGap);
        }

        /// <summary>按当前“间距滑杆”设置每个 GridViewItem 的 Margin。</summary>
        private void ApplyGap(double gap)
        {
            if (TileGrid == null) return;
            int count = Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                if (TileGrid.ContainerFromIndex(i) is GridViewItem gvi)
                    gvi.Margin = new Thickness(gap);
            }
        }

        /// <summary>容器变化时，始终同步跨度（不要限制 Phase）。</summary>
        private void TileGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer is GridViewItem gvi && args.Item is TileItem tile)
            {
                // 间距
                gvi.Margin = new Thickness(_tileGap);

                // ★关键：每次都应用跨度，避免容器复用导致 1x1 回退
                VariableSizedWrapGrid.SetRowSpan(gvi, tile.RowSpan);
                VariableSizedWrapGrid.SetColumnSpan(gvi, tile.ColumnSpan);
            }
        }

        /// <summary>把当前已实现的容器全部与数据项的 Row/ColSpan 对齐。</summary>
        private void EnsureAllContainerSpans()
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (TileGrid.ContainerFromIndex(i) is GridViewItem gvi)
                {
                    var t = Tiles[i];
                    VariableSizedWrapGrid.SetRowSpan(gvi, t.RowSpan);
                    VariableSizedWrapGrid.SetColumnSpan(gvi, t.ColumnSpan);
                }
            }

            if (TileGrid.ItemsPanelRoot is VariableSizedWrapGrid panel)
            {
                panel.InvalidateMeasure();
                panel.InvalidateArrange();
                panel.UpdateLayout();
            }
        }

        // ====== 尺寸右键菜单 ======

        private void ResizeTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mi &&
                mi.DataContext is TileItem tile &&
                mi.Tag is string s)
            {
                var parts = s.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int c) &&
                    int.TryParse(parts[1], out int r))
                {
                    tile.ColumnSpan = c;
                    tile.RowSpan = r;

                    // 立刻同步到容器
                    var idx = Tiles.IndexOf(tile);
                    if (idx >= 0 && TileGrid.ContainerFromIndex(idx) is GridViewItem gvi)
                    {
                        VariableSizedWrapGrid.SetRowSpan(gvi, tile.RowSpan);
                        VariableSizedWrapGrid.SetColumnSpan(gvi, tile.ColumnSpan);
                    }
                    EnsureAllContainerSpans();
                }
            }
        }

        // ====== 手写拖放重排 ======

        private void TileGrid_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items is { Count: > 0 })
            {
                _dragItem = e.Items[0] as TileItem;
                _dragSourceIndex = (_dragItem != null) ? Tiles.IndexOf(_dragItem) : -1;
                e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            }
        }

        private void TileGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }

        private void TileGrid_Drop(object sender, DragEventArgs e)
        {
            if (_dragSourceIndex < 0 || _dragItem is null) return;

            // ★关键：根据鼠标位置精确计算插入索引（左半插前，右半插后；未命中容器按行末或最后）
            int targetIndex = GetTargetInsertIndex(e.GetPosition(TileGrid));

            // 若把 A 从前面拖到后面，移除后目标索引会左移 1，需修正
            if (targetIndex > _dragSourceIndex) targetIndex--;

            // 边界保护
            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex > Tiles.Count - 1) targetIndex = Tiles.Count - 1;

            MoveItem(Tiles, _dragSourceIndex, targetIndex);

            _dragSourceIndex = -1;
            _dragItem = null;

            // 拖拽完成后，确保所有容器的跨度与数据同步，不出现“变小”
            EnsureAllContainerSpans();
        }

        /// <summary>
        /// 计算插入索引：命中某个容器则看左右半（插前/插后）；未命中容器则依据行/末尾推断。
        /// </summary>
        private int GetTargetInsertIndex(Point p)
        {
            int lastRealized = -1;
            Rect lastBounds = default;

            for (int i = 0; i < Tiles.Count; i++)
            {
                if (TileGrid.ContainerFromIndex(i) is GridViewItem gvi)
                {
                    var bounds = GetBoundsRelativeTo(gvi, TileGrid);
                    lastRealized = i;
                    lastBounds = bounds;

                    if (bounds.Contains(p))
                    {
                        // 命中该卡片：左半插到它前，右半插到它后
                        bool insertBefore = p.X < (bounds.Left + bounds.Width / 2);
                        return insertBefore ? i : i + 1;
                    }
                }
            }

            // 没命中任何可视容器：若在最后一行右侧，插到最后；若在最后一行左侧，插到最后一行前面
            if (lastRealized >= 0)
            {
                if (p.Y < lastBounds.Top)       // 在第一行上方，插到最前
                    return 0;
                return Tiles.Count;             // 其它情况视为插到最后
            }

            // 没有任何容器（空列表）
            return 0;
        }

        private static Rect GetBoundsRelativeTo(FrameworkElement child, FrameworkElement root)
        {
            var t = child.TransformToVisual(root);
            return t.TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
        }

        private static void MoveItem<T>(ObservableCollection<T> coll, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            if (oldIndex < 0 || oldIndex >= coll.Count) return;
            if (newIndex < 0) newIndex = 0;
            if (newIndex > coll.Count) newIndex = coll.Count;

            var item = coll[oldIndex];
            coll.RemoveAt(oldIndex);
            // newIndex 是“插入位置”，等价于 Insert(i) 把元素放在 i 之前
            if (newIndex >= coll.Count) coll.Add(item);
            else coll.Insert(newIndex, item);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSamples();
            ApplyItemSize(SizeSlider.Value);
            ApplyGap(_tileGap);
            EnsureAllContainerSpans(); // 确保跨度也重置
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
            set { if (_rowSpan != value) { _rowSpan = value; OnPropertyChanged(); OnPropertyChanged(nameof(SizeText)); } }
        }

        public int ColumnSpan
        {
            get => _columnSpan;
            set { if (_columnSpan != value) { _columnSpan = value; OnPropertyChanged(); OnPropertyChanged(nameof(SizeText)); } }
        }

        public string SizeText => $"{ColumnSpan}x{RowSpan}";

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
