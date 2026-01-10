using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

public class Post : ContentItem
{
    [YamlMember(Alias = "date")]
    public DateTime Date { get; set; }

    private List<string> _tags = new();

    [YamlMember(Alias = "tags")]
    public List<string> Tags 
    { 
        get => _tags;
        set => _tags = value ?? new List<string>();
    }

    public IReadOnlyList<string> ReadOnlyTags => _tags.AsReadOnly();


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
