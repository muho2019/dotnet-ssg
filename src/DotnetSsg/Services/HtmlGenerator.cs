using DotnetSsg.Models;
using DotnetSsg.Components.Pages;
using DotnetSsg.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace DotnetSsg.Services;

public class HtmlGenerator
{
    private readonly BlazorRenderer _blazorRenderer;

    public HtmlGenerator(BlazorRenderer blazorRenderer)
    {
        _blazorRenderer = blazorRenderer;
    }

    public async Task GenerateAsync(ContentItem item, SiteConfig siteConfig)
    {
        string fullHtml;

        if (item is Post post)
        {
            // Post를 직접 MainLayout의 Body로 전달
            var layoutParams = new Dictionary<string, object?>
            {
                ["ChildContent"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<PostPage>(0);
                    builder.AddAttribute(1, "Post", post);
                    builder.CloseComponent();
                }),
                ["Title"] = post.Title,
                ["Description"] = post.Description ?? siteConfig.Description,
                ["SiteTitle"] = siteConfig.Title,
                ["Author"] = siteConfig.Author,
                ["GithubUrl"] = siteConfig.GithubUrl
            };

            fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);
        }
        else
        {
            // Page를 직접 MainLayout의 Body로 전달
            var layoutParams = new Dictionary<string, object?>
            {
                ["ChildContent"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<PageTemplate>(0);
                    builder.AddAttribute(1, "Page", item);
                    builder.CloseComponent();
                }),
                ["Title"] = item.Title,
                ["Description"] = item.Description ?? siteConfig.Description,
                ["SiteTitle"] = siteConfig.Title,
                ["Author"] = siteConfig.Author,
                ["GithubUrl"] = siteConfig.GithubUrl
            };

            fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);
        }

        // 출력 경로 결정
        var relativePath = Path.GetRelativePath("content", item.SourcePath);
        var pathWithoutExtension = Path.ChangeExtension(relativePath, null);

        string outputPath;
        var fileName = Path.GetFileNameWithoutExtension(item.SourcePath);
        if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            var parentDir = Path.GetDirectoryName(pathWithoutExtension);
            outputPath = Path.Combine("output", parentDir ?? string.Empty, "index.html");
        }
        else
        {
            outputPath = Path.Combine("output", pathWithoutExtension, "index.html");
        }

        // 디렉토리 생성 및 파일 저장
        var outputDir = Path.GetDirectoryName(outputPath);
        if (outputDir != null)
        {
            Directory.CreateDirectory(outputDir);
        }

        await File.WriteAllTextAsync(outputPath, fullHtml);
        Console.WriteLine($"Generated: {outputPath}");
    }

    public async Task GenerateIndexAsync(SiteConfig siteConfig, List<Post> posts, string outputDirectory)
    {
        var layoutParams = new Dictionary<string, object?>
        {
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<IndexPage>(0);
                builder.AddAttribute(1, "Site", siteConfig);
                builder.AddAttribute(2, "Posts", posts);
                builder.CloseComponent();
            }),
            ["Title"] = siteConfig.Title,
            ["Description"] = siteConfig.Description,
            ["SiteTitle"] = siteConfig.Title,
            ["Author"] = siteConfig.Author,
            ["GithubUrl"] = siteConfig.GithubUrl
        };

        var fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);

        var outputPath = Path.Combine(outputDirectory, "index.html");
        await File.WriteAllTextAsync(outputPath, fullHtml);
        Console.WriteLine($"Generated: {outputPath}");
    }

    public async Task GenerateTagArchiveAsync(SiteConfig siteConfig, string tag, List<Post> posts,
        string outputDirectory)
    {
        var layoutParams = new Dictionary<string, object?>
        {
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<TagArchive>(0);
                builder.AddAttribute(1, "Site", siteConfig);
                builder.AddAttribute(2, "Tag", tag);
                builder.AddAttribute(3, "Posts", posts);
                builder.CloseComponent();
            }),
            ["Title"] = $"{tag} - {siteConfig.Title}",
            ["Description"] = $"{tag} 태그가 포함된 포스트",
            ["SiteTitle"] = siteConfig.Title,
            ["Author"] = siteConfig.Author,
            ["GithubUrl"] = siteConfig.GithubUrl
        };

        var fullHtml = await _blazorRenderer.RenderComponentAsync<MainLayout>(layoutParams);

        var tagDir = Path.Combine(outputDirectory, "tags", tag);
        Directory.CreateDirectory(tagDir);

        var outputPath = Path.Combine(tagDir, "index.html");
        await File.WriteAllTextAsync(outputPath, fullHtml);
        Console.WriteLine($"Generated: {outputPath}");
    }
}