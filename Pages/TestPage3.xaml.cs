using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI;

namespace App2.Pages;

/// <summary>
/// Win10风格的磁贴页面，支持拖拽和不规则大小
/// </summary>
public sealed partial class TestPage3 : Page
{
    public ObservableCollection<DashboardTile> TileItems { get; set; }

    public TestPage3()
    {
        InitializeComponent();
        InitializeTiles();
    }

    private void InitializeTiles()
    {
        TileItems = new ObservableCollection<DashboardTile>
        {
            // 小磁贴 (1x1)
            new DashboardTile
            {
                Title = "邮件",
                IconGlyph = "\uE715",
                TileColor = Color.FromArgb(255, 30, 144, 255),
                Description = "查看最新邮件",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "日历",
                IconGlyph = "\uE787",
                TileColor = Color.FromArgb(255, 60, 179, 113),
                Description = "今日日程安排",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "设置",
                IconGlyph = "\uE713",
                TileColor = Color.FromArgb(255, 112, 128, 144),
                Description = "系统设置",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "商店",
                IconGlyph = "\uE719",
                TileColor = Color.FromArgb(255, 0, 128, 128),
                Description = "应用商店",
                TileSize = TileSize.Small
            },

            // 中等磁贴 (2x2)
            new DashboardTile
            {
                Title = "照片",
                IconGlyph = "\uE91B",
                TileColor = Color.FromArgb(255, 255, 69, 0),
                Description = "浏览相册",
                ImageUrl = "https://picsum.photos/400/400?random=1",
                TileSize = TileSize.Medium
            },
            new DashboardTile
            {
                Title = "音乐",
                IconGlyph = "\uE8D6",
                TileColor = Color.FromArgb(255, 147, 112, 219),
                Description = "播放音乐",
                ImageUrl = "https://picsum.photos/400/400?random=2",
                TileSize = TileSize.Medium
            },

            // 宽磁贴 (4x2)
            new DashboardTile
            {
                Title = "天气",
                IconGlyph = "\uE753",
                TileColor = Color.FromArgb(255, 135, 206, 235),
                Description = "新加坡 · 28°C · 多云 · 湿度 78%",
                TileSize = TileSize.Wide
            },
            new DashboardTile
            {
                Title = "新闻",
                IconGlyph = "\uE8F2",
                TileColor = Color.FromArgb(255, 220, 20, 60),
                Description = "最新头条 · 科技要闻 · 财经动态",
                ImageUrl = "https://picsum.photos/640/320?random=3",
                TileSize = TileSize.Wide
            },

            // 大磁贴 (4x4)
            new DashboardTile
            {
                Title = "地图",
                IconGlyph = "\uE707",
                TileColor = Color.FromArgb(255, 255, 127, 80),
                Description = "导航 · 实时交通 · 周边搜索 · 路线规划",
                ImageUrl = "https://picsum.photos/640/640?random=4",
                TileSize = TileSize.Large
            },

            // 更多小磁贴
            new DashboardTile
            {
                Title = "通知",
                IconGlyph = "\uE91C",
                TileColor = Color.FromArgb(255, 255, 140, 0),
                Description = "通知中心",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "视频",
                IconGlyph = "\uE714",
                TileColor = Color.FromArgb(255, 199, 21, 133),
                Description = "视频播放器",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "文档",
                IconGlyph = "\uE8A5",
                TileColor = Color.FromArgb(255, 70, 130, 180),
                Description = "我的文档",
                TileSize = TileSize.Small
            },

            // 中等磁贴
            new DashboardTile
            {
                Title = "游戏",
                IconGlyph = "\uE7FC",
                TileColor = Color.FromArgb(255, 46, 139, 87),
                Description = "游戏中心",
                ImageUrl = "https://picsum.photos/400/400?random=5",
                TileSize = TileSize.Medium
            },

            // 宽磁贴
            new DashboardTile
            {
                Title = "财务",
                IconGlyph = "\uE8C7",
                TileColor = Color.FromArgb(255, 0, 100, 0),
                Description = "账户余额 · 交易记录 · 投资理财",
                ImageUrl = "https://picsum.photos/640/320?random=6",
                TileSize = TileSize.Wide
            }
        };
    }

