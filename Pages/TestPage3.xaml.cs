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
/// Win10���Ĵ���ҳ�棬֧����ק�Ͳ������С
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
            // С���� (1x1)
            new DashboardTile
            {
                Title = "�ʼ�",
                IconGlyph = "\uE715",
                TileColor = Color.FromArgb(255, 30, 144, 255),
                Description = "�鿴�����ʼ�",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "����",
                IconGlyph = "\uE787",
                TileColor = Color.FromArgb(255, 60, 179, 113),
                Description = "�����ճ̰���",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "����",
                IconGlyph = "\uE713",
                TileColor = Color.FromArgb(255, 112, 128, 144),
                Description = "ϵͳ����",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "�̵�",
                IconGlyph = "\uE719",
                TileColor = Color.FromArgb(255, 0, 128, 128),
                Description = "Ӧ���̵�",
                TileSize = TileSize.Small
            },

            // �еȴ��� (2x2)
            new DashboardTile
            {
                Title = "��Ƭ",
                IconGlyph = "\uE91B",
                TileColor = Color.FromArgb(255, 255, 69, 0),
                Description = "������",
                ImageUrl = "https://picsum.photos/400/400?random=1",
                TileSize = TileSize.Medium
            },
            new DashboardTile
            {
                Title = "����",
                IconGlyph = "\uE8D6",
                TileColor = Color.FromArgb(255, 147, 112, 219),
                Description = "��������",
                ImageUrl = "https://picsum.photos/400/400?random=2",
                TileSize = TileSize.Medium
            },

            // ����� (4x2)
            new DashboardTile
            {
                Title = "����",
                IconGlyph = "\uE753",
                TileColor = Color.FromArgb(255, 135, 206, 235),
                Description = "�¼��� �� 28��C �� ���� �� ʪ�� 78%",
                TileSize = TileSize.Wide
            },
            new DashboardTile
            {
                Title = "����",
                IconGlyph = "\uE8F2",
                TileColor = Color.FromArgb(255, 220, 20, 60),
                Description = "����ͷ�� �� �Ƽ�Ҫ�� �� �ƾ���̬",
                ImageUrl = "https://picsum.photos/640/320?random=3",
                TileSize = TileSize.Wide
            },

            // ����� (4x4)
            new DashboardTile
            {
                Title = "��ͼ",
                IconGlyph = "\uE707",
                TileColor = Color.FromArgb(255, 255, 127, 80),
                Description = "���� �� ʵʱ��ͨ �� �ܱ����� �� ·�߹滮",
                ImageUrl = "https://picsum.photos/640/640?random=4",
                TileSize = TileSize.Large
            },

            // ����С����
            new DashboardTile
            {
                Title = "֪ͨ",
                IconGlyph = "\uE91C",
                TileColor = Color.FromArgb(255, 255, 140, 0),
                Description = "֪ͨ����",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "��Ƶ",
                IconGlyph = "\uE714",
                TileColor = Color.FromArgb(255, 199, 21, 133),
                Description = "��Ƶ������",
                TileSize = TileSize.Small
            },
            new DashboardTile
            {
                Title = "�ĵ�",
                IconGlyph = "\uE8A5",
                TileColor = Color.FromArgb(255, 70, 130, 180),
                Description = "�ҵ��ĵ�",
                TileSize = TileSize.Small
            },

            // �еȴ���
            new DashboardTile
            {
                Title = "��Ϸ",
                IconGlyph = "\uE7FC",
                TileColor = Color.FromArgb(255, 46, 139, 87),
                Description = "��Ϸ����",
                ImageUrl = "https://picsum.photos/400/400?random=5",
                TileSize = TileSize.Medium
            },

            // �����
            new DashboardTile
            {
                Title = "����",
                IconGlyph = "\uE8C7",
                TileColor = Color.FromArgb(255, 0, 100, 0),
                Description = "�˻���� �� ���׼�¼ �� Ͷ�����",
                ImageUrl = "https://picsum.photos/640/320?random=6",
                TileSize = TileSize.Wide
            }
        };
    }

    private void TilePanel_TileClicked(object sender, DashboardTile tile)
    {
        System.Diagnostics.Debug.WriteLine($"����˴���: {tile.Title}");
        // ���������ﵼ������Ӧҳ���ִ�в���
    }
}

/// <summary>
/// ������Сö��
/// </summary>
public enum TileSize
{
    Small = 1,    // 1x1
    Medium = 2,   // 2x2
    Wide = 3,     // 4x2
    Large = 4     // 4x4
}

/// <summary>
/// �Ǳ���������ģ��
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
/// �Զ�����ק��������
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

        // ����Ƿ���Ҫ����
        CheckAndReorder(container, new Point(newX, newY));

        e.Handled = true;
    }

    private void TileContainer_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingTile == null) return;

        _draggingTile.ReleasePointerCaptures();
        _draggingTile.Opacity = 1.0;
        Canvas.SetZIndex(_draggingTile, 0);

        // �����ص�����λ��
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

                    // ��������Դ˳��
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
        const double PADDING = 20; // �߾�ƫ��
        var grid = new bool[100, MAX_COLUMNS]; // �������100��
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

        // ����Canvas�߶�
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