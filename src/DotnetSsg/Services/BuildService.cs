using System.Diagnostics;
using DotnetSsg.Models;

namespace DotnetSsg.Services;

public class BuildService
{
    public async Task<bool> BuildAsync(string workingDirectory, string outputPath = "output",
        bool includeDrafts = false)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("🚀 dotnet-ssg 빌드를 시작합니다...");

        BlazorRenderer? blazorRenderer = null;

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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("❌ 오류: config.json 파일을 찾을 수 없습니다.");
                Console.Error.WriteLine("   현재 디렉토리가 dotnet-ssg 프로젝트 루트인지 확인하세요.");
                Console.Error.WriteLine("   새 프로젝트를 시작하려면: dotnet-ssg init <프로젝트명>");
                Console.ResetColor();
                return false;
            }

            if (!Directory.Exists(contentDir))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("❌ 오류: content 폴더를 찾을 수 없습니다.");
                Console.Error.WriteLine("   dotnet-ssg 프로젝트 구조가 올바른지 확인하세요.");
                Console.ResetColor();
                return false;
            }

            // 출력 디렉토리 준비
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }

            Directory.CreateDirectory(outputDir);

            // 1. 서비스 초기화
            var configLoader = new ConfigLoader();
            var fileScanner = new FileScanner();
            var staticFileCopier = new StaticFileCopier();
            var markdownParser = new MarkdownParser();

            // Blazor 렌더러 초기화
            blazorRenderer = new BlazorRenderer();
            var htmlGenerator = new HtmlGenerator(blazorRenderer);

            var sitemapGenerator = new SitemapGenerator();
            var robotsTxtGenerator = new RobotsTxtGenerator();
            var rssFeedGenerator = new RssFeedGenerator();

            // 2. 설정 로드
            Console.WriteLine("📄 설정 로딩 중...");
            var siteConfig = await configLoader.LoadConfigAsync(configPath);

            // 3. 정적 파일 복사
            Console.WriteLine("📁 정적 파일 복사 중...");
            staticFileCopier.Copy(staticDir, Path.Combine(outputDir, "static"));

            // Favicon 및 기타 정적 파일 복사
            string[] staticFiles = ["favicon.ico", "404.html"];
            foreach (var staticFile in staticFiles)
            {
                var sourcePath = Path.Combine(contentDir, staticFile);
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, Path.Combine(outputDir, staticFile), true);
                    Console.WriteLine($"Copied: {staticFile}");
                }
            }

            // 4. 콘텐츠 스캔
            Console.WriteLine("🔍 콘텐츠 스캔 중...");
            var files = fileScanner.Scan(contentDir, "md");
            var fileList = files.ToList();
            Console.WriteLine($"📝 파일 {fileList.Count}개를 찾았습니다.");

            // 5. 콘텐츠 파싱 및 HTML 생성 (순차 처리로 변경)
            Console.WriteLine("⚙️ 콘텐츠 파싱 및 생성 중...");
            var contentItems = new List<ContentItem>();

            foreach (var file in fileList)
            {
                try
                {
                    var contentItem = await markdownParser.ParseAsync(file);

                    // draft 옵션 처리
                    if (contentItem is Post post && post.Draft && !includeDrafts)
                    {
                        Console.WriteLine($"⏭️ Draft 건너뜀: {file}");
                        continue;
                    }

                    contentItems.Add(contentItem);

                    await htmlGenerator.GenerateAsync(contentItem, siteConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ '{file}' 처리 중 오류 발생: {ex.Message}");
                }
            }

            // 6. 인덱스 페이지 및 아카이브 생성
            Console.WriteLine("🏠 인덱스 및 아카이브 생성 중...");
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
            Console.WriteLine("🗺️ 사이트맵 생성 중...");
            sitemapGenerator.Generate(siteConfig, contentItems.ToList(), outputDir, posts, tagList.ToList());

            // 8. robots.txt 생성
            Console.WriteLine("🤖 robots.txt 생성 중...");
            robotsTxtGenerator.Generate(siteConfig, outputDir);

            // 9. RSS 피드 생성
            Console.WriteLine("📡 RSS 피드 생성 중...");
            rssFeedGenerator.Generate(siteConfig, posts, outputDir);

            stopwatch.Stop();
            Console.WriteLine($"✅ 빌드가 {stopwatch.ElapsedMilliseconds}ms만에 성공적으로 완료되었습니다.");
            Console.WriteLine($"📊 총 {contentItems.Count}개의 콘텐츠, {posts.Count}개의 포스트, {tagList.Count}개의 태그");

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ 빌드 실패: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return false;
        }
        finally
        {
            // BlazorRenderer 리소스 정리
            if (blazorRenderer != null)
            {
                try
                {
                    await blazorRenderer.DisposeAsync();
                }
                catch (Exception disposeEx)
                {
                    // Dispose 에러는 무시하지만, 디버깅을 위해 로그를 남깁니다.
                    Console.Error.WriteLine(
                        $"⚠️ BlazorRenderer Dispose 중 예외 발생: {disposeEx.GetType().Name}: {disposeEx.Message}");
                    Console.Error.WriteLine(disposeEx.StackTrace);
                }
            }
        }
    }
}
