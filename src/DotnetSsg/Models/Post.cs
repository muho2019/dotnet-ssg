using YamlDotNet.Serialization;
using System;
using System.Collections.Generic;

namespace DotnetSsg.Models;

public class Post : ContentItem
{
    [YamlMember(Alias = "date")]
    public DateTime Date { get; set; }
    
    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = new();

    [YamlMember(Alias = "author")]
    public string? Author { get; set; }
    
    [YamlMember(Alias = "cover_image")]
    public new string? CoverImage 
    { 
        get => base.CoverImage;
        set => base.CoverImage = value;
    }
}
