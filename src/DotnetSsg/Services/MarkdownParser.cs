using System.Text.RegularExpressions;
using DotnetSsg.Models;
using Markdig;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace DotnetSsg.Services;

/// <summary>
/// 마크다운 파일을 파싱하여 HTML과 메타데이터로 변환하는 클래스입니다.
/// </summary>
public class MarkdownParser : IMarkdownParser
{
    private readonly IDeserializer _yamlDeserializer;
    private readonly ILogger<MarkdownParser> _logger;
    private readonly IPathResolver _pathResolver;

    public MarkdownParser(ILogger<MarkdownParser> logger, IPathResolver pathResolver)
    {
        _logger = logger;
        _pathResolver = pathResolver;
        // YamlMember 특성을 사용하는 경우 별도의 Deserializer 설정이 필요 없습니다.
        _yamlDeserializer = new DeserializerBuilder().Build();
    }

    /// <inheritdoc />
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
        // content 루트 기준 상대 경로를 확인하여 포스트인지 판별
        var relativePathToRoot = Path.GetRelativePath(contentRoot, filePath).Replace('\\', '/');
        
        if (relativePathToRoot.StartsWith("posts/", StringComparison.OrdinalIgnoreCase))
        {
            var post = _yamlDeserializer.Deserialize<Post>(frontMatter);
            item = post;

            if (post.Date == default)
            {
                post.Date = File.GetCreationTime(filePath);
                _logger.LogWarning("Front Matter에서 날짜를 찾을 수 없거나 유효하지 않습니다: '{FilePath}'. 파일 생성 시간을 사용합니다.", filePath);
            }

            // YamlDotNet은 단일 문자열 또는 문자열 목록을 List<string>으로 변환 처리합니다.
            if (post.Tags != null)
            {
                post.Tags = post.Tags.Select(t => t.ToLowerInvariant()).ToList();
            }

            // 포스트에 대한 기본 SEO 설정
            if (item.Priority == 0.5) // 사용자에 의해 설정되지 않음
            {
                item.Priority = 0.7; // 포스트는 기본보다 중요도가 높음
            }

            if (item.ChangeFrequency == "monthly") // 기본값
            {
                item.ChangeFrequency = "never"; // 포스트는 일반적으로 변경되지 않음
            }
        }
        else
        {
            // 페이지의 경우 기본 타입으로 역직렬화하여 Title 등을 가져올 수 있습니다.
            item = _yamlDeserializer.Deserialize<Page>(frontMatter) ?? new Page();

            // 페이지에 대한 기본 SEO 설정
            if (item.Priority == 0.5) // 사용자에 의해 설정되지 않음
            {
                item.Priority = 0.7; // 페이지는 좋은 우선순위를 가짐
            }

            if (item.ChangeFrequency == "monthly") // 기본값
            {
                item.ChangeFrequency = "monthly";
            }
        }

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            item.Title = GetTitleFromFilePath(filePath);
            _logger.LogWarning("Front Matter에서 제목을 찾을 수 없습니다: '{FilePath}'. '{Title}'을(를) 사용합니다.", filePath, item.Title);
        }

        item.SourcePath = filePath;
        item.HtmlContent = htmlContent;
        // 경로 및 URL 계산을 PathResolver에 위임
        item.Url = _pathResolver.GetUrl(item, contentRoot);
        item.OutputPath = _pathResolver.GetOutputPath(item, contentRoot);

        return item;
    }

    /// <summary>
    /// 파일 내용에서 Front Matter와 마크다운 본문을 분리합니다.
    /// </summary>
    private (string frontMatter, string markdown) ExtractAndSeparateFrontMatter(string fileContent)
    {
        var match = Regex.Match(fileContent, @"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n?(.*)", RegexOptions.Singleline);
        if (match.Success)
        {
            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        return (string.Empty, fileContent);
    }

    /// <summary>
    /// 파일 경로에서 파일명을 추출하여 제목으로 사용합니다.
    /// </summary>
    private string GetTitleFromFilePath(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.Replace('-', ' '));
    }
}
