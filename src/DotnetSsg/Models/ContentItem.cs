using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

public abstract class ContentItem
{
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    public string SourcePath { get; set; } = string.Empty;
    
    public string OutputPath { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;
}
