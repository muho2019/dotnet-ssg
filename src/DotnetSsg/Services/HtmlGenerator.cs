using DotnetSsg.Models;
using DotnetSsg.Components.Pages;
using DotnetSsg.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DotnetSsg.Services;

public class HtmlGenerator : IHtmlGenerator
{
    private readonly IBlazorRenderer _blazorRenderer;
    private readonly ILogger<HtmlGenerator> _logger;

    public HtmlGenerator(IBlazorRenderer blazorRenderer, ILogger<HtmlGenerator> logger)
    {
        _blazorRenderer = blazorRenderer;
        _logger = logger;
    }


    public async Task GenerateAsync(ContentItem item, SiteConfig siteConfig)
    {
        string fullHtml;

        if (item is Post post)
        {
            // Post를 직접 MainLayout의 Body로 전달
            var canonicalUrl = BuildCanonicalUrl(siteConfig, post.Url);
            var ogImage = GetAbsoluteImageUrl(siteConfig, post.CoverImage ?? post.Image ?? siteConfig.OgImage);
            var structuredData = GenerateArticleStructuredData(post, siteConfig, canonicalUrl, ogImage);
            var optimizedTitle = FormatTitle(post.Title, siteConfig.Title);
            var optimizedDescription = !string.IsNullOrWhiteSpace(post.OptimizedDescription)
                ? post.OptimizedDescription
                : siteConfig.Description;

            var layoutParams = new Dictionary<string, object?>
            {
                ["ChildContent"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<PostPage>(0);
                    builder.AddAttribute(1, "Post", post);
                    builder.CloseComponent();
                }),
                ["Title"] = optimizedTitle,
                ["Description"] = optimizedDescription,
                ["SiteTitle"] = siteConfig.Title,
                ["Author"] = siteConfig.Author,
                ["GithubUrl"] = siteConfig.GithubUrl,
                ["GoogleAnalyticsId"] = siteConfig.GoogleAnalyticsId,
                ["BasePath"] = siteConfig.BasePath,
                ["CanonicalUrl"] = canonicalUrl,
                ["OgType"] = "article",
                ["OgImage"] = ogImage,
                ["OgImageAlt"] = post.ImageAlt ?? post.Title,
                ["TwitterCardType"] = !string.IsNullOrEmpty(ogImage) ? "summary_large_image" : "summary",
                ["TwitterSite"] = siteConfig.TwitterSite,
                ["TwitterCreator"] = siteConfig.TwitterCreator,
                ["StructuredData"] = structuredData
            };

            fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);
        }
        else
        {
            // Page를 직접 MainLayout의 Body로 전달
            var canonicalUrl = BuildCanonicalUrl(siteConfig, item.Url);
            var ogImage = GetAbsoluteImageUrl(siteConfig, item.CoverImage ?? item.Image ?? siteConfig.OgImage);
            var optimizedTitle = FormatTitle(item.Title, siteConfig.Title);
            var optimizedDescription = !string.IsNullOrWhiteSpace(item.OptimizedDescription)
                ? item.OptimizedDescription
                : siteConfig.Description;

            var layoutParams = new Dictionary<string, object?>
            {
                ["ChildContent"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<PageTemplate>(0);
                    builder.AddAttribute(1, "Page", item);
                    builder.CloseComponent();
                }),
                ["Title"] = optimizedTitle,
                ["Description"] = optimizedDescription,
                ["SiteTitle"] = siteConfig.Title,
                ["Author"] = siteConfig.Author,
                ["GithubUrl"] = siteConfig.GithubUrl,
                ["GoogleAnalyticsId"] = siteConfig.GoogleAnalyticsId,
                ["BasePath"] = siteConfig.BasePath,
                ["CanonicalUrl"] = canonicalUrl,
                ["OgType"] = "website",
                ["OgImage"] = ogImage,
                ["OgImageAlt"] = item.ImageAlt ?? item.Title,
                ["TwitterCardType"] = !string.IsNullOrEmpty(ogImage) ? "summary_large_image" : "summary",
                ["TwitterSite"] = siteConfig.TwitterSite,
                ["TwitterCreator"] = siteConfig.TwitterCreator
            };

            fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);
        }

        // 출력 경로 결정
        var relativePath = Path.GetRelativePath("content", item.SourcePath);
        var pathWithoutExtension = Path.ChangeExtension(relativePath, null);

        string outputPath;
        var fileName = Path.GetFileNameWithoutExtension(item.SourcePath);
        if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            var parentDir = Path.GetDirectoryName(pathWithoutExtension);
            outputPath = Path.Combine("output", parentDir ?? string.Empty, "index.html");
        }
        else
        {
            outputPath = Path.Combine("output", pathWithoutExtension, "index.html");
        }

        // 디렉토리 생성 및 파일 저장
        var outputDir = Path.GetDirectoryName(outputPath);
        if (outputDir != null)
        {
            Directory.CreateDirectory(outputDir);
        }

        await WriteFileWithRetryAsync(outputPath, fullHtml);
        _logger.LogInformation("Generated: {OutputPath}", outputPath);
    }

    public async Task GenerateIndexAsync(SiteConfig siteConfig, List<Post> posts, string outputDirectory)
    {
        var canonicalUrl = BuildCanonicalUrl(siteConfig, "/");
        var ogImage = GetAbsoluteImageUrl(siteConfig, siteConfig.OgImage);
        var structuredData = GenerateWebSiteStructuredData(siteConfig, canonicalUrl);

        var layoutParams = new Dictionary<string, object?>
        {
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<IndexPage>(0);
                builder.AddAttribute(1, "Site", siteConfig);
                builder.AddAttribute(2, "Posts", posts);
                builder.CloseComponent();
            }),
            ["Title"] = siteConfig.Title,
            ["Description"] = siteConfig.Description,
            ["SiteTitle"] = siteConfig.Title,
            ["Author"] = siteConfig.Author,
            ["GithubUrl"] = siteConfig.GithubUrl,
            ["GoogleAnalyticsId"] = siteConfig.GoogleAnalyticsId,
            ["BasePath"] = siteConfig.BasePath,
            ["CanonicalUrl"] = canonicalUrl,
            ["OgType"] = "website",
            ["OgImage"] = ogImage,
            ["OgImageAlt"] = siteConfig.OgImageAlt,
            ["TwitterCardType"] = !string.IsNullOrEmpty(ogImage) ? "summary_large_image" : "summary",
            ["TwitterSite"] = siteConfig.TwitterSite,
            ["TwitterCreator"] = siteConfig.TwitterCreator,
            ["StructuredData"] = structuredData
        };

        var fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);

        var outputPath = Path.Combine(outputDirectory, "index.html");
        await WriteFileWithRetryAsync(outputPath, fullHtml);
        _logger.LogInformation("Generated: {OutputPath}", outputPath);
    }

    public async Task GenerateTagArchiveAsync(SiteConfig siteConfig, string tag, List<Post> posts,
        string outputDirectory)
    {
        var tagUrl = $"/tags/{tag}/";
        var canonicalUrl = BuildCanonicalUrl(siteConfig, tagUrl);
        var ogImage = GetAbsoluteImageUrl(siteConfig, siteConfig.OgImage);
        var structuredData = GenerateCollectionPageStructuredData(tag, posts, siteConfig, canonicalUrl);

        var layoutParams = new Dictionary<string, object?>
        {
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<TagArchive>(0);
                builder.AddAttribute(1, "Site", siteConfig);
                builder.AddAttribute(2, "Tag", tag);
                builder.AddAttribute(3, "Posts", posts);
                builder.CloseComponent();
            }),
            ["Title"] = $"{tag} - {siteConfig.Title}",
            ["Description"] = $"{tag} 태그가 포함된 포스트",
            ["SiteTitle"] = siteConfig.Title,
            ["Author"] = siteConfig.Author,
            ["GithubUrl"] = siteConfig.GithubUrl,
            ["GoogleAnalyticsId"] = siteConfig.GoogleAnalyticsId,
            ["BasePath"] = siteConfig.BasePath,
            ["CanonicalUrl"] = canonicalUrl,
            ["OgType"] = "website",
            ["OgImage"] = ogImage,
            ["OgImageAlt"] = siteConfig.OgImageAlt,
            ["TwitterCardType"] = "summary",
            ["TwitterSite"] = siteConfig.TwitterSite,
            ["TwitterCreator"] = siteConfig.TwitterCreator,
            ["StructuredData"] = structuredData
        };

        var fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);

        var tagDir = Path.Combine(outputDirectory, "tags", tag);
        Directory.CreateDirectory(tagDir);

        var outputPath = Path.Combine(tagDir, "index.html");
        await WriteFileWithRetryAsync(outputPath, fullHtml);
        _logger.LogInformation("Generated: {OutputPath}", outputPath);
    }


    private string BuildCanonicalUrl(SiteConfig siteConfig, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(siteConfig.BaseUrl))
            return string.Empty;

        var baseUrl = siteConfig.NormalizedBaseUrl;
        var path = relativePath.TrimStart('/');
        return $"{baseUrl}{path}";
    }

    private string? GetAbsoluteImageUrl(SiteConfig siteConfig, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        // 이미 절대 URL인 경우
        if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            return imageUrl;

        // 상대 경로를 절대 URL로 변환
        if (string.IsNullOrWhiteSpace(siteConfig.BaseUrl))
            return null;

        var baseUrl = siteConfig.NormalizedBaseUrl;
        var path = imageUrl.TrimStart('/');
        return $"{baseUrl}{path}";
    }

    private string GenerateArticleStructuredData(Post post, SiteConfig siteConfig, string canonicalUrl,
        string? imageUrl)
    {
        var data = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Article",
            ["headline"] = post.Title,
            ["datePublished"] = post.Date.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            ["dateModified"] = (post.LastModified ?? post.Date).ToString("yyyy-MM-ddTHH:mm:sszzz"),
            ["author"] = new Dictionary<string, object>
            {
                ["@type"] = "Person",
                ["name"] = post.Author ?? siteConfig.Author
            },
            ["publisher"] = new Dictionary<string, object>
            {
                ["@type"] = "Organization",
                ["name"] = siteConfig.Title
            }
        };

        if (!string.IsNullOrWhiteSpace(post.Description))
            data["description"] = post.Description;

        if (!string.IsNullOrWhiteSpace(canonicalUrl))
            data["url"] = canonicalUrl;

        if (!string.IsNullOrWhiteSpace(imageUrl))
            data["image"] = imageUrl;

        if (post.Tags.Any())
            data["keywords"] = string.Join(", ", post.Tags);

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateWebSiteStructuredData(SiteConfig siteConfig, string canonicalUrl)
    {
        var data = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "WebSite",
            ["name"] = siteConfig.Title,
            ["description"] = siteConfig.Description
        };

        if (!string.IsNullOrWhiteSpace(canonicalUrl))
            data["url"] = canonicalUrl;

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateCollectionPageStructuredData(string tag, List<Post> posts, SiteConfig siteConfig,
        string canonicalUrl)
    {
        var data = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "CollectionPage",
            ["name"] = $"{tag} - {siteConfig.Title}",
            ["description"] = $"{tag} 태그가 포함된 포스트",
            ["numberOfItems"] = posts.Count
        };

        if (!string.IsNullOrWhiteSpace(canonicalUrl))
            data["url"] = canonicalUrl;

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private string FormatTitle(string pageTitle, string siteTitle)
    {
        // 홈페이지는 사이트 제목만
        if (string.IsNullOrWhiteSpace(pageTitle) || pageTitle == siteTitle)
            return siteTitle;

        // 일반 페이지는 "페이지 제목 | 사이트 제목" 형식
        var formattedTitle = $"{pageTitle} | {siteTitle}";

        // SEO 권장 최대 길이 60자 체크 (경고만 표시, 강제하지 않음)
        if (formattedTitle.Length > 60)
        {
            _logger.LogWarning(
                "Title length warning: '{FormattedTitle}' is {TitleLength} characters (recommended: 50-60)", formattedTitle, formattedTitle.Length);
        }

        return formattedTitle;
    }

    private static async Task WriteFileWithRetryAsync(string path, string content, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await File.WriteAllTextAsync(path, content);
                return; // 성공
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                // 파일이 사용 중이면 잠시 대기 후 재시도
                await Task.Delay(50 * (i + 1)); // 50ms, 100ms, 150ms
            }
        }
    }
}