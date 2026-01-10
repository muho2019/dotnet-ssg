using DotnetSsg.Components.Pages;
using DotnetSsg.Components.Layout;
using DotnetSsg.Models;
using Microsoft.AspNetCore.Components;

namespace DotnetSsg.Services.RenderStrategies;

/// <summary>
/// 블로그 포스트(Post) 렌더링을 담당하는 전략 구현체입니다.
/// </summary>
public class PostRenderStrategy : IRenderStrategy
{
    private readonly IBlazorRenderer _renderer;
    private readonly ISeoService _seoService;

    public PostRenderStrategy(IBlazorRenderer renderer, ISeoService seoService)
    {
        _renderer = renderer;
        _seoService = seoService;
    }

    /// <inheritdoc />
    public bool CanRender(ContentItem item) => item is Post;

    /// <inheritdoc />
    public async Task<string> RenderAsync(ContentItem item, SiteConfig siteConfig)
    {
        var post = (Post)item;
        
        // SEO 데이터 생성
        var canonicalUrl = _seoService.BuildCanonicalUrl(siteConfig, post.Url);
        var ogImage = _seoService.GetAbsoluteImageUrl(siteConfig, post.CoverImage ?? post.Image ?? siteConfig.OgImage);
        var structuredData = _seoService.GenerateArticleStructuredData(post, siteConfig, canonicalUrl, ogImage);
        var optimizedTitle = _seoService.FormatTitle(post.Title, siteConfig.Title);
        var optimizedDescription = !string.IsNullOrWhiteSpace(post.OptimizedDescription)
            ? post.OptimizedDescription
            : siteConfig.Description;

        // MainLayout 파라미터 구성
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

        return await _renderer.RenderComponentAsync<MainLayout>(layoutParams);
    }
}
