using System.Text;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class SitemapGenerator
{
    public void Generate(SiteConfig config, IEnumerable<ContentItem> items, string outputDirectory)
    {
        var sitemapContent = new StringBuilder();
        sitemapContent.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sitemapContent.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

        foreach (var item in items)
        {
            var baseUrl = config.BaseUrl.EndsWith("/") ? config.BaseUrl : config.BaseUrl + "/";
            var location = new Uri(new Uri(baseUrl), item.Url.TrimStart('/')).ToString();

            sitemapContent.AppendLine("  <url>");
            sitemapContent.AppendLine($"    <loc>{XmlEscape(location)}</loc>");
            
            if (item is Post post)
            {
                sitemapContent.AppendLine($"    <lastmod>{post.Date:yyyy-MM-dd}</lastmod>");
            }
            
            sitemapContent.AppendLine("  </url>");
        }

        sitemapContent.AppendLine("</urlset>");

        var outputPath = Path.Combine(outputDirectory, "sitemap.xml");
        File.WriteAllText(outputPath, sitemapContent.ToString());
        Console.WriteLine("Sitemap generated at sitemap.xml");
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
