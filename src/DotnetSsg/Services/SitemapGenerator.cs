using System.Text;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class SitemapGenerator
{
    public void Generate(SiteConfig config, List<ContentItem> contentItems, string outputDirectory, List<Post> posts,
        IEnumerable<string> tags)
    {
        // Build sitemap items including home page and tag archives
        var sitemapItems = new List<ContentItem>(contentItems);

        // Add home page
        var homePage = new Page
        {
            Title = "Home",
            Description = "Home page",
            Url = "",
            Priority = 1.0,
            ChangeFrequency = "weekly",
            SourcePath = "index.html",
            LastModified = DateTime.Now
        };
        sitemapItems.Add(homePage);

        // Add tag archive pages
        foreach (var tag in tags)
        {
            var tagPage = new Page
            {
                Title = $"{tag} - Posts",
                Description = $"Posts tagged with {tag}",
                Url = $"tags/{tag}/",
                Priority = 0.6,
                ChangeFrequency = "monthly",
                SourcePath = $"tags/{tag}/index.html",
                LastModified = posts
                    .Where(p => p.Tags.Contains(tag))
                    .Max(p => (DateTime?)p.Date) ?? DateTime.Now
            };
            sitemapItems.Add(tagPage);
        }

        // Generate sitemap XML
        var sitemapContent = new StringBuilder();
        sitemapContent.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sitemapContent.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

        foreach (var item in sitemapItems)
        {
            var location = config.BaseUri != null
                ? new Uri(config.BaseUri, item.Url).ToString()
                : item.Url;

            sitemapContent.AppendLine("  <url>");
            sitemapContent.AppendLine($"    <loc>{XmlEscape(location)}</loc>");

            // Add lastmod if available
            var lastMod = GetLastModifiedDate(item);
            if (lastMod != null)
            {
                sitemapContent.AppendLine($"    <lastmod>{lastMod:yyyy-MM-ddTHH:mm:sszzz}</lastmod>");
            }

            // Add changefreq
            if (!string.IsNullOrEmpty(item.ChangeFrequency))
            {
                sitemapContent.AppendLine($"    <changefreq>{item.ChangeFrequency}</changefreq>");
            }

            // Add priority
            if (item.Priority > 0)
            {
                sitemapContent.AppendLine($"    <priority>{item.Priority:F1}</priority>");
            }

            sitemapContent.AppendLine("  </url>");
        }

        sitemapContent.AppendLine("</urlset>");

        var outputPath = Path.Combine(outputDirectory, "sitemap.xml");
        File.WriteAllText(outputPath, sitemapContent.ToString());
        Console.WriteLine("Sitemap generated at sitemap.xml");
    }

    private DateTime? GetLastModifiedDate(ContentItem item)
    {
        // If explicitly set, use it
        if (item.LastModified.HasValue)
            return item.LastModified.Value;

        // For posts, use the date
        if (item is Post post)
            return post.Date;

        // For pages with date, use it
        if (item is Page page && page.Date.HasValue)
            return page.Date.Value;

        return null;
    }

    private static string XmlEscape(string text)
    {
        return text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}