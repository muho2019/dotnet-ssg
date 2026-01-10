using DotnetSsg.Models;
using DotnetSsg.Components.Pages;
using DotnetSsg.Components.Layout;
using DotnetSsg.Services.RenderStrategies;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DotnetSsg.Services;

public class HtmlGenerator : IHtmlGenerator
{
    private readonly IBlazorRenderer _blazorRenderer;
    private readonly ILogger<HtmlGenerator> _logger;
    private readonly IEnumerable<IRenderStrategy> _renderStrategies;
    private readonly IPathResolver _pathResolver;

    public HtmlGenerator(IBlazorRenderer blazorRenderer, ILogger<HtmlGenerator> logger, IEnumerable<IRenderStrategy> renderStrategies, IPathResolver pathResolver)
    {
        _blazorRenderer = blazorRenderer;
        _logger = logger;
        _renderStrategies = renderStrategies;
        _pathResolver = pathResolver;
    }


    public async Task GenerateAsync(ContentItem item, SiteConfig siteConfig)
    {
        var strategy = _renderStrategies.FirstOrDefault(s => s.CanRender(item));
        if (strategy == null)
        {
            _logger.LogWarning("No render strategy found for content item type: {ItemType}", item.GetType().Name);
            return;
        }

        var fullHtml = await strategy.RenderAsync(item, siteConfig);

        // PathResolver를 사용하여 출력 경로 결정 (HtmlGenerator의 로직 단순화)
        // 참고: 여기서 contentRoot는 "content"로 가정합니다. HtmlGenerator는 보통 빌드 컨텍스트에서 실행되며
        // 소스 경로는 프로젝트 루트에 상대적이거나 절대 경로입니다.
        // 더 나은 방법: MarkdownParser에서 이미 item.OutputPath가 설정되었다면 그것을 신뢰하는 것이 좋음?
        // 일관성을 위해 PathResolver를 사용하되, contentRoot가 동적이라면 알아야 합니다.
        // 현재로서는 PathResolver 인터페이스/구현에서 기본값 "content"가 관례와 일치합니다.
        string outputPath = _pathResolver.GetOutputPath(item, "content");

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

        // PathResolver를 활용할 수도 있지만, IndexGenerator/TagArchiveGenerator의 역할이므로 
        // 일단 outputDirectory를 신뢰하고 파일명만 추가
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
                "제목 길이 경고: '{FormattedTitle}'의 길이는 {TitleLength}자입니다 (권장: 50-60자)", formattedTitle, formattedTitle.Length);
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