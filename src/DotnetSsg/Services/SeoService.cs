using System.Text.Json;
using DotnetSsg.Models;
using Microsoft.Extensions.Logging;

namespace DotnetSsg.Services;

public class SeoService : ISeoService
{
    private readonly ILogger<SeoService> _logger;

    public SeoService(ILogger<SeoService> logger)
    {
        _logger = logger;
    }

    public string BuildCanonicalUrl(SiteConfig siteConfig, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(siteConfig.BaseUrl))
            return string.Empty;

        var baseUrl = siteConfig.NormalizedBaseUrl;
        var path = relativePath.TrimStart('/');
        return $"{baseUrl}{path}";
    }

    public string? GetAbsoluteImageUrl(SiteConfig siteConfig, string? imageUrl)
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

    public string GenerateArticleStructuredData(Post post, SiteConfig siteConfig, string canonicalUrl,
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

    public string FormatTitle(string pageTitle, string siteTitle)
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
}
