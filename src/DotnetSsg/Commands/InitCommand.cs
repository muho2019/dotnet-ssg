using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DotnetSsg.Commands;

public static class InitCommand
{
    public static Command Create()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "프로젝트 이름",
            DefaultValueFactory = _ => "my-blog"
        };

        var command = new Command("init", "새 dotnet-ssg 프로젝트를 초기화합니다")
        {
            nameArgument
        };

        command.Action = new SynchronousInitAction(nameArgument);

        return command;
    }

    private class SynchronousInitAction : SynchronousCommandLineAction
    {
        private readonly Argument<string> _nameArgument;

        public SynchronousInitAction(Argument<string> nameArgument)
        {
            _nameArgument = nameArgument;
        }

        public override int Invoke(ParseResult parseResult)
        {
            var name = parseResult.GetValue(_nameArgument)!;

            // 프로젝트 이름 검증 (경로 조작 방지)
            if (!IsValidProjectName(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ 잘못된 프로젝트 이름입니다.");
                Console.WriteLine("   프로젝트 이름은 영문, 숫자, 하이픈, 언더스코어만 사용할 수 있습니다.");
                Console.ResetColor();
                return 1;
            }

            var workingDirectory = Directory.GetCurrentDirectory();
            var projectPath = Path.Combine(workingDirectory, name);

            if (Directory.Exists(projectPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 디렉토리가 이미 존재합니다: {projectPath}");
                Console.ResetColor();
                return 1;
            }

            try
            {
                Console.WriteLine($"🚀 새 프로젝트를 생성합니다: {name}");

                // 프로젝트 디렉토리 구조 생성
                Directory.CreateDirectory(projectPath);
                Directory.CreateDirectory(Path.Combine(projectPath, "content"));
                Directory.CreateDirectory(Path.Combine(projectPath, "content", "posts"));
                Directory.CreateDirectory(Path.Combine(projectPath, "content", "static"));
                Directory.CreateDirectory(Path.Combine(projectPath, "content", "static", "css"));
                Directory.CreateDirectory(Path.Combine(projectPath, "content", "static", "images"));

                // config.json 생성
                CreateConfigFile(projectPath, name);

                // 샘플 포스트 생성
                CreateSamplePost(projectPath);

                // About 페이지 생성
                CreateAboutPage(projectPath);

                // 404 페이지 생성
                Create404Page(projectPath);

                // README.md 생성
                CreateReadme(projectPath, name);

                // .gitignore 생성
                CreateGitignore(projectPath);

                Console.WriteLine($"✅ 프로젝트가 성공적으로 생성되었습니다: {projectPath}");
                Console.WriteLine();
                Console.WriteLine("다음 명령어로 시작하세요:");
                Console.WriteLine($"  cd {name}");
                Console.WriteLine("  dotnet-ssg build");
                return 0;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 파일 또는 디렉터리에 접근할 수 없습니다: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 파일 시스템 오류로 인해 프로젝트 생성에 실패했습니다: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 알 수 없는 오류로 인해 프로젝트 생성에 실패했습니다: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private static bool IsValidProjectName(string name)
        {
            // 경로 조작 방지: 알파벳, 숫자, 하이픈, 언더스코어만 허용
            return !string.IsNullOrWhiteSpace(name) &&
                   Regex.IsMatch(name, @"^[a-zA-Z0-9_-]+$") &&
                   !name.Contains("..") &&
                   !name.Contains("/") &&
                   !name.Contains("\\");
        }
    }

    private static void CreateConfigFile(string projectPath, string projectName)
    {
        // JSON 직렬화를 사용하여 안전하게 생성
        var configObject = new
        {
            title = projectName,
            description = $"{projectName}에 오신 것을 환영합니다",
            url = "https://example.com",
            author = "Your Name",
            language = "ko",
            postsPerPage = 10
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(configObject, jsonOptions);
        File.WriteAllText(Path.Combine(projectPath, "config.json"), json);
        Console.WriteLine("  ✓ config.json");
    }

    private static void CreateSamplePost(string projectPath)
    {
        var post = new StringBuilder();
        post.AppendLine("---");
        post.AppendLine("title: \"첫 번째 포스트\"");
        post.AppendLine($"date: {DateTime.Now:yyyy-MM-dd}");
        post.AppendLine("draft: false");
        post.AppendLine("tags:");
        post.AppendLine("  - dotnet-ssg");
        post.AppendLine("  - 시작하기");
        post.AppendLine("description: \"dotnet-ssg로 만든 첫 번째 포스트입니다\"");
        post.AppendLine("---");
        post.AppendLine();
        post.AppendLine("# 첫 번째 포스트");
        post.AppendLine();
        post.AppendLine("**dotnet-ssg**에 오신 것을 환영합니다!");
        post.AppendLine();
        post.AppendLine("이것은 샘플 포스트입니다. 자유롭게 수정하거나 삭제하세요.");
        post.AppendLine();
        post.AppendLine("## 기능");
        post.AppendLine();
        post.AppendLine("- 마크다운 지원");
        post.AppendLine("- Blazor 렌더링");
        post.AppendLine("- RSS 피드");
        post.AppendLine("- 사이트맵");
        post.AppendLine("- 태그 아카이브");

        var postsPath = Path.Combine(projectPath, "content", "posts", "hello-world.md");
        File.WriteAllText(postsPath, post.ToString());
        Console.WriteLine("  ✓ content/posts/hello-world.md");
    }

    private static void CreateAboutPage(string projectPath)
    {
        var about = new StringBuilder();
        about.AppendLine("---");
        about.AppendLine("title: \"About\"");
        about.AppendLine("description: \"사이트 소개\"");
        about.AppendLine("---");
        about.AppendLine();
        about.AppendLine("# About");
        about.AppendLine();
        about.AppendLine("이 사이트는 **dotnet-ssg**로 만들어졌습니다.");

        File.WriteAllText(Path.Combine(projectPath, "content", "about.md"), about.ToString());
        Console.WriteLine("  ✓ content/about.md");
    }

    private static void Create404Page(string projectPath)
    {
        var notFound = new StringBuilder();
        notFound.AppendLine("<!DOCTYPE html>");
        notFound.AppendLine("<html lang=\"ko\">");
        notFound.AppendLine("<head>");
        notFound.AppendLine("    <meta charset=\"UTF-8\">");
        notFound.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        notFound.AppendLine("    <title>404 - Page Not Found</title>");
        notFound.AppendLine("</head>");
        notFound.AppendLine("<body>");
        notFound.AppendLine("    <h1>404 - Page Not Found</h1>");
        notFound.AppendLine("    <p>요청하신 페이지를 찾을 수 없습니다.</p>");
        notFound.AppendLine("    <a href=\"/\">홈으로 돌아가기</a>");
        notFound.AppendLine("</body>");
        notFound.AppendLine("</html>");

        File.WriteAllText(Path.Combine(projectPath, "content", "404.html"), notFound.ToString());
        Console.WriteLine("  ✓ content/404.html");
    }

    private static void CreateReadme(string projectPath, string projectName)
    {
        var readme = new StringBuilder();
        readme.AppendLine($"# {projectName}");
        readme.AppendLine();
        readme.AppendLine("dotnet-ssg로 생성된 정적 사이트입니다.");
        readme.AppendLine();
        readme.AppendLine("## 시작하기");
        readme.AppendLine();
        readme.AppendLine("```bash");
        readme.AppendLine("# 사이트 빌드");
        readme.AppendLine("dotnet-ssg build");
        readme.AppendLine();
        readme.AppendLine("# 새 포스트 생성");
        readme.AppendLine("dotnet-ssg new post \"포스트 제목\"");
        readme.AppendLine();
        readme.AppendLine("# 출력 폴더 정리");
        readme.AppendLine("dotnet-ssg clean");
        readme.AppendLine("```");

        File.WriteAllText(Path.Combine(projectPath, "README.md"), readme.ToString());
        Console.WriteLine("  ✓ README.md");
    }

    private static void CreateGitignore(string projectPath)
    {
        var gitignore = new StringBuilder();
        gitignore.AppendLine("# dotnet-ssg");
        gitignore.AppendLine("output/");
        gitignore.AppendLine();
        gitignore.AppendLine("# OS");
        gitignore.AppendLine(".DS_Store");
        gitignore.AppendLine("Thumbs.db");

        File.WriteAllText(Path.Combine(projectPath, ".gitignore"), gitignore.ToString());
        Console.WriteLine("  ✓ .gitignore");
    }
}
