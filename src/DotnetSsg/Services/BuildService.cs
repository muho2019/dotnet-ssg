using System.Diagnostics;
using DotnetSsg.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetSsg.Services;

public class BuildService : IBuildService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfigLoader _configLoader;
    private readonly IFileScanner _fileScanner;
    private readonly IStaticFileCopier _staticFileCopier;
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ISitemapGenerator _sitemapGenerator;
    private readonly IRobotsTxtGenerator _robotsTxtGenerator;
    private readonly IRssFeedGenerator _rssFeedGenerator;
    private readonly ILogger<BuildService> _logger;

    public BuildService(
        IServiceScopeFactory scopeFactory,
        IConfigLoader configLoader,
        IFileScanner fileScanner,
        IStaticFileCopier staticFileCopier,
        IFileSystemUtils fileSystemUtils,
        ISitemapGenerator sitemapGenerator,
        IRobotsTxtGenerator robotsTxtGenerator,
        IRssFeedGenerator rssFeedGenerator,
        ILogger<BuildService> logger)
    {
        _scopeFactory = scopeFactory;
        _configLoader = configLoader;
        _fileScanner = fileScanner;
        _staticFileCopier = staticFileCopier;
        _fileSystemUtils = fileSystemUtils;
        _sitemapGenerator = sitemapGenerator;
        _robotsTxtGenerator = robotsTxtGenerator;
        _rssFeedGenerator = rssFeedGenerator;
        _logger = logger;
    }

    public async Task<bool> BuildAsync(string workingDirectory, string outputPath = "output",
        bool includeDrafts = false)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("🚀 dotnet-ssg 빌드를 시작합니다...");

        try
        {
            // 0. 경로 설정
            var currentDir = workingDirectory;
            var contentDir = Path.Combine(currentDir, "content");
            var outputDir = Path.Combine(currentDir, outputPath);
            var staticDir = Path.Combine(contentDir, "static");
            var configPath = Path.Combine(currentDir, "config.json");

            // 필수 파일/폴더 검증
            if (!File.Exists(configPath))
            {
                _logger.LogError("❌ 오류: config.json 파일을 찾을 수 없습니다.");
                _logger.LogError("   현재 디렉토리가 dotnet-ssg 프로젝트 루트인지 확인하세요.");
                _logger.LogError("   새 프로젝트를 시작하려면: dotnet-ssg init <프로젝트명>");
                return false;
            }

            if (!Directory.Exists(contentDir))
            {
                _logger.LogError("❌ 오류: content 폴더를 찾을 수 없습니다.");
                _logger.LogError("   dotnet-ssg 프로젝트 구조가 올바른지 확인하세요.");
                return false;
            }

            // 출력 디렉토리 준비
            if (Directory.Exists(outputDir))
            {
                // 서버가 파일을 사용 중일 수 있으므로 안전하게 삭제
                _fileSystemUtils.DeleteDirectorySafe(outputDir);
            }

            Directory.CreateDirectory(outputDir);

            // 1. 서비스 초기화 (Scoped)
            // BlazorRenderer와 HtmlGenerator, MarkdownParser는 상태를 가지거나 리소스를 점유하므로
            // 빌드 단위로 Scope를 생성하여 관리합니다.
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var markdownParser = scope.ServiceProvider.GetRequiredService<IMarkdownParser>();
                var htmlGenerator = scope.ServiceProvider.GetRequiredService<IHtmlGenerator>();
                // BlazorRenderer는 HtmlGenerator 내부에서 사용되지만, 명시적 해제가 필요하다면 여기서 관리 가능
                // IAsyncDisposable이므로 using scope가 끝날 때 자동으로 처리되기를 기대하지만,
                // BlazorRenderer는 IAsyncDisposable을 구현하므로 scope가 끝날 때 DisposeAsync가 호출됨.

                // 2. 설정 로드
                _logger.LogInformation("📄 설정 로딩 중...");
                var siteConfig = await _configLoader.LoadConfigAsync(configPath);

                // 3. 정적 파일 복사
                _logger.LogInformation("📁 정적 파일 복사 중...");
                _staticFileCopier.Copy(staticDir, Path.Combine(outputDir, "static"));

                // Favicon 및 기타 정적 파일 복사
                string[] staticFiles = ["favicon.ico", "404.html"];
                foreach (var staticFile in staticFiles)
                {
                    var sourcePath = Path.Combine(contentDir, staticFile);
                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, Path.Combine(outputDir, staticFile), true);
                        _logger.LogInformation("Copied: {StaticFile}", staticFile);
                    }
                }

                // 4. 콘텐츠 스캔
                _logger.LogInformation("🔍 콘텐츠 스캔 중...");
                var files = _fileScanner.Scan(contentDir, "md");
                var fileList = files.ToList();
                _logger.LogInformation("📝 파일 {Count}개를 찾았습니다.", fileList.Count);

                // 5. 콘텐츠 파싱 및 HTML 생성
                _logger.LogInformation("⚙️ 콘텐츠 파싱 및 생성 중...");
                var contentItems = new List<ContentItem>();

                foreach (var file in fileList)
                {
                    try
                    {
                        var contentItem = await markdownParser.ParseAsync(file, contentDir);

                        // draft 옵션 처리
                        if (contentItem is Post post && post.Draft && !includeDrafts)
                        {
                            _logger.LogInformation("⏭️ Draft 건너뜀: {File}", file);
                            continue;
                        }

                        contentItems.Add(contentItem);

                        await htmlGenerator.GenerateAsync(contentItem, siteConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ '{File}' 처리 중 오류 발생: {Message}", file, ex.Message);
                    }
                }

                // 6. 인덱스 페이지 및 아카이브 생성
                _logger.LogInformation("🏠 인덱스 및 아카이브 생성 중...");
                var posts = contentItems.OfType<Post>().OrderByDescending(p => p.Date).ToList();

                // 인덱스 페이지 (Home)
                await htmlGenerator.GenerateIndexAsync(siteConfig, posts, outputDir);

                // 태그별 아카이브
                var tags = posts.SelectMany(p => p.Tags).Distinct();
                var tagList = tags.ToList();
                foreach (var tag in tagList)
                {
                    var tagPosts = posts.Where(p => p.Tags.Contains(tag)).ToList();
                    await htmlGenerator.GenerateTagArchiveAsync(siteConfig, tag, tagPosts, outputDir);
                }

                // 7. 사이트맵 생성
                _logger.LogInformation("🗺️ 사이트맵 생성 중...");
                _sitemapGenerator.Generate(siteConfig, contentItems.ToList(), outputDir, posts, tagList.ToList());

                // 8. robots.txt 생성
                _logger.LogInformation("🤖 robots.txt 생성 중...");
                _robotsTxtGenerator.Generate(siteConfig, outputDir);

                // 9. RSS 피드 생성
                _logger.LogInformation("📡 RSS 피드 생성 중...");
                _rssFeedGenerator.Generate(siteConfig, posts, outputDir);

                stopwatch.Stop();
                _logger.LogInformation("✅ 빌드가 {ElapsedMilliseconds}ms만에 성공적으로 완료되었습니다.", stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("📊 총 {ContentCount}개의 콘텐츠, {PostCount}개의 포스트, {TagCount}개의 태그",
                    contentItems.Count, posts.Count, tagList.Count);

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 빌드 실패: {Message}", ex.Message);
            return false;
        }
    }

}

