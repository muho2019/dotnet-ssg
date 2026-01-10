using System.Text;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class SitemapGenerator : ISitemapGenerator
{
    public void Generate(SiteConfig config, List<ContentItem> contentItems, string outputDirectory, List<Post> posts,
        List<string> tags)
    {
        // 홈 페이지 및 태그 아카이브를 포함한 사이트맵 아이템 생성
        var sitemapItems = new List<ContentItem>(contentItems);

        // 홈 페이지 추가
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

        // 태그 아카이브 페이지 추가
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

        // 사이트맵 XML 생성
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

            // lastmod가 있으면 추가
            var lastMod = GetLastModifiedDate(item);
            if (lastMod != null)
            {
                sitemapContent.AppendLine($"    <lastmod>{lastMod:yyyy-MM-ddTHH:mm:sszzz}</lastmod>");
            }

            // changefreq 추가
            if (!string.IsNullOrEmpty(item.ChangeFrequency))
            {
                sitemapContent.AppendLine($"    <changefreq>{item.ChangeFrequency}</changefreq>");
            }

            // priority 추가
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
        // 명시적으로 설정된 경우 사용
        if (item.LastModified.HasValue)
            return item.LastModified.Value;

        // 포스트인 경우 날짜 사용
        if (item is Post post)
            return post.Date;

        // 날짜가 있는 페이지인 경우 사용
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