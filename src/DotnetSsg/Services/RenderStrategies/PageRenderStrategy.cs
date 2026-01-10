using DotnetSsg.Components.Pages;
using DotnetSsg.Models;

namespace DotnetSsg.Services.RenderStrategies;

public class PageRenderStrategy : IRenderStrategy
{
    private readonly IBlazorRenderer _renderer;

    public PageRenderStrategy(IBlazorRenderer renderer)
    {
        _renderer = renderer;
    }

    public bool CanRender(ContentItem item) => item is Page;

    public async Task<string> RenderAsync(ContentItem item, SiteConfig siteConfig)
    {
        var page = (Page)item;
        var parameters = new Dictionary<string, object?>
        {
            { "Page", page },
            { "Config", siteConfig }
        };

        return await _renderer.RenderComponentAsync<PageTemplate>(parameters);
    }
}
