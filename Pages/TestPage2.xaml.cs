using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2.Pages;

/// <summary>
/// ���ͼƬ�ٲ���ҳ��
/// </summary>
public sealed partial class TestPage2 : Page
{
    public ObservableCollection<ImageItem> ImageCollection { get; } = new();

    private readonly Random _random = new();
    private readonly string[] _categories = { "nature", "city", "technology", "food", "animals", "architecture", "people", "abstract" };
    private readonly string[] _imageDescriptions =
    {
        "��������Ȼ���", "�ִ����о���", "�Ƽ���ʮ��", "��ζ����",
        "�ɰ��Ķ���", "��������", "������Ӱ", "��������",
        "�λ�ɫ��", "��Ӱ��֯", "����ʱ��", "��������"
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
    /// �������ͼƬ
    /// </summary>
    private async Task LoadRandomImages()
    {
        LoadingRing.IsActive = true;
        ImageCollection.Clear();

        try
        {
            // ����30-50�����ͼƬ
            int imageCount = _random.Next(30, 51);

            for (int i = 0; i < imageCount; i++)
            {
                var imageItem = GenerateRandomImageItem(i);
                ImageCollection.Add(imageItem);

                // ÿ��Ӽ���ͼƬ����ͣһ�£����콥��ʽ����Ч��
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
    /// �������ͼƬ��
    /// </summary>
    private ImageItem GenerateRandomImageItem(int index)
    {
        // ����ߴ� - �̶���ȣ��仯�߶��Դ����ٲ���Ч��
        int width = 400; // �̶����
        int height = _random.Next(300, 700); // ����߶�

        // ʹ�� Lorem Picsum �����������ͼƬ
        // ��ʽ: https://picsum.photos/width/height?random=seed
        string imageUrl = $"https://picsum.photos/{width}/{height}?random={_random.Next(1, 10000)}";

        // ������������
        string title = $"���ͼƬ #{index + 1}";
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
/// ͼƬ������ģ��
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
/// ��չ��ͼƬ�֧�ָ������ͼƬ����
/// </summary>
public static class ImageUrlGenerator
{
    private static readonly Random Random = new();

    /// <summary>
    /// ���� Picsum Photos ���ͼƬURL
    /// </summary>
    public static string GetPicsumUrl(int width, int height, int? seed = null)
    {
        int actualSeed = seed ?? Random.Next(1, 10000);
        return $"https://picsum.photos/{width}/{height}?random={actualSeed}";
    }

    /// <summary>
    /// ���� Unsplash Source ���ͼƬURL (��ѡ)
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
    /// ���� Lorem Flickr ���ͼƬURL (��ѡ)
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