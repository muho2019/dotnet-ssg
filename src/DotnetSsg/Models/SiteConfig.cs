namespace DotnetSsg.Models;

public class SiteConfig
{
    public string Title { get; set; } = "My Awesome Blog";
    public string Description { get; set; } = "A blog about something cool.";
    public string GitHubRepositoryName { get; set; } = "dotnet-ssg";
    public string BaseUrl => string.IsNullOrWhiteSpace(GitHubRepositoryName)
        ? "/"
        : $"/{GitHubRepositoryName}";
}
