using DotnetSsg.Services;
using DotnetSsg.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetSsg.Tests.Services;

public class MarkdownParserTests
{
    private readonly Mock<ILogger<MarkdownParser>> _loggerMock;
    private readonly IMarkdownParser _parser;
    private readonly string _testContentPath;

    public MarkdownParserTests()
    {
        _loggerMock = new Mock<ILogger<MarkdownParser>>();
        _parser = new MarkdownParser(_loggerMock.Object);
        // Create a temporary test content directory
        _testContentPath = Path.Combine(Path.GetTempPath(), "dotnet-ssg-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_testContentPath);
        Directory.CreateDirectory(Path.Combine(_testContentPath, "content"));
    }

    [Fact]
    public async Task ParseAsync_ShouldExtractFrontMatterAndContent()
    {
        // Arrange
        var contentDir = Path.Combine(_testContentPath, "content");
        var filePath = Path.Combine(contentDir, "posts", "test-post.md");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var content = @"---
title: Test Post
date: 2023-01-01
tags: [csharp, testing]
---
# Hello World
This is a test post.";

        await File.WriteAllTextAsync(filePath, content);

        // Act
        var result = await _parser.ParseAsync(filePath, contentDir);

        // Assert
        Assert.Equal("Test Post", result.Title);
        Assert.Contains("Hello World", result.HtmlContent);
        Assert.Contains("This is a test post.", result.HtmlContent);
        Assert.IsType<DotnetSsg.Models.Post>(result);
        var post = (DotnetSsg.Models.Post)result;
        Assert.Equal(new DateTime(2023, 1, 1), post.Date);
        Assert.Equal(2, post.Tags?.Count);
        
        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task ParseAsync_ShouldHandleMissingFrontMatter()
    {
        // Arrange
        var contentDir = Path.Combine(_testContentPath, "content");
        var filePath = Path.Combine(contentDir, "no-fm.md");
        var content = @"# Just Markdown
No front matter here.";

        await File.WriteAllTextAsync(filePath, content);

        // Act
        var result = await _parser.ParseAsync(filePath, contentDir);

        // Assert
        // Title should be derived from filename "no-fm" -> "No Fm"
        Assert.Equal("No Fm", result.Title);
        Assert.Contains("Just Markdown", result.HtmlContent);
        
        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task ParseAsync_ShouldGenerateCorrectUrls_ForRootIndex()
    {
        // Arrange
        var contentDir = Path.Combine(_testContentPath, "content");
        var filePath = Path.Combine(contentDir, "index.md");
        var content = "---\ntitle: Home\n---\nHome page";
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var result = await _parser.ParseAsync(filePath, contentDir);

        // Assert
        Assert.Equal(string.Empty, result.Url); // Root index usually maps to / or empty string base
        Assert.Equal("index.html", result.OutputPath);

        // Cleanup
        File.Delete(filePath);
    }
}
