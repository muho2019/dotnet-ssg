using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class RobotsTxtGenerator : IRobotsTxtGenerator
{
    public void Generate(SiteConfig config, string outputDirectory)
    {
        var sitemapUrl = config.BaseUri != null
            ? new Uri(config.BaseUri, "sitemap.xml").ToString()
            : "sitemap.xml";

        var robotsTxt = $@"User-agent: *
Allow: /

Sitemap: {sitemapUrl}
";

        var robotsPath = Path.Combine(outputDirectory, "robots.txt");
        File.WriteAllText(robotsPath, robotsTxt);
        Console.WriteLine("robots.txt generated");
    }
}
