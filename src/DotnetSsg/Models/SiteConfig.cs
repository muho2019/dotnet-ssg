using System.Text.Json.Serialization;

namespace DotnetSsg.Models;

/// <summary>
/// 사이트 전체 설정을 나타내는 클래스입니다.
/// config.json 파일에서 로드됩니다.
/// </summary>
public class SiteConfig
{
    /// <summary>
    /// 사이트 제목
    /// </summary>
    public string Title { get; set; } = "My Awesome Blog";

    /// <summary>
    /// 사이트 설명 (메타 태그용)
    /// </summary>
    public string Description { get; set; } = "A blog about something cool.";

    /// <summary>
    /// 사이트 기본 URL (예: https://example.com)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 기본 작성자 이름
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 사이트 언어 코드 (예: ko, en)
    /// </summary>
    public string Language { get; set; } = "ko";

    /// <summary>
    /// Open Graph 이미지 경로 (소셜 미디어 공유용)
    /// </summary>
    public string? OgImage { get; set; }

    /// <summary>
    /// Open Graph 이미지 대체 텍스트
    /// </summary>
    public string OgImageAlt { get; set; } = "Site cover image";

    /// <summary>
    /// 트위터 사이트 계정 (@username)
    /// </summary>
    public string? TwitterSite { get; set; }

    /// <summary>
    /// 트위터 작성자 계정 (@username)
    /// </summary>
    public string? TwitterCreator { get; set; }

    /// <summary>
    /// GitHub 저장소 또는 프로필 URL
    /// </summary>
    public string? GithubUrl { get; set; }

    /// <summary>
    /// Google Analytics 추적 ID (G-XXXXXXXXXX)
    /// </summary>
    public string? GoogleAnalyticsId { get; set; }

    /// <summary>
    /// 후행 슬래시가 보장된 정규화된 기본 URL을 반환합니다.
    /// </summary>
    [JsonIgnore]
    public string NormalizedBaseUrl => string.IsNullOrWhiteSpace(BaseUrl)
        ? string.Empty
        : (BaseUrl.EndsWith("/") ? BaseUrl : BaseUrl + "/");

    /// <summary>
    /// BaseUrl을 Uri 객체로 반환합니다. 파싱 실패 시 null을 반환합니다.
    /// </summary>
    [JsonIgnore]
    public Uri? BaseUri => Uri.TryCreate(NormalizedBaseUrl, UriKind.Absolute, out var uri) ? uri : null;

    /// <summary>
    /// 사이트의 기본 경로(Path)를 반환합니다. (예: /blog/ 또는 /)
    /// </summary>
    [JsonIgnore]
    public string BasePath
    {
        get
        {
            if (BaseUri == null)
            {
                return "/";
            }

            var path = BaseUri.AbsolutePath;
            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }

            return path.EndsWith("/") ? path : path + "/";
        }
    }
}
