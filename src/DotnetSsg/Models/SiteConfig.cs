using System.Text.Json.Serialization;

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
    public string? GithubUrl { get; set; }
    public string? GoogleAnalyticsId { get; set; }

    [JsonIgnore]
    public string NormalizedBaseUrl => string.IsNullOrWhiteSpace(BaseUrl)
        ? string.Empty
        : (BaseUrl.EndsWith("/") ? BaseUrl : BaseUrl + "/");

    [JsonIgnore]
    public Uri? BaseUri => Uri.TryCreate(NormalizedBaseUrl, UriKind.Absolute, out var uri) ? uri : null;

    [JsonIgnore]
    public string BasePath
    {
        get
        {
            if (BaseUri == null)
            {
                return "/";
            }

            var path = BaseUri.AbsolutePath;
            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }

            return path.EndsWith("/") ? path : path + "/";
        }
    }
}
