using DotnetSsg.Components.Pages;
using DotnetSsg.Components.Layout;
using DotnetSsg.Models;
using Microsoft.AspNetCore.Components;

namespace DotnetSsg.Services.RenderStrategies;

/// <summary>
/// 일반 페이지(Page) 렌더링을 담당하는 전략 구현체입니다.
/// </summary>
public class PageRenderStrategy : IRenderStrategy
{
    private readonly IBlazorRenderer _renderer;
    private readonly ISeoService _seoService;

    public PageRenderStrategy(IBlazorRenderer renderer, ISeoService seoService)
    {
        _renderer = renderer;
        _seoService = seoService;
    }

    /// <inheritdoc />
    public bool CanRender(ContentItem item) => item is Page;

    /// <inheritdoc />
    public async Task<string> RenderAsync(ContentItem item, SiteConfig siteConfig)
    {
        var page = (Page)item;

        // SEO 데이터 생성
        var canonicalUrl = _seoService.BuildCanonicalUrl(siteConfig, page.Url);
        var ogImage = _seoService.GetAbsoluteImageUrl(siteConfig, page.CoverImage ?? page.Image ?? siteConfig.OgImage);
        var optimizedTitle = _seoService.FormatTitle(page.Title, siteConfig.Title);
        var optimizedDescription = !string.IsNullOrWhiteSpace(page.OptimizedDescription)
            ? page.OptimizedDescription
            : siteConfig.Description;

        // MainLayout 파라미터 구성
        var layoutParams = new Dictionary<string, object?>
        {
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<PageTemplate>(0);
                builder.AddAttribute(1, "Page", page);
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
            ["OgImageAlt"] = page.ImageAlt ?? page.Title,
            ["TwitterCardType"] = !string.IsNullOrEmpty(ogImage) ? "summary_large_image" : "summary",
            ["TwitterSite"] = siteConfig.TwitterSite,
            ["TwitterCreator"] = siteConfig.TwitterCreator
        };

        return await _renderer.RenderComponentAsync<MainLayout>(layoutParams);
    }
}
