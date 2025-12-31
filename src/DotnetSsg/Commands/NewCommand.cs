using System.CommandLine;
using System.Text;

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

            if (type.ToLower() == "post")
            {
                CreatePost(contentDir, title, draft, date);
            }
            else if (type.ToLower() == "page")
            {
                CreatePage(contentDir, title);
            }
            else
            {
                Console.WriteLine($"❌ 알 수 없는 타입: {type}. 'post' 또는 'page'를 사용하세요.");
                return 1;
            }

            return 0;
        }

        private static void CreatePost(string contentDir, string title, bool draft, string? dateStr)
        {
            var postsDir = Path.Combine(contentDir, "posts");
            if (!Directory.Exists(postsDir))
            {
                Directory.CreateDirectory(postsDir);
            }

            var slug = ToKebabCase(title);
            var filePath = Path.Combine(postsDir, $"{slug}.md");
            var postDate = string.IsNullOrEmpty(dateStr) ? DateTime.Now.ToString("yyyy-MM-dd") : dateStr;

            var frontMatter = new StringBuilder();
            frontMatter.AppendLine("---");
            frontMatter.AppendLine($"title: \"{title}\"");
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
        }

        private static void CreatePage(string contentDir, string title)
        {
            var slug = ToKebabCase(title);
            var filePath = Path.Combine(contentDir, $"{slug}.md");

            var frontMatter = new StringBuilder();
            frontMatter.AppendLine("---");
            frontMatter.AppendLine($"title: \"{title}\"");
            frontMatter.AppendLine("description: \"페이지 설명을 입력하세요\"");
            frontMatter.AppendLine("---");
            frontMatter.AppendLine();
            frontMatter.AppendLine($"# {title}");
            frontMatter.AppendLine();
            frontMatter.AppendLine("여기에 콘텐츠를 작성하세요.");

            File.WriteAllText(filePath, frontMatter.ToString());
            Console.WriteLine($"✅ 새 페이지가 생성되었습니다: {filePath}");
        }

        private static string ToKebabCase(string text)
        {
            return text
                .ToLower()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Replace(".", "-")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("'", "")
                .Replace("\"", "");
        }
    }
}
