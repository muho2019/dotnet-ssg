using DotnetSsg.Models;

namespace DotnetSsg.Services;

public interface IBlazorRenderer : IAsyncDisposable
{
    Task<string> RenderComponentAsync<TComponent>(Dictionary<string, object?> parameters) where TComponent : Microsoft.AspNetCore.Components.IComponent;
}

public interface IBuildService
{
    Task<bool> BuildAsync(string workingDirectory, string outputPath = "output", bool includeDrafts = false);
}

public interface IConfigLoader
{
    Task<SiteConfig> LoadConfigAsync(string? configPath = null);
}

public interface IFileScanner
{
    IEnumerable<string> Scan(string directory, string extension);
}

public interface IStaticFileCopier
{
    void Copy(string sourceDirectory, string destinationDirectory);
}

public interface IMarkdownParser
{
    Task<ContentItem> ParseAsync(string filePath, string contentRoot);
}

public interface IHtmlGenerator
{
    Task GenerateAsync(ContentItem item, SiteConfig siteConfig);
    Task GenerateIndexAsync(SiteConfig siteConfig, List<Post> posts, string outputDirectory);
    Task GenerateTagArchiveAsync(SiteConfig siteConfig, string tag, List<Post> posts, string outputDirectory);
}

public interface ICssBuilder
{
    Task BuildTailwindCssAsync(string workingDirectory);
}

public interface ISitemapGenerator
{
    void Generate(SiteConfig config, List<ContentItem> contentItems, string outputDirectory, List<Post> posts, List<string> tags);
}

public interface IRobotsTxtGenerator
{
    void Generate(SiteConfig config, string outputDirectory);
}

public interface IRssFeedGenerator
{
    void Generate(SiteConfig config, List<Post> posts, string outputDirectory);
}
