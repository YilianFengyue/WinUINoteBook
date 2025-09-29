// ★新增（放在其它 using 之后）
using App2.Models; // 引用 TileKind / FileKind 枚举
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
// ★新增 using
using Windows.Storage;
using Windows.System;

using Windows.Storage.Pickers;

using WinRT.Interop;  // InitializeWithWindow.Initialize
using System.IO;
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

        // ★替换：ResetSamples（加入 Image / Note / File 各类示例）
        private void ResetSamples()
        {
            Tiles.Clear();

            // 先放一个 2x2 的图片卡
            Tiles.Add(new TileItem
            {
                Kind = TileKind.Image,
                Title = "示例图片 · 封面",
                Subtitle = "双击打开大图",
                ImageUrl = "https://picsum.photos/id/1011/1200/1200",
                ColumnSpan = 2,
                RowSpan = 2
            });

            // 放一个薄亚克力笔记卡
            Tiles.Add(new TileItem
            {
                Kind = TileKind.Note,
                Title = "待办清单",
                Subtitle = "薄亚克力便签（可编辑）",
                NoteText = "· 完成 PDF 页码跳转\n· 文件夹导入磁贴\n· 工作区 JSON 持久化",
                ColumnSpan = 1,
                RowSpan = 1
            });

            // 放一个文件卡（占位：路径可后续从导入功能带入）
            Tiles.Add(new TileItem
            {
                Kind = TileKind.File,
                FileKind = FileKind.Pdf,
                Title = "算法讲义（PDF）",
                Subtitle = "PDF · 第 12 页（占位）",
                FilePath = null,      // 现在没有真实文件路径
                PdfPage = 12,
                ColumnSpan = 1,
                RowSpan = 1
            });

            // 其余继续用你原来的图片示例（演示布局/拖拽）
            int[] ids =
            {
        1025, 1031, 1050, 1069, 1074, 1084, 1080, 1081, 1082,
        1083, 1085, 1089, 109, 1100, 1110, 1120, 1130, 1140, 1150
    };
            for (int i = 0; i < ids.Length; i++)
            {
                Tiles.Add(new TileItem
                {
                    Kind = TileKind.Image,
                    Title = $"图片 {i + 1}",
                    Subtitle = "双击打开 · 可拖拽排序",
                    ImageUrl = $"https://picsum.photos/id/{ids[i]}/800/800",
                    ColumnSpan = 1,
                    RowSpan = 1
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

        // ★新增：磁贴点击占位路由（A 步先用 ContentDialog）
        private async void TileGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not TileItem tile) return;
            await OpenTileAsync(tile);
        }

        // ★新增：根据类型给出占位行为（B 步再接入内部预览导航）
        private async Task OpenTileAsync(TileItem tile)
        {
            // WinUI 3 的 ContentDialog 需要设置 XamlRoot（官方建议）
            // 参考：ContentDialog 文档
            var dlg = new ContentDialog
            {
                XamlRoot = this.Content.XamlRoot,
                Title = tile.Title,
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            };

            switch (tile.Kind)
            {
                case TileKind.Image:
                    {
                        var img = new Image
                        {
                            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
                            MaxWidth = 1200,
                            Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                                new Uri(tile.ImagePathOrUrl ?? tile.ImageUrl))
                        };
                        dlg.Content = new ScrollViewer { Content = img };
                        break;
                    }
                case TileKind.Note:
                    {
                        var box = new TextBox
                        {
                            AcceptsReturn = true,
                            TextWrapping = TextWrapping.Wrap,
                            MinWidth = 420,
                            MinHeight = 240,
                            Text = tile.NoteText ?? ""
                        };
                        dlg.Content = box;
                        dlg.SecondaryButtonText = "保存";
                        dlg.CloseButtonText = "取消";

                        var result = await dlg.ShowAsync();
                        if (result == ContentDialogResult.Secondary)
                        {
                            tile.NoteText = box.Text;
                        }
                        return; // 已经 Show 过
                    }
                case TileKind.File:
                    {
                        if (!string.IsNullOrWhiteSpace(tile.FilePath))
                        {
                            if (tile.FileKind == FileKind.Pdf)
                            {
                                var args = new OpenArgs
                                {
                                    Kind = TileKind.File,
                                    FileKind = FileKind.Pdf,
                                    LocalPath = tile.FilePath
                                    // ★不传 Page（你说不要）
                                };
                                Frame.Navigate(typeof(PdfViewerPage), args);
                                return;
                            }
                            // 其它类型：默认外部打开
                            try
                            {
                                var file = await StorageFile.GetFileFromPathAsync(tile.FilePath);
                                await Launcher.LaunchFileAsync(file);
                            }
                            catch { /* 可选提示 */ }
                        }
                        else
                        {
                            var tip = new ContentDialog
                            {
                                XamlRoot = this.Content.XamlRoot,
                                Title = "未设置路径",
                                Content = "请先为该文件卡设置文件路径。",
                                PrimaryButtonText = "知道了"
                            };
                            await tip.ShowAsync();
                        }
                        return;
                    }
                default:
                    dlg.Content = new TextBlock { Text = "未知类型" };
                    break;
            }

            await dlg.ShowAsync();
        }

        // ★工具：获取窗口句柄（WinUI3 的 FileOpenPicker 需要）
        private IntPtr GetHwnd()
        {
            return App.m_window is not null
                ? WindowNative.GetWindowHandle(App.m_window)
                : IntPtr.Zero;
        }

        // ★添加：文件卡（PDF/Markdown）
        private async void AddFileTile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            var hwnd = GetHwnd();
            if (hwnd != IntPtr.Zero) InitializeWithWindow.Initialize(picker, hwnd);
            picker.FileTypeFilter.Add(".pdf");
            picker.FileTypeFilter.Add(".md");
            picker.FileTypeFilter.Add(".markdown");

            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            var ext = Path.GetExtension(file.Path).ToLowerInvariant();
            var kind = ext == ".pdf" ? FileKind.Pdf : FileKind.Markdown;

            Tiles.Insert(0, new TileItem
            {
                Kind = TileKind.File,
                FileKind = kind,
                Title = file.Name,
                Subtitle = (kind == FileKind.Pdf) ? "PDF 文件" : "Markdown 文件",
                FilePath = file.Path,
                ColumnSpan = 1,
                RowSpan = 1
            });
            EnsureAllContainerSpans();
        }

        // ★添加：图片卡
        private async void AddImageTile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            var hwnd = GetHwnd();
            if (hwnd != IntPtr.Zero) InitializeWithWindow.Initialize(picker, hwnd);
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");

            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            Tiles.Insert(0, new TileItem
            {
                Kind = TileKind.Image,
                Title = file.Name,
                Subtitle = "图片",
                ImagePathOrUrl = file.Path,
                ColumnSpan = 1,
                RowSpan = 1
            });
            EnsureAllContainerSpans();
        }

        // ★添加：笔记卡
        private void AddNoteTile_Click(object sender, RoutedEventArgs e)
        {
            Tiles.Insert(0, new TileItem
            {
                Kind = TileKind.Note,
                Title = "新建便签",
                Subtitle = "薄亚克力便签（可编辑）",
                NoteText = "输入内容…",
                ColumnSpan = 1,
                RowSpan = 1
            });
            EnsureAllContainerSpans();
        }

        // ★右键：删除当前卡片（Image/Note/File 通用）
        private void DeleteTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem mi) return;
            if (mi.DataContext is not TileItem tile) return;
            Tiles.Remove(tile);
            EnsureAllContainerSpans();
        }

        // ★右键（文件卡）：使用系统打开
        private async void UseSystemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem mi) return;
            if (mi.DataContext is not TileItem tile) return;
            if (string.IsNullOrWhiteSpace(tile.FilePath)) return;

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(tile.FilePath);
                await Launcher.LaunchFileAsync(file);  // 交给系统默认应用
            }
            catch { /* 可选：弹错误 */ }
        }

        // ★右键（文件卡）：重设文件路径
        private async void SetFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem mi) return;
            if (mi.DataContext is not TileItem tile) return;

            var picker = new FileOpenPicker();
            var hwnd = GetHwnd();
            if (hwnd != IntPtr.Zero) InitializeWithWindow.Initialize(picker, hwnd);

            if (tile.FileKind == FileKind.Pdf) { picker.FileTypeFilter.Add(".pdf"); }
            else { picker.FileTypeFilter.Add(".md"); picker.FileTypeFilter.Add(".markdown"); }

            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            tile.FilePath = file.Path;
            // 若想立刻刷新 Subtitle，可把 Subtitle 改为通知属性（见下一个补丁）
        }
    }

    public class TileItem : System.ComponentModel.INotifyPropertyChanged
    {
        private int _rowSpan = 1;
        private int _columnSpan = 1;
        // ★新增：把“本地路径优先，其次 Url”的选择放到属性里
        public string? ImageSourceResolved => string.IsNullOrEmpty(ImagePathOrUrl) ? ImageUrl : ImagePathOrUrl;

        // ===== 通用信息 =====
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public TileKind Kind { get; set; } = TileKind.Image;

        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";

        // 现有图片 Url 字段保留；再补一个本地路径字段（Image 优先用本地路径）
        public string ImageUrl { get; set; } = "";
        public string? ImagePathOrUrl { get; set; }

        // ===== Note 载荷 =====
        //public string? NoteText { get; set; }
        private string? _noteText;
        public string? NoteText
        {
            get => _noteText;
            set { if (_noteText != value) { _noteText = value; OnPropertyChanged(); } }
        }
        // ===== File 载荷 =====
        public FileKind? FileKind { get; set; }
        public string? FilePath { get; set; }
        public int? PdfPage { get; set; }
        public string? MdAnchor { get; set; }
        public string? PptSlideIdOrIndex { get; set; }

        // ===== 布局跨度 =====
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