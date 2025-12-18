namespace DotnetSsg.Models;

public abstract class ContentItem
{
    // Front Matter에서 추출되는 메타데이터
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // 파일 시스템 정보
    public string SourcePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    // 변환된 콘텐츠
    public string HtmlContent { get; set; } = string.Empty;
}
