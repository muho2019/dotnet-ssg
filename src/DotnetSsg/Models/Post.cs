using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

/// <summary>
/// 블로그 포스트 콘텐츠를 나타내는 모델입니다.
/// 마크다운 파일의 Front Matter와 본문 내용을 포함합니다.
/// </summary>
public class Post : ContentItem
{
    /// <summary>
    /// 포스트 작성 날짜
    /// </summary>
    [YamlMember(Alias = "date")]
    public DateTime Date { get; set; }

    private List<string> _tags = new();

    /// <summary>
    /// 포스트 태그 목록
    /// </summary>
    [YamlMember(Alias = "tags")]
    public List<string> Tags 
    { 
        get => _tags;
        set => _tags = value ?? new List<string>();
    }

    /// <summary>
    /// 태그 목록의 읽기 전용 뷰
    /// </summary>
    public IReadOnlyList<string> ReadOnlyTags => _tags.AsReadOnly();


    /// <summary>
    /// 포스트 작성자
    /// </summary>
    [YamlMember(Alias = "author")]
    public string? Author { get; set; }

    /// <summary>
    /// 초안 여부 (true인 경우 빌드에서 제외될 수 있음)
    /// </summary>
    [YamlMember(Alias = "draft")]
    public bool Draft { get; set; } = false;

    /// <summary>
    /// 커버 이미지 경로 (부모 속성 재정의)
    /// </summary>
    [YamlMember(Alias = "cover_image")]
    public new string? CoverImage
    {
        get => base.CoverImage;
        set => base.CoverImage = value;
    }
}
