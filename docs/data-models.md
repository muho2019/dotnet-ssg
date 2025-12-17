# 데이터 모델 및 도메인 모델 정의

이 문서는 `dotnet-ssg` 프로젝트에서 사용될 핵심 데이터 모델과 도메인 모델을 C# 코드로 정의합니다.

## 1. 전역 설정 모델

### `SiteConfig.cs`

사이트의 전역 설정을 담는 모델입니다. `config.json` 파일과 매핑됩니다.

```csharp
namespace DotnetSsg.Models;

public class SiteConfig
{
    public string Title { get; set; } = "My Awesome Blog";
    public string BaseUrl { get; set; } = "https://example.com";
    public string Description { get; set; } = "A blog about something cool.";
}
```

## 2. 콘텐츠 기본 모델

### `ContentItem.cs`

모든 콘텐츠(포스트, 페이지 등)가 상속받는 기본 클래스입니다. 공통 속성을 정의합니다.

```csharp
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
```

## 3. 콘텐츠 상세 모델

### `Post.cs`

블로그 포스트를 나타내는 모델입니다. `ContentItem`을 상속받아 포스트 관련 속성을 추가합니다.

```csharp
namespace DotnetSsg.Models;

public class Post : ContentItem
{
    public DateTime Date { get; set; }
    public List<string> Tags { get; set; } = new();
}
```

### `Page.cs`

'About', 'Contact' 등 일반 정적 페이지를 나타내는 모델입니다. 현재는 추가 속성 없이 `ContentItem`을 그대로 사용하지만, 향후 확장을 위해 별도 클래스로 정의합니다.

```csharp
namespace DotnetSsg.Models;

public class Page : ContentItem
{
    // 페이지에만 필요한 추가 속성을 여기에 정의할 수 있습니다.
    // 예: public int Order { get; set; }
}
```
