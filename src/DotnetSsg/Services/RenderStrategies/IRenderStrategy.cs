using DotnetSsg.Models;

namespace DotnetSsg.Services.RenderStrategies;

public interface IRenderStrategy
{
    bool CanRender(ContentItem item);
    Task<string> RenderAsync(ContentItem item, SiteConfig siteConfig);
}
