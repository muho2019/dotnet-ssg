using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class HtmlGenerator
{
    private readonly TemplateRenderer _templateRenderer;

    public HtmlGenerator(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task GenerateAsync(ContentItem item, SiteConfig siteConfig)
    {
        var templateName = item is Post ? "post" : "page";
        var contentTemplatePath = $"templates/{templateName}.liquid";
        var layoutTemplatePath = "templates/layout.liquid";

        // Render the inner content first
        var innerContent = await _templateRenderer.RenderAsync(contentTemplatePath, new { item });

        // Render the full page with the layout
        var fullHtml = await _templateRenderer.RenderAsync(layoutTemplatePath, new { item, site = siteConfig, content = innerContent });
        
        // Determine output path for pretty URLs (e.g., /posts/my-post/index.html)
        var relativePath = Path.GetRelativePath("content", item.SourcePath);
        var pathWithoutExtension = Path.ChangeExtension(relativePath, null);
        
        string outputPath;
        if (Path.GetFileNameWithoutExtension(item.SourcePath).Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            var parentDir = Path.GetDirectoryName(pathWithoutExtension);
            outputPath = Path.Combine("output", parentDir ?? string.Empty, "index.html");
        }
        else
        {
            outputPath = Path.Combine("output", pathWithoutExtension, "index.html");
        }

        var outputDir = Path.GetDirectoryName(outputPath);
        if (outputDir != null)
        {
            Directory.CreateDirectory(outputDir);
        }

        await File.WriteAllTextAsync(outputPath, fullHtml);
        Console.WriteLine($"Generated: {outputPath}");
    }
}
