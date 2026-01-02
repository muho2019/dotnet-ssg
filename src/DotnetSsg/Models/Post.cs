using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

public class Post : ContentItem
{
    [YamlMember(Alias = "date")]
    public DateTime Date { get; set; }

    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = new();

    [YamlMember(Alias = "author")]
    public string? Author { get; set; }

    [YamlMember(Alias = "draft")]
    public bool Draft { get; set; } = false;

    [YamlMember(Alias = "cover_image")]
    public new string? CoverImage
    {
        get => base.CoverImage;
        set => base.CoverImage = value;
    }
}
