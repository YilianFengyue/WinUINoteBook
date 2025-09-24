using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace App2.Pages
{
    public sealed partial class TestPage : Page
    {
        public ObservableCollection<TileItem> Tiles { get; } = new();

        // 全局“间距”与“尺寸”
        private double _tileGap = 6;

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
            if (TileGrid?.ItemsPanelRoot is ItemsWrapGrid panel)
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
            if (args.ItemContainer is GridViewItem gvi)
            {
                gvi.Margin = new Thickness(_tileGap);
            }
        }

        public Visibility ContextualItem { get; } = Visibility.Visible;

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSamples();
            ApplyGap(_tileGap);
        }
    }

    public class TileItem
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}
