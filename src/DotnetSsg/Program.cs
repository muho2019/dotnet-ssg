using System.CommandLine;
using System.Text;
using DotnetSsg.Commands;

// 콘솔 UTF-8 인코딩 설정 (이모지 출력을 위해)
Console.OutputEncoding = Encoding.UTF8;

var rootCommand = new RootCommand("dotnet-ssg - .NET 기반 정적 사이트 생성기");

// 명령어 추가
rootCommand.Add(BuildCommand.Create());
rootCommand.Add(ServeCommand.Create());
rootCommand.Add(CleanCommand.Create());
rootCommand.Add(NewCommand.Create());
rootCommand.Add(InitCommand.Create());

// 인자 없이 실행하면 도움말 표시
if (args.Length == 0)
{
    args = ["--help"];
}

var parseResult = rootCommand.Parse(args);

return await parseResult.InvokeAsync();