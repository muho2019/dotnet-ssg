using System.Text.RegularExpressions;
using DotnetSsg.Models;
using Markdig;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace DotnetSsg.Services;

public class MarkdownParser : IMarkdownParser
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly ILogger<MarkdownParser> _logger;

    public MarkdownParser(ILogger<MarkdownParser> logger)
    {
        _logger = logger;
        // No need for a custom deserializer setup if we use YamlMember attributes
        _yamlDeserializer = new DeserializerBuilder().Build();
    }

    public async Task<ContentItem> ParseAsync(string filePath, string contentRoot)
    {
        var fileContent = await File.ReadAllTextAsync(filePath);

        var (frontMatter, markdownBody) = ExtractAndSeparateFrontMatter(fileContent);

        // Markdig 파이프라인 설정: GFM + 고급 기능
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions() // 테이블, 취소선, 자동링크 등
            .UseGenericAttributes() // {.class} 문법 지원
            .UsePipeTables() // 파이프 테이블
            .UseTaskLists() // 체크박스 리스트
            .UseAutoLinks() // 자동 링크 변환
            .UseEmphasisExtras() // 굵게, 기울임 등
            .Build();

        var htmlContent = Markdown.ToHtml(markdownBody, pipeline);

        ContentItem item;
        var normalizedPath = filePath.Replace('\\', '/');
        // Check relative to content root to determine if it's a post
        var relativePathToRoot = Path.GetRelativePath(contentRoot, filePath).Replace('\\', '/');
        
        if (relativePathToRoot.StartsWith("posts/", StringComparison.OrdinalIgnoreCase))
        {
            var post = _yamlDeserializer.Deserialize<Post>(frontMatter);
            item = post;

            if (post.Date == default)
            {
                post.Date = File.GetCreationTime(filePath);
                _logger.LogWarning("Date not found or invalid in Front Matter for '{FilePath}'. Using file creation time.", filePath);
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
            item = _yamlDeserializer.Deserialize<Page>(frontMatter) ?? new Page();

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
            _logger.LogWarning("Title not found in Front Matter for '{FilePath}'. Using '{Title}'.", filePath, item.Title);
        }

        item.SourcePath = filePath;
        item.HtmlContent = htmlContent;
        item.Url = GenerateUrl(filePath, contentRoot);
        item.OutputPath = GenerateOutputPath(filePath, contentRoot);

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

    private string GenerateUrl(string filePath, string contentRoot)
    {
        var relativePath = Path.GetRelativePath(contentRoot, filePath);
        var directory = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var normalizedDirectory = directory.Replace(Path.DirectorySeparatorChar, '/');
        var fileName = Path.GetFileNameWithoutExtension(relativePath);

        if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(normalizedDirectory))
            {
                return string.Empty;
            }

            return normalizedDirectory.EndsWith("/")
                ? normalizedDirectory
                : normalizedDirectory + "/";
        }

        var prefix = string.IsNullOrEmpty(normalizedDirectory)
            ? string.Empty
            : (normalizedDirectory.EndsWith("/") ? normalizedDirectory : normalizedDirectory + "/");

        return prefix + fileName + "/";
    }

    private string GenerateOutputPath(string filePath, string contentRoot)
    {
        var relativePath = Path.GetRelativePath(contentRoot, filePath);
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