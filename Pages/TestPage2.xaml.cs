using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2.Pages;

/// <summary>
/// 随机图片瀑布流页面
/// </summary>
public sealed partial class TestPage2 : Page
{
    public ObservableCollection<ImageItem> ImageCollection { get; } = new();

    private readonly Random _random = new();
    private readonly string[] _categories = { "nature", "city", "technology", "food", "animals", "architecture", "people", "abstract" };
    private readonly string[] _imageDescriptions =
    {
        "美丽的自然风光", "现代都市景观", "科技感十足", "美味佳肴",
        "可爱的动物", "建筑艺术", "人文摄影", "抽象艺术",
        "梦幻色彩", "光影交织", "静谧时光", "活力四射"
    };

    public TestPage2()
    {
        this.InitializeComponent();
        this.Loaded += TestPage2_Loaded;
    }

    private async void TestPage2_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRandomImages();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadRandomImages();
    }

    /// <summary>
    /// 加载随机图片
    /// </summary>
    private async Task LoadRandomImages()
    {
        LoadingRing.IsActive = true;
        ImageCollection.Clear();

        try
        {
            // 生成30-50张随机图片
            int imageCount = _random.Next(30, 51);

            for (int i = 0; i < imageCount; i++)
            {
                var imageItem = GenerateRandomImageItem(i);
                ImageCollection.Add(imageItem);

                // 每添加几张图片就暂停一下，创造渐进式加载效果
                if (i % 5 == 0)
                {
                    await Task.Delay(50);
                }
            }
        }
        finally
        {
            LoadingRing.IsActive = false;
        }
    }

    /// <summary>
    /// 生成随机图片项
    /// </summary>
    private ImageItem GenerateRandomImageItem(int index)
    {
        // 随机尺寸 - 固定宽度，变化高度以创造瀑布流效果
        int width = 400; // 固定宽度
        int height = _random.Next(300, 700); // 随机高度

        // 使用 Lorem Picsum 服务生成随机图片
        // 格式: https://picsum.photos/width/height?random=seed
        string imageUrl = $"https://picsum.photos/{width}/{height}?random={_random.Next(1, 10000)}";

        // 随机标题和描述
        string title = $"随机图片 #{index + 1}";
        string description = _imageDescriptions[_random.Next(_imageDescriptions.Length)];

        return new ImageItem
        {
            Id = index,
            ImageUrl = imageUrl,
            Title = title,
            Description = description,
            Width = width,
            Height = height
        };
    }
}

/// <summary>
/// 图片项数据模型
/// </summary>
public class ImageItem
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// 扩展的图片项，支持更多随机图片服务
/// </summary>
public static class ImageUrlGenerator
{
    private static readonly Random Random = new();

    /// <summary>
    /// 生成 Picsum Photos 随机图片URL
    /// </summary>
    public static string GetPicsumUrl(int width, int height, int? seed = null)
    {
        int actualSeed = seed ?? Random.Next(1, 10000);
        return $"https://picsum.photos/{width}/{height}?random={actualSeed}";
    }

    /// <summary>
    /// 生成 Unsplash Source 随机图片URL (备选)
    /// </summary>
    public static string GetUnsplashUrl(int width, int height, string? category = null)
    {
        if (string.IsNullOrEmpty(category))
        {
            return $"https://source.unsplash.com/{width}x{height}?random={Random.Next(1, 10000)}";
        }
        return $"https://source.unsplash.com/{width}x{height}/?{category}&random={Random.Next(1, 10000)}";
    }

    /// <summary>
    /// 生成 Lorem Flickr 随机图片URL (备选)
    /// </summary>
    public static string GetLoremFlickrUrl(int width, int height, string? tags = null)
    {
        if (string.IsNullOrEmpty(tags))
        {
            return $"https://loremflickr.com/{width}/{height}?random={Random.Next(1, 10000)}";
        }
        return $"https://loremflickr.com/{width}/{height}/{tags}?random={Random.Next(1, 10000)}";
    }
}