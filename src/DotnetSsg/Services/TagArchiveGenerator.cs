using DotnetSsg.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSsg.Services;

public class TagArchiveGenerator
{
    private readonly TemplateRenderer _templateRenderer;

    public TagArchiveGenerator(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task GenerateAsync(IEnumerable<ContentItem> contentItems, SiteConfig siteConfig)
    {
        var posts = contentItems.OfType<Post>().ToList();

        var postsByTag = posts
            .SelectMany(p => p.Tags.Select(tag => new { Tag = tag, Post = p }))
            .GroupBy(x => x.Tag)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Post).OrderByDescending(p => p.Date).ToList());

        foreach (var (tag, tagPosts) in postsByTag)
        {
            var model = new { tag, posts = tagPosts, site = siteConfig };
            var templatePath = "templates/tag_archive.liquid";
            var htmlContent = await _templateRenderer.RenderAsync(templatePath, model);

            var outputPath = $"output/tags/{tag}/index.html";
            var outputDir = Path.GetDirectoryName(outputPath);
            if (outputDir != null)
            {
                Directory.CreateDirectory(outputDir);
            }

            await File.WriteAllTextAsync(outputPath, htmlContent);
            Console.WriteLine($"Generated: {outputPath}");
        }
    }
}
