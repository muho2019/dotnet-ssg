namespace DotnetSsg.Models;

public class SiteConfig
{
    public string Title { get; set; } = "My Awesome Blog";
    public string Description { get; set; } = "A blog about something cool.";
    public string BaseUrl { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Language { get; set; } = "ko";
    public string? OgImage { get; set; }
    public string OgImageAlt { get; set; } = "Site cover image";
    public string? TwitterSite { get; set; }
    public string? TwitterCreator { get; set; }
    public string?  GithubUrl { get; set; }
}
