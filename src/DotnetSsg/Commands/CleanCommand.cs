using System.CommandLine;
using System.CommandLine.Invocation;

namespace DotnetSsg.Commands;

public static class CleanCommand
{
    public static Command Create()
    {
        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "정리할 출력 디렉토리 경로",
            DefaultValueFactory = _ => "output"
        };

        var command = new Command("clean", "출력 디렉토리를 정리합니다")
        {
            outputOption
        };

        command.Action = new SynchronousCleanAction(outputOption);

        return command;
    }

    private class SynchronousCleanAction : SynchronousCommandLineAction
    {
        private readonly Option<string> _outputOption;

        public SynchronousCleanAction(Option<string> outputOption)
        {
            _outputOption = outputOption;
        }

        public override int Invoke(ParseResult parseResult)
        {
            var output = parseResult.GetValue(_outputOption)!;
            var workingDirectory = Directory.GetCurrentDirectory();
            var outputPath = Path.Combine(workingDirectory, output);

            if (Directory.Exists(outputPath))
            {
                Console.WriteLine($"🗑️ {outputPath} 디렉토리를 정리합니다...");
                Directory.Delete(outputPath, true);
                Console.WriteLine("✅ 정리가 완료되었습니다.");
            }
            else
            {
                Console.WriteLine($"⚠️ {outputPath} 디렉토리가 존재하지 않습니다.");
            }

            return 0;
        }
    }
}
