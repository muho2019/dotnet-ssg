using System.Text.RegularExpressions;
using DotnetSsg.Models;
using Markdig;
using YamlDotNet.Serialization;

namespace DotnetSsg.Services;

public class MarkdownParser
{
    private readonly IDeserializer _yamlDeserializer;

    public MarkdownParser()
    {
        // No need for a custom deserializer setup if we use YamlMember attributes
        _yamlDeserializer = new DeserializerBuilder().Build();
    }

    public async Task<ContentItem> ParseAsync(string filePath, string baseUrl)
    {
        var fileContent = await File.ReadAllTextAsync(filePath);

        var (frontMatter, markdownBody) = ExtractAndSeparateFrontMatter(fileContent);

        var htmlContent = Markdown.ToHtml(markdownBody);

        ContentItem item;
        var normalizedPath = filePath.Replace('\\', '/');
        if (normalizedPath.Contains("content/posts", StringComparison.OrdinalIgnoreCase))
        {
            var post = _yamlDeserializer.Deserialize<Post>(frontMatter);
            item = post;

            if (post.Date == default)
            {
                post.Date = File.GetCreationTime(filePath);
                Console.WriteLine(
                    $"Warning: Date not found or invalid in Front Matter for '{filePath}'. Using file creation time.");
            }

            // YamlDotNet handles converting single string or list of strings to List<string>
            if (post.Tags != null)
            {
                post.Tags = post.Tags.Select(t => t.ToLowerInvariant()).ToList();
            }

            // Set SEO defaults for posts
            if (item.Priority == 0.5) // Not set by user
            {
                item.Priority = 0.7; // Posts are more important than default
            }

            if (item.ChangeFrequency == "monthly") // Default value
            {
                item.ChangeFrequency = "never"; // Posts typically don't change
            }
        }
        else
        {
            // For pages, we can still deserialize into the base type to get Title, etc.
            item = _yamlDeserializer.Deserialize<Page>(frontMatter);

            // Set SEO defaults for pages
            if (item.Priority == 0.5) // Not set by user
            {
                item.Priority = 0.7; // Pages have good priority
            }

            if (item.ChangeFrequency == "monthly") // Default value
            {
                item.ChangeFrequency = "monthly";
            }
        }

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            item.Title = GetTitleFromFilePath(filePath);
            Console.WriteLine($"Warning: Title not found in Front Matter for '{filePath}'. Using '{item.Title}'.");
        }

        item.SourcePath = filePath;
        item.HtmlContent = htmlContent;
        item.Url = GenerateUrl(filePath, baseUrl);
        item.OutputPath = GenerateOutputPath(filePath);

        return item;
    }

    private (string frontMatter, string markdown) ExtractAndSeparateFrontMatter(string fileContent)
    {
        var match = Regex.Match(fileContent, @"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n?(.*)", RegexOptions.Singleline);
        if (match.Success)
        {
            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        return (string.Empty, fileContent);
    }

    private string GetTitleFromFilePath(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace('-', ' '));
    }

    private string GenerateUrl(string filePath, string baseUrl)
    {
        var relativePath = Path.GetRelativePath("content", filePath);
        var url = string.Concat(baseUrl, "/",
            relativePath.Replace(Path.DirectorySeparatorChar, '/').Replace(".md", string.Empty));

        if (Path.GetFileNameWithoutExtension(filePath).Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            // For "content/index.md", url becomes "" -> "/"
            // For "content/posts/index.md", url becomes "posts" -> "/posts/"
            return string.IsNullOrEmpty(url) ? "" : url + "/";
        }

        // For "content/posts/my-post.md", url becomes "posts/my-post" -> "/posts/my-post/"
        return url + "/";
    }

    private string GenerateOutputPath(string filePath)
    {
        var relativePath = Path.GetRelativePath("content", filePath);
        var fileName = Path.GetFileNameWithoutExtension(relativePath);
        var directory = Path.GetDirectoryName(relativePath) ?? string.Empty;

        // "content/about.md" -> "about/index.html"
        // "content/posts/my-first-post.md" -> "posts/my-first-post/index.html"
        if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            // "content/index.md" -> "index.html"
            // "content/posts/index.md" -> "posts/index.html"
            return string.IsNullOrEmpty(directory)
                ? "index.html"
                : Path.Combine(directory, "index.html").Replace(Path.DirectorySeparatorChar, '/');
        }

        var outputDir = string.IsNullOrEmpty(directory)
            ? fileName
            : Path.Combine(directory, fileName);

        return Path.Combine(outputDir, "index.html").Replace(Path.DirectorySeparatorChar, '/');
    }
}