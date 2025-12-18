using System.Text.RegularExpressions;
using DotnetSsg.Models;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DotnetSsg.Services;

public class MarkdownParser
{
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly IDeserializer _yamlDeserializer;

    public MarkdownParser()
    {
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions() // GFM extensions like tables, strikethrough
            .UseYamlFrontMatter() // Enable YamlFrontMatter extension
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<ContentItem> ParseAsync(string filePath)
    {
        var fileContent = await File.ReadAllTextAsync(filePath);
        var frontMatter = ExtractFrontMatter(fileContent);
        var markdownBody = RemoveFrontMatter(fileContent);

        var htmlContent = Markdig.Markdown.ToHtml(markdownBody, _markdownPipeline);

        // Determine content type based on path
        ContentItem item;
        if (filePath.Contains("content/posts", StringComparison.OrdinalIgnoreCase))
        {
            item = new Post();
            if (frontMatter.ContainsKey("date") && DateTime.TryParse(frontMatter["date"].ToString(), out var date))
            {
                ((Post)item).Date = date;
            }
            else
            {
                // Fallback to file creation time or current date if date is missing/invalid
                ((Post)item).Date = File.GetCreationTime(filePath);
                Console.WriteLine($"Warning: Date not found or invalid in Front Matter for '{filePath}'. Using file creation time.");
            }
            if (frontMatter.ContainsKey("tags"))
            {
                // YamlDotNet might parse single tag as string, multiple as List<object>
                if (frontMatter["tags"] is IList<object> tagsList)
                {
                    ((Post)item).Tags = tagsList.Select(t => t.ToString()?.ToLowerInvariant() ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList();
                }
                else if (frontMatter["tags"] is string singleTag)
                {
                    ((Post)item).Tags.Add(singleTag.ToLowerInvariant());
                }
            }
        }
        else
        {
            item = new Page();
            // Page specific properties can be set here if needed
        }

        item.SourcePath = filePath;
        item.HtmlContent = htmlContent;
        item.Title = GetTitle(frontMatter, filePath);
        item.Description = frontMatter.ContainsKey("description") ? frontMatter["description"].ToString() : null;
        item.Url = GenerateUrl(filePath); // Placeholder for now

        return item;
    }

    private Dictionary<string, object> ExtractFrontMatter(string fileContent)
    {
        var match = Regex.Match(fileContent, @"^---\s*$(.*?)^---\s*$", RegexOptions.Singleline | RegexOptions.Multiline);
        if (match.Success)
        {
            var yaml = match.Groups[1].Value;
            try
            {
                return _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                Console.Error.WriteLine($"Warning: Invalid YAML Front Matter. Details: {ex.Message}");
                // Return empty dictionary on error, to proceed with markdown body
                return new Dictionary<string, object>();
            }
        }
        return new Dictionary<string, object>();
    }

    private string RemoveFrontMatter(string fileContent)
    {
        return Regex.Replace(fileContent, @"^---\s*$(.*?)^---\s*$", string.Empty, RegexOptions.Singleline | RegexOptions.Multiline).TrimStart();
    }

    private string GetTitle(Dictionary<string, object> frontMatter, string filePath)
    {
        if (frontMatter.ContainsKey("title") && !string.IsNullOrWhiteSpace(frontMatter["title"].ToString()))
        {
            return frontMatter["title"].ToString()!;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var title = ToTitleCase(fileName);
        Console.WriteLine($"Warning: Title not found in Front Matter for '{filePath}'. Using '{title}'.");
        return title;
    }

    private string ToTitleCase(string input)
    {
        // Simple title case for file names, replace hyphens with spaces and capitalize words
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.Replace('-', ' '));
    }

    private string GenerateUrl(string filePath)
    {
        // This is a simplified URL generation, will be refined later
        var relativePath = Path.GetRelativePath("content", filePath);
        // Standardize path separators to '/' for URLs
        var url = relativePath.Replace(Path.DirectorySeparatorChar, '/').Replace(".md", "/");
        if (url.EndsWith("index/"))
        {
            url = url.Replace("index/", "");
        }
        return "/" + url.ToLowerInvariant();
    }
}
