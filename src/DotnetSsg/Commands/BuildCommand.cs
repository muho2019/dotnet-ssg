using System.CommandLine;
using System.CommandLine.Invocation;
using DotnetSsg.Services;

namespace DotnetSsg.Commands;

public static class BuildCommand
{
    public static Command Create()
    {
        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "출력 디렉토리 경로",
            DefaultValueFactory = _ => "output"
        };

        var draftsOption = new Option<bool>("--drafts", "-d")
        {
            Description = "Draft 포스트도 포함하여 빌드",
            DefaultValueFactory = _ => false
        };

        var command = new Command("build", "정적 사이트를 빌드합니다")
        {
            outputOption,
            draftsOption
        };

        command.Action = new AsynchronousBuildAction(outputOption, draftsOption);

        return command;
    }

    private class AsynchronousBuildAction : AsynchronousCommandLineAction
    {
        private readonly Option<string> _outputOption;
        private readonly Option<bool> _draftsOption;

        public AsynchronousBuildAction(Option<string> outputOption, Option<bool> draftsOption)
        {
            _outputOption = outputOption;
            _draftsOption = draftsOption;
        }

        public override async Task<int> InvokeAsync(ParseResult parseResult,
            CancellationToken cancellationToken = default)
        {
            var output = parseResult.GetValue(_outputOption)!;
            var drafts = parseResult.GetValue(_draftsOption);

            var workingDirectory = Directory.GetCurrentDirectory();
            var buildService = new BuildService();
            var success = await buildService.BuildAsync(workingDirectory, output, drafts);

            if (success)
            {
                // Tailwind CSS 빌드
                await BuildTailwindCssAsync(workingDirectory);
            }

            return success ? 0 : 1;
        }

        private static async Task BuildTailwindCssAsync(string workingDirectory)
        {
            try
            {
                Console.WriteLine("🎨 Tailwind CSS 빌드 중...");

                var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = isWindows ? "powershell.exe" : "npm",
                    Arguments = isWindows ? "-NoProfile -Command \"npm run css:build\"" : "run css:build",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process == null)
                {
                    Console.WriteLine("⚠️ npm을 실행할 수 없습니다. Tailwind CSS 빌드를 건너뜁니다.");
                    return;
                }

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("✅ Tailwind CSS 빌드 완료");
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    Console.WriteLine($"⚠️ Tailwind CSS 빌드 실패: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Tailwind CSS 빌드 중 오류: {ex.Message}");
            }
        }
    }
}