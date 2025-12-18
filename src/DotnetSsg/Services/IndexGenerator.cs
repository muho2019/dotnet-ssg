using DotnetSsg.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSsg.Services;

public class IndexGenerator
{
    private readonly TemplateRenderer _templateRenderer;

    public IndexGenerator(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task GenerateAsync(IEnumerable<ContentItem> contentItems, SiteConfig siteConfig)
    {
        // Filter for posts and order them by date descending
        var posts = contentItems.OfType<Post>()
                                .OrderByDescending(p => p.Date)
                                .ToList();

        var model = new { posts, site = siteConfig };
        var templatePath = "templates/index.liquid";
        var htmlContent = await _templateRenderer.RenderAsync(templatePath, model);

        var outputPath = "output/index.html";
        var outputDir = Path.GetDirectoryName(outputPath);
        if (outputDir != null)
        {
            Directory.CreateDirectory(outputDir);
        }

        await File.WriteAllTextAsync(outputPath, htmlContent);
        Console.WriteLine($"Generated: {outputPath}");
    }
}
