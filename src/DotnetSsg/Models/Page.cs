using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

/// <summary>
/// 일반 페이지 콘텐츠를 나타내는 모델입니다.
/// (예: About, Contact 등)
/// </summary>
public class Page : ContentItem
{
    /// <summary>
    /// 페이지 생성 또는 수정 날짜 (선택 사항)
    /// </summary>
    [YamlMember(Alias = "date")]
    public DateTime? Date { get; set; }

    // 페이지에만 필요한 추가 속성을 여기에 정의할 수 있습니다.
    // 예: public int Order { get; set; }
}
