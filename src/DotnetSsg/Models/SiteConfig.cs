namespace DotnetSsg.Models;

public class SiteConfig
{
    public string Title { get; set; } = "My Awesome Blog";
    public string BaseUrl { get; set; } = "https://example.com";
    public string Description { get; set; } = "A blog about something cool.";
    public string RepositoryName { get; set; } = "dotnet-ssg";
}
