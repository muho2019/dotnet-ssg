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
            return success ? 0 : 1;
        }
    }
}