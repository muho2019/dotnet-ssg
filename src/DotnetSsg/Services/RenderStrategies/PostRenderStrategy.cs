using DotnetSsg.Components.Pages;
using DotnetSsg.Models;

namespace DotnetSsg.Services.RenderStrategies;

public class PostRenderStrategy : IRenderStrategy
{
    private readonly IBlazorRenderer _renderer;

    public PostRenderStrategy(IBlazorRenderer renderer)
    {
        _renderer = renderer;
    }

    public bool CanRender(ContentItem item) => item is Post;

    public async Task<string> RenderAsync(ContentItem item, SiteConfig siteConfig)
    {
        var post = (Post)item;
        var parameters = new Dictionary<string, object?>
        {
            { "Post", post },
            { "Config", siteConfig }
        };

        return await _renderer.RenderComponentAsync<PostPage>(parameters);
    }
}
