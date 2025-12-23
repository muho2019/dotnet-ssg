using YamlDotNet.Serialization;

namespace DotnetSsg.Models;

public class Page : ContentItem
{
    [YamlMember(Alias = "date")]
    public DateTime? Date { get; set; }

    // 페이지에만 필요한 추가 속성을 여기에 정의할 수 있습니다.
    // 예: public int Order { get; set; }
}
