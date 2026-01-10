using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

public abstract class ContentItem
{
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    /// <summary>
    /// SEO를 위한 최적화된 설명을 가져옵니다 (데스크톱의 경우 최대 160자, 모바일의 경우 120자)
    /// </summary>
    [YamlIgnore]
    public string OptimizedDescription
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Description))
                return string.Empty;

            const int maxLength = 160;
            if (Description.Length <= maxLength)
                return Description;

            // 단어 경계에서 자르기
            var truncated = Description.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > 0)
                truncated = truncated.Substring(0, lastSpace);

            return truncated + "...";
        }
    }

    [YamlMember(Alias = "image")]
    public string? Image { get; set; }

    [YamlMember(Alias = "imageAlt")]
    public string? ImageAlt { get; set; }

    public string SourcePath { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// SEO 우선순위 (0.0 - 1.0). 기본값은 0.5
    /// </summary>
    public double Priority { get; set; } = 0.5;

    /// <summary>
    /// 검색 엔진을 위한 변경 빈도 힌트: always, hourly, daily, weekly, monthly, yearly, never
    /// </summary>
    public string ChangeFrequency { get; set; } = "monthly";

    /// <summary>
    /// 사이트맵을 위한 마지막 수정 날짜. null인 경우 파일 수정 시간 또는 포스트 날짜를 사용
    /// </summary>
    public DateTime? LastModified { get; set; }

    // Notion 스타일 커버 이미지
    public string? CoverImage { get; set; }
}
