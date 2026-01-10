using DotnetSsg.Models;

namespace DotnetSsg.Services;

/// <summary>
/// 검색 엔진 최적화(SEO) 관련 기능을 제공하는 서비스 인터페이스입니다.
/// </summary>
public interface ISeoService
{
    /// <summary>
    /// 표준(Canonical) URL을 생성합니다.
    /// </summary>
    /// <param name="siteConfig">사이트 설정</param>
    /// <param name="relativePath">상대 경로</param>
    /// <returns>완전한 절대 URL</returns>
    string BuildCanonicalUrl(SiteConfig siteConfig, string relativePath);

    /// <summary>
    /// 이미지의 절대 URL을 반환합니다.
    /// </summary>
    /// <param name="siteConfig">사이트 설정</param>
    /// <param name="imageUrl">이미지 경로 (상대 또는 절대)</param>
    /// <returns>이미지의 절대 URL 또는 null</returns>
    string? GetAbsoluteImageUrl(SiteConfig siteConfig, string? imageUrl);

    /// <summary>
    /// 기사(Article)에 대한 구조화된 데이터(JSON-LD)를 생성합니다.
    /// </summary>
    /// <param name="post">포스트 객체</param>
    /// <param name="siteConfig">사이트 설정</param>
    /// <param name="canonicalUrl">포스트의 표준 URL</param>
    /// <param name="imageUrl">대표 이미지 URL</param>
    /// <returns>JSON-LD 문자열</returns>
    string GenerateArticleStructuredData(Post post, SiteConfig siteConfig, string canonicalUrl, string? imageUrl);

    /// <summary>
    /// SEO에 최적화된 페이지 제목을 포맷팅합니다.
    /// </summary>
    /// <param name="pageTitle">개별 페이지 제목</param>
    /// <param name="siteTitle">사이트 전체 제목</param>
    /// <returns>포맷팅된 제목 문자열</returns>
    string FormatTitle(string pageTitle, string siteTitle);
}
