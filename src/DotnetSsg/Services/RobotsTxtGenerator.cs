using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class RobotsTxtGenerator
{
    public void Generate(SiteConfig config, string outputDirectory)
    {
        var robotsTxt = $@"User-agent: *
Allow: /

Sitemap: {(config.BaseUrl.EndsWith("/") ? config.BaseUrl : config.BaseUrl + "/")}sitemap.xml
";

        var robotsPath = Path.Combine(outputDirectory, "robots.txt");
        File.WriteAllText(robotsPath, robotsTxt);
        Console.WriteLine("robots.txt generated");
    }
}
