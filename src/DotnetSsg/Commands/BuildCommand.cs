using System.CommandLine;
using System.CommandLine.Invocation;
using DotnetSsg.Services;

namespace DotnetSsg.Commands;

public static class BuildCommand
{
    public static Command Create(IBuildService buildService, ICssBuilder cssBuilder)
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

        command.Action = new AsynchronousBuildAction(outputOption, draftsOption, buildService, cssBuilder);

        return command;
    }

    private class AsynchronousBuildAction : AsynchronousCommandLineAction
    {
        private readonly Option<string> _outputOption;
        private readonly Option<bool> _draftsOption;
        private readonly IBuildService _buildService;
        private readonly ICssBuilder _cssBuilder;

        public AsynchronousBuildAction(
            Option<string> outputOption,
            Option<bool> draftsOption,
            IBuildService buildService,
            ICssBuilder cssBuilder)
        {
            _outputOption = outputOption;
            _draftsOption = draftsOption;
            _buildService = buildService;
            _cssBuilder = cssBuilder;
        }

        public override async Task<int> InvokeAsync(ParseResult parseResult,
            CancellationToken cancellationToken = default)
        {
            var output = parseResult.GetValue(_outputOption)!;
            var drafts = parseResult.GetValue(_draftsOption);

            var workingDirectory = Directory.GetCurrentDirectory();
            var success = await _buildService.BuildAsync(workingDirectory, output, drafts);

            if (success)
            {
                // Tailwind CSS 빌드
                await _cssBuilder.BuildTailwindCssAsync(workingDirectory);
            }

            return success ? 0 : 1;
        }
    }
}