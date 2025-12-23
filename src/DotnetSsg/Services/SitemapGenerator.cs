using System.Text;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class SitemapGenerator
{
    public void Generate(SiteConfig config, List<ContentItem> contentItems, string outputDirectory, List<Post> posts, IEnumerable<string> tags)
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
            SourcePath = Path.Combine(Path.GetDirectoryName(contentItems.First().SourcePath) ?? "", "..", "..", "templates", "index.liquid"),
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
                SourcePath = Path.Combine(Path.GetDirectoryName(contentItems.First().SourcePath) ?? "", "..", "..", "templates", "tag_archive.liquid"),
                LastModified = posts
                    .Where(p => p.Tags != null && p.Tags.Contains(tag))
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
            var baseUrl = config.BaseUrl.EndsWith("/") ? config.BaseUrl : config.BaseUrl + "/";
            var location = new Uri(new Uri(baseUrl), item.Url.TrimStart('/')).ToString();

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

        // Otherwise, try to get file modification time
        if (File.Exists(item.SourcePath))
            return File.GetLastWriteTime(item.SourcePath);

        return null;
    }

    private static string XmlEscape(string unescaped)
    {
        return unescaped.Replace("&", "&amp;")
                      .Replace("'", "&apos;")
                      .Replace("\"", "&quot;")
                      .Replace(">", "&gt;")
                      .Replace("<", "&lt;");
    }
}