    private void TilePanel_TileClicked(object sender, DashboardTile tile)
    {
        System.Diagnostics.Debug.WriteLine($"点击了磁贴: {tile.Title}");
        // 可以在这里导航到对应页面或执行操作
    }
}

/// <summary>
/// 磁贴大小枚举
/// </summary>
public enum TileSize
{
    Small = 1,    // 1x1
    Medium = 2,   // 2x2
    Wide = 3,     // 4x2
    Large = 4     // 4x4
}

/// <summary>
/// 仪表板磁贴数据模型
/// </summary>
public class DashboardTile
{
    public string Title { get; set; }
    public string IconGlyph { get; set; }
    public Color TileColor { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public TileSize TileSize { get; set; } = TileSize.Small;
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);

    public int ColSpan => TileSize switch
    {
        TileSize.Small => 1,
        TileSize.Medium => 2,
        TileSize.Wide => 4,
        TileSize.Large => 4,
        _ => 1
    };

    public int RowSpan => TileSize switch
    {
        TileSize.Small => 1,
        TileSize.Medium => 2,
        TileSize.Wide => 2,
        TileSize.Large => 4,
        _ => 1
    };
}

/// <summary>
/// 自定义拖拽磁贴容器
/// </summary>
public class DraggableTilePanel : Canvas
{
    private const double TILE_SIZE = 140;
    private const double TILE_MARGIN = 4;
    private const int MAX_COLUMNS = 8;

    private readonly List<TileContainer> _tileContainers = new();
    private TileContainer _draggingTile;
    private Point _dragStartPoint;
    private Point _dragOffset;
    private int _originalIndex;

    public static readonly DependencyProperty TileItemsProperty =
        DependencyProperty.Register(nameof(TileItems), typeof(ObservableCollection<DashboardTile>),
            typeof(DraggableTilePanel), new PropertyMetadata(null, OnTileItemsChanged));

    public static readonly DependencyProperty SmallTileTemplateProperty =
        DependencyProperty.Register(nameof(SmallTileTemplate), typeof(DataTemplate),
            typeof(DraggableTilePanel), new PropertyMetadata(null));

    public static readonly DependencyProperty MediumTileTemplateProperty =
        DependencyProperty.Register(nameof(MediumTileTemplate), typeof(DataTemplate),
            typeof(DraggableTilePanel), new PropertyMetadata(null));

    public static readonly DependencyProperty WideTileTemplateProperty =
        DependencyProperty.Register(nameof(WideTileTemplate), typeof(DataTemplate),
            typeof(DraggableTilePanel), new PropertyMetadata(null));

    public static readonly DependencyProperty LargeTileTemplateProperty =
        DependencyProperty.Register(nameof(LargeTileTemplate), typeof(DataTemplate),
            typeof(DraggableTilePanel), new PropertyMetadata(null));

    public ObservableCollection<DashboardTile> TileItems
    {
        get => (ObservableCollection<DashboardTile>)GetValue(TileItemsProperty);
        set => SetValue(TileItemsProperty, value);
    }

    public DataTemplate SmallTileTemplate
    {
        get => (DataTemplate)GetValue(SmallTileTemplateProperty);
        set => SetValue(SmallTileTemplateProperty, value);
    }

    public DataTemplate MediumTileTemplate
    {
        get => (DataTemplate)GetValue(MediumTileTemplateProperty);
        set => SetValue(MediumTileTemplateProperty, value);
    }

    public DataTemplate WideTileTemplate
    {
        get => (DataTemplate)GetValue(WideTileTemplateProperty);
        set => SetValue(WideTileTemplateProperty, value);
    }

    public DataTemplate LargeTileTemplate
    {
        get => (DataTemplate)GetValue(LargeTileTemplateProperty);
        set => SetValue(LargeTileTemplateProperty, value);
    }

    public event EventHandler<DashboardTile> TileClicked;

