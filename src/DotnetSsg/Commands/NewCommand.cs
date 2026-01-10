using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DotnetSsg.Commands;

public static class NewCommand
{
    public static Command Create()
    {
        var typeArgument = new Argument<string>("type")
        {
            Description = "생성할 콘텐츠 타입 (post 또는 page)",
            DefaultValueFactory = _ => "post"
        };

        var titleArgument = new Argument<string>("title")
        {
            Description = "콘텐츠 제목"
        };

        var draftOption = new Option<bool>("--draft", "-d")
        {
            Description = "Draft로 생성",
            DefaultValueFactory = _ => false
        };

        var dateOption = new Option<string?>("--date")
        {
            Description = "게시 날짜 (YYYY-MM-DD 형식, 기본값: 오늘)"
        };

        var command = new Command("new", "새 콘텐츠를 생성합니다")
        {
            typeArgument,
            titleArgument,
            draftOption,
            dateOption
        };

        command.Action = new SynchronousNewAction(typeArgument, titleArgument, draftOption, dateOption);

        return command;
    }

    private class SynchronousNewAction : System.CommandLine.Invocation.SynchronousCommandLineAction
    {
        private readonly Argument<string> _typeArgument;
        private readonly Argument<string> _titleArgument;
        private readonly Option<bool> _draftOption;
        private readonly Option<string?> _dateOption;

        public SynchronousNewAction(Argument<string> typeArgument, Argument<string> titleArgument,
            Option<bool> draftOption, Option<string?> dateOption)
        {
            _typeArgument = typeArgument;
            _titleArgument = titleArgument;
            _draftOption = draftOption;
            _dateOption = dateOption;
        }

        public override int Invoke(ParseResult parseResult)
        {
            var type = parseResult.GetValue(_typeArgument)!;
            var title = parseResult.GetValue(_titleArgument)!;
            var draft = parseResult.GetValue(_draftOption);
            var date = parseResult.GetValue(_dateOption);

            var workingDirectory = Directory.GetCurrentDirectory();
            var contentDir = Path.Combine(workingDirectory, "content");

            // content 디렉토리 존재 확인
            if (!Directory.Exists(contentDir))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ 콘텐츠 디렉터리를 찾을 수 없습니다.");
                Console.WriteLine("   이 디렉터리에서 처음 실행하는 경우 'dotnet-ssg init' 명령을 먼저 실행해 초기화해 주세요.");
                Console.ResetColor();
                return 1;
            }

            try
            {
                if (type.Equals("post", StringComparison.OrdinalIgnoreCase))
                {
                    return CreatePost(contentDir, title, draft, date);
                }
                else if (type.Equals("page", StringComparison.OrdinalIgnoreCase))
                {
                    return CreatePage(contentDir, title);
                }
                else
                {
                    Console.WriteLine($"❌ 알 수 없는 타입: {type}. 'post' 또는 'page'를 사용하세요.");
                    return 1;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 파일 생성 권한이 없습니다: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 파일 생성 중 오류가 발생했습니다: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ 알 수 없는 오류가 발생했습니다: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        private static int CreatePost(string contentDir, string title, bool draft, string? dateStr)
        {
            var postsDir = Path.Combine(contentDir, "posts");
            if (!Directory.Exists(postsDir))
            {
                Directory.CreateDirectory(postsDir);
            }

            // 날짜 검증
            DateTime postDateTime;
            if (!string.IsNullOrEmpty(dateStr))
            {
                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out postDateTime))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ 잘못된 날짜 형식입니다: {dateStr}");
                    Console.WriteLine("   올바른 형식: YYYY-MM-DD (예: 2026-01-02)");
                    Console.ResetColor();
                    return 1;
                }
            }
            else
            {
                postDateTime = DateTime.Now;
            }

            var postDate = postDateTime.ToString("yyyy-MM-dd");

            var slug = ToKebabCase(title);
            var filePath = Path.Combine(postsDir, $"{slug}.md");

            // 파일 존재 확인
            if (File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ 파일이 이미 존재합니다: {filePath}");
                Console.WriteLine("   덮어쓰시겠습니까? (y/N): ");
                Console.ResetColor();
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response != "y" && response != "yes")
                {
                    Console.WriteLine("취소되었습니다.");
                    return 0;
                }
            }

            var escapedTitle = EscapeYamlString(title);
            var frontMatter = new StringBuilder();
            frontMatter.AppendLine("---");
            frontMatter.AppendLine($"title: \"{escapedTitle}\"");
            frontMatter.AppendLine($"date: {postDate}");
            frontMatter.AppendLine($"draft: {draft.ToString().ToLower()}");
            frontMatter.AppendLine("tags:");
            frontMatter.AppendLine("  - tag1");
            frontMatter.AppendLine("description: \"포스트 설명을 입력하세요\"");
            frontMatter.AppendLine("---");
            frontMatter.AppendLine();
            frontMatter.AppendLine($"# {title}");
            frontMatter.AppendLine();
            frontMatter.AppendLine("여기에 콘텐츠를 작성하세요.");

            File.WriteAllText(filePath, frontMatter.ToString());
            Console.WriteLine($"✅ 새 포스트가 생성되었습니다: {filePath}");
            if (draft)
            {
                Console.WriteLine("📝 Draft 모드로 생성되었습니다.");
            }

            return 0;
        }

        private static int CreatePage(string contentDir, string title)
        {
            var slug = ToKebabCase(title);
            var filePath = Path.Combine(contentDir, $"{slug}.md");

            // 파일 존재 확인
            if (File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ 파일이 이미 존재합니다: {filePath}");
                Console.WriteLine("   덮어쓰시겠습니까? (y/N): ");
                Console.ResetColor();
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response != "y" && response != "yes")
                {
                    Console.WriteLine("취소되었습니다.");
                    return 0;
                }
            }

            var escapedTitle = EscapeYamlString(title);
            var frontMatter = new StringBuilder();
            frontMatter.AppendLine("---");
            frontMatter.AppendLine($"title: \"{escapedTitle}\"");
            frontMatter.AppendLine("description: \"페이지 설명을 입력하세요\"");
            frontMatter.AppendLine("---");
            frontMatter.AppendLine();
            frontMatter.AppendLine($"# {title}");
            frontMatter.AppendLine();
            frontMatter.AppendLine("여기에 콘텐츠를 작성하세요.");

            File.WriteAllText(filePath, frontMatter.ToString());
            Console.WriteLine($"✅ 새 페이지가 생성되었습니다: {filePath}");
            return 0;
        }

        private static string ToKebabCase(string text)
        {
            // 1. 먼저 유니코드 문자를 정규화하고 악센트 제거
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // 2. 소문자 변환
            result = result.ToLower();

            // 3. 안전하지 않은 문자들을 하이픈으로 변환하거나 제거
            result = Regex.Replace(result, @"[^a-z0-9\u4e00-\u9fff\uac00-\ud7af\u3040-\u309f\u30a0-\u30ff-]+", "-");

            // 4. 연속된 하이픈을 하나로
            result = Regex.Replace(result, @"-+", "-");

            // 5. 앞뒤 하이픈 제거
            result = result.Trim('-');

            // 6. 빈 문자열이면 기본값 반환
            if (string.IsNullOrEmpty(result))
            {
                result = "untitled";
            }

            return result;
        }

        private static string EscapeYamlString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // YAML 문자열 이스케이프: 백슬래시와 따옴표
            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}
