using System.Text;
using System.Xml;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class RssFeedGenerator : IRssFeedGenerator
{
    public void Generate(SiteConfig config, List<Post> posts, string outputDirectory)
    {
        var sortedPosts = posts.OrderByDescending(p => p.Date).Take(20).ToList();

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8
        };

        var outputPath = Path.Combine(outputDirectory, "feed.xml");

        using var writer = XmlWriter.Create(outputPath, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("rss");
        writer.WriteAttributeString("version", "2.0");
        writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");

        writer.WriteStartElement("channel");

        // Channel metadata
        writer.WriteElementString("title", config.Title);
        writer.WriteElementString("description", config.Description);
        var baseUri = config.BaseUri;
        writer.WriteElementString("link", baseUri?.ToString() ?? string.Empty);

        // Self link (Atom namespace)
        var feedHref = baseUri != null ? new Uri(baseUri, "feed.xml").ToString() : "feed.xml";
        writer.WriteStartElement("atom", "link", "http://www.w3.org/2005/Atom");
        writer.WriteAttributeString("href", feedHref);
        writer.WriteAttributeString("rel", "self");
        writer.WriteAttributeString("type", "application/rss+xml");
        writer.WriteEndElement();

        writer.WriteElementString("language", config.Language);
        writer.WriteElementString("generator", "dotnet-ssg");

        if (sortedPosts.Any())
        {
            writer.WriteElementString("lastBuildDate", sortedPosts.First().Date.ToString("R"));
        }

        // Items
        foreach (var post in sortedPosts)
        {
            writer.WriteStartElement("item");

            writer.WriteElementString("title", post.Title);

            var postUrl = baseUri != null ? new Uri(baseUri, post.Url).ToString() : post.Url;
            writer.WriteElementString("link", postUrl);
            writer.WriteElementString("guid", postUrl);

            if (!string.IsNullOrEmpty(post.Description))
            {
                writer.WriteElementString("description", post.Description);
            }

            // Content (full HTML content)
            if (!string.IsNullOrEmpty(post.HtmlContent))
            {
                writer.WriteStartElement("content", "encoded", "http://purl.org/rss/1.0/modules/content/");
                writer.WriteCData(post.HtmlContent);
                writer.WriteEndElement();
            }

            writer.WriteElementString("pubDate", post.Date.ToString("R"));

            // Tags as categories
            if (post.Tags != null)
            {
                foreach (var tag in post.Tags)
                {
                    writer.WriteElementString("category", tag);
                }
            }

            writer.WriteEndElement(); // item
        }

        writer.WriteEndElement(); // channel
        writer.WriteEndElement(); // rss
        writer.WriteEndDocument();

        Console.WriteLine("RSS feed generated at feed.xml");
    }
}