    private static void OnTileItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DraggableTilePanel panel)
        {
            panel.RefreshTiles();
        }
    }

    private void RefreshTiles()
    {
        Children.Clear();
        _tileContainers.Clear();

        if (TileItems == null) return;

        foreach (var tile in TileItems)
        {
            var container = CreateTileContainer(tile);
            _tileContainers.Add(container);
            Children.Add(container);
        }

        ArrangeTiles();
    }

    private TileContainer CreateTileContainer(DashboardTile tile)
    {
        var template = tile.TileSize switch
        {
            TileSize.Small => SmallTileTemplate,
            TileSize.Medium => MediumTileTemplate,
            TileSize.Wide => WideTileTemplate,
            TileSize.Large => LargeTileTemplate,
            _ => SmallTileTemplate
        };

        var content = template?.LoadContent() as FrameworkElement;
        if (content != null)
        {
            content.DataContext = tile;
        }

        var container = new TileContainer
        {
            Tile = tile,
            Width = TILE_SIZE * tile.ColSpan + TILE_MARGIN * 2 * (tile.ColSpan - 1),
            Height = TILE_SIZE * tile.RowSpan + TILE_MARGIN * 2 * (tile.RowSpan - 1),
            Content = content,
            RenderTransform = new TranslateTransform()
        };

        container.PointerPressed += TileContainer_PointerPressed;
        container.PointerMoved += TileContainer_PointerMoved;
        container.PointerReleased += TileContainer_PointerReleased;
        container.PointerCanceled += TileContainer_PointerReleased;
        container.Tapped += TileContainer_Tapped;

        return container;
    }

    private void TileContainer_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is TileContainer container && _draggingTile == null)
        {
            TileClicked?.Invoke(this, container.Tile);
        }
    }

    private void TileContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not TileContainer container) return;

        _draggingTile = container;
        _dragStartPoint = e.GetCurrentPoint(this).Position;
        _dragOffset = new Point(
            _dragStartPoint.X - Canvas.GetLeft(container),
            _dragStartPoint.Y - Canvas.GetTop(container)
        );
        _originalIndex = _tileContainers.IndexOf(container);

        container.CapturePointer(e.Pointer);
        container.Opacity = 0.7;
        Canvas.SetZIndex(container, 1000);

        e.Handled = true;
    }

    private void TileContainer_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingTile == null || sender is not TileContainer container) return;

        var currentPoint = e.GetCurrentPoint(this).Position;
        var newX = currentPoint.X - _dragOffset.X;
        var newY = currentPoint.Y - _dragOffset.Y;

        if (container.RenderTransform is TranslateTransform transform)
        {
            var originalLeft = Canvas.GetLeft(container);
            var originalTop = Canvas.GetTop(container);

            transform.X = newX - originalLeft;
            transform.Y = newY - originalTop;
        }

        // 检查是否需要重排
        CheckAndReorder(container, new Point(newX, newY));

        e.Handled = true;
    }

    private void TileContainer_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingTile == null) return;

        _draggingTile.ReleasePointerCaptures();
        _draggingTile.Opacity = 1.0;
        Canvas.SetZIndex(_draggingTile, 0);

        // 动画回到最终位置
        AnimateTileToPosition(_draggingTile, Canvas.GetLeft(_draggingTile), Canvas.GetTop(_draggingTile));

        _draggingTile = null;
        e.Handled = true;
    }

    private void CheckAndReorder(TileContainer draggingContainer, Point currentPosition)
    {
        var dragCenterX = currentPosition.X + draggingContainer.Width / 2;
        var dragCenterY = currentPosition.Y + draggingContainer.Height / 2;

        for (int i = 0; i < _tileContainers.Count; i++)
        {
            var target = _tileContainers[i];
            if (target == draggingContainer) continue;

            var targetLeft = Canvas.GetLeft(target);
            var targetTop = Canvas.GetTop(target);
            var targetCenterX = targetLeft + target.Width / 2;
            var targetCenterY = targetTop + target.Height / 2;

            var distance = Math.Sqrt(
                Math.Pow(dragCenterX - targetCenterX, 2) +
                Math.Pow(dragCenterY - targetCenterY, 2)
            );

            if (distance < TILE_SIZE)
            {
                var currentIndex = _tileContainers.IndexOf(draggingContainer);
                if (currentIndex != i)
                {
                    _tileContainers.RemoveAt(currentIndex);
                    _tileContainers.Insert(i, draggingContainer);

                    // 更新数据源顺序
                    if (TileItems != null)
                    {
                        TileItems.Move(currentIndex, i);
                    }

                    ArrangeTiles(draggingContainer);
                    break;
                }
            }
        }
    }

    private void ArrangeTiles(TileContainer excludeTile = null)
    {
        const double PADDING = 20; // 边距偏移
        var grid = new bool[100, MAX_COLUMNS]; // 假设最多100行
        var positions = new Dictionary<TileContainer, Point>();

        foreach (var container in _tileContainers)
        {
            if (container == excludeTile) continue;

            var placed = false;
            for (int row = 0; row < 100 && !placed; row++)
            {
                for (int col = 0; col < MAX_COLUMNS && !placed; col++)
                {
                    if (CanPlaceTile(grid, row, col, container.Tile.RowSpan, container.Tile.ColSpan))
                    {
                        PlaceTile(grid, row, col, container.Tile.RowSpan, container.Tile.ColSpan);
                        var x = PADDING + col * (TILE_SIZE + TILE_MARGIN * 2);
                        var y = PADDING + row * (TILE_SIZE + TILE_MARGIN * 2);
                        positions[container] = new Point(x, y);
                        placed = true;
                    }
                }
            }
        }

        foreach (var kvp in positions)
        {
            if (kvp.Key == _draggingTile) continue;
            AnimateTileToPosition(kvp.Key, kvp.Value.X, kvp.Value.Y);
        }

        // 更新Canvas高度
        if (positions.Any())
        {
            var maxY = positions.Max(p => p.Value.Y + p.Key.Height);
            Height = maxY + PADDING;
        }
    }

    private bool CanPlaceTile(bool[,] grid, int row, int col, int rowSpan, int colSpan)
    {
        if (col + colSpan > MAX_COLUMNS) return false;

        for (int r = row; r < row + rowSpan; r++)
        {
            for (int c = col; c < col + colSpan; c++)
            {
                if (grid[r, c]) return false;
            }
        }
        return true;
    }

    private void PlaceTile(bool[,] grid, int row, int col, int rowSpan, int colSpan)
    {
        for (int r = row; r < row + rowSpan; r++)
        {
            for (int c = col; c < col + colSpan; c++)
            {
                grid[r, c] = true;
            }
        }
    }

    private void AnimateTileToPosition(TileContainer container, double toX, double toY)
    {
        var transform = container.RenderTransform as TranslateTransform;
        if (transform == null) return;

        var currentLeft = Canvas.GetLeft(container);
        var currentTop = Canvas.GetTop(container);

        var storyboard = new Storyboard();
        var duration = new Duration(TimeSpan.FromMilliseconds(250));

        var animX = new DoubleAnimation
        {
            From = transform.X,
            To = 0,
            Duration = duration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(animX, transform);
        Storyboard.SetTargetProperty(animX, "X");

        var animY = new DoubleAnimation
        {
            From = transform.Y,
            To = 0,
            Duration = duration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(animY, transform);
        Storyboard.SetTargetProperty(animY, "Y");

        storyboard.Children.Add(animX);
        storyboard.Children.Add(animY);

        storyboard.Completed += (s, e) =>
        {
            Canvas.SetLeft(container, toX);
            Canvas.SetTop(container, toY);
            transform.X = 0;
            transform.Y = 0;
        };

        Canvas.SetLeft(container, currentLeft);
        Canvas.SetTop(container, currentTop);

        storyboard.Begin();
    }

    private class TileContainer : ContentControl
    {
        public DashboardTile Tile { get; set; }
    }
}