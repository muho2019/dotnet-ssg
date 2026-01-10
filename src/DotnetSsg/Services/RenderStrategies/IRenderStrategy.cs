using DotnetSsg.Models;

namespace DotnetSsg.Services.RenderStrategies;

/// <summary>
/// 콘텐츠 타입(Post, Page 등)에 따른 렌더링 전략을 정의하는 인터페이스입니다.
/// 전략 패턴을 사용하여 콘텐츠별로 다른 렌더링 로직을 적용합니다.
/// </summary>
public interface IRenderStrategy
{
    /// <summary>
    /// 이 전략이 주어진 콘텐츠 아이템을 처리할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="item">확인할 콘텐츠 아이템</param>
    /// <returns>처리 가능 여부</returns>
    bool CanRender(ContentItem item);

    /// <summary>
    /// 콘텐츠 아이템을 HTML 문자열로 렌더링합니다.
    /// </summary>
    /// <param name="item">렌더링할 콘텐츠 아이템</param>
    /// <param name="siteConfig">사이트 설정 정보</param>
    /// <returns>생성된 HTML 문자열</returns>
    Task<string> RenderAsync(ContentItem item, SiteConfig siteConfig);
}
