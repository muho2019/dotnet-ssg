namespace DotnetSsg.Models;

public class Post : ContentItem
{
    public DateTime Date { get; set; }
    public List<string> Tags { get; set; } = new();
}
