using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

public abstract class ContentItem
{
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "image")]
    public string? Image { get; set; }

    [YamlMember(Alias = "imageAlt")]
    public string? ImageAlt { get; set; }

    public string SourcePath { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// SEO priority (0.0 - 1.0). Default is 0.5
    /// </summary>
    public double Priority { get; set; } = 0.5;

    /// <summary>
    /// Change frequency hint for search engines: always, hourly, daily, weekly, monthly, yearly, never
    /// </summary>
    public string ChangeFrequency { get; set; } = "monthly";

    /// <summary>
    /// Last modified date for sitemap. If null, will use file modification time or post date
    /// </summary>
    public DateTime? LastModified { get; set; }

    // Notion 스타일 커버 이미지
    public string? CoverImage { get; set; }
}
