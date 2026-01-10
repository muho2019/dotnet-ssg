# dotnet-ssg 에이전트 가이드라인

이 문서는 이 리포지토리에서 작업하는 AI 에이전트를 위한 포괄적인 지침을 제공합니다.

**⚠️ 중요: 사용자에게 응답은 항상 한국어로 하세요.**

## 1. 빌드, 린트 및 테스트 (Build, Lint, and Test)

### 빌드 프로세스
- **핵심 애플리케이션**: 이 프로젝트는 .NET 10 콘솔 애플리케이션입니다.
  ```bash
  dotnet build src/DotnetSsg/DotnetSsg.csproj
  ```
- **애플리케이션 실행**:
  ```bash
  # 표준 실행 (기본 로직 또는 도움말 실행)
  dotnet run --project src/DotnetSsg
  
  # 특정 CLI 명령어로 실행 (예: 정적 사이트 빌드)
  dotnet run --project src/DotnetSsg -- build
  ```
- **CSS 빌드**: 스타일링을 위해 Tailwind CSS가 사용되며 별도로 빌드해야 합니다.
  ```bash
  npm run css:build
  ```

### 테스트
- **현재 상태**: 현재 리포지토리에서 감지된 테스트 스위트가 없습니다.
- **향후 구현**: 테스트를 추가할 때 반드시 **xUnit**을 사용하세요.
  - **프로젝트 생성**: `dotnet new xunit -o tests/DotnetSsg.Tests`
  - **참조 추가**: `dotnet add tests/DotnetSsg.Tests reference src/DotnetSsg/DotnetSsg.csproj`
  - **테스트 실행**: `dotnet test`
  - **규칙**: 테스트 클래스 이름은 `Tests`로 끝나야 합니다 (예: `MarkdownParserTests`).

### 린팅 및 포맷팅
- **표준 도구**: 코드 스타일 유지를 위해 내장된 .NET 포맷팅 도구를 사용하세요.
  ```bash
  dotnet format
  ```

## 2. 코드 스타일 및 규칙 (Code Style & Conventions)

### 일반적인 C# 가이드라인
- **프레임워크 버전**: .NET 10 (Preview/Future) 또는 최신 안정 버전.
- **네임스페이스**: 들여쓰기를 줄이기 위해 항상 **파일 범위 네임스페이스(file-scoped namespaces)**를 사용하세요.
  ```csharp
  // ✅ Good
  namespace DotnetSsg.Services;
  
  // ❌ Bad
  namespace DotnetSsg.Services { ... }
  ```
- **Async/Await**: 모든 I/O 바운드 작업(파일 액세스, 네트워크)에는 비동기 작업(`Task`, `Task<T>`)을 선호하세요.
- **의존성 주입**: 표준 생성자 주입을 사용하세요. CLI 명령 팩토리 메서드를 제외하고는 가능한 경우 정적 상태(static state)를 피하세요.

### 네이밍 규칙
- **클래스/메서드**: `PascalCase` (예: `BuildCommand`, `Create`, `ParseAsync`).
- **변수/매개변수**: `camelCase` (예: `outputOption`, `workingDirectory`).
- **비공개 필드**: `_camelCase` (예: `_outputOption`, `_yamlDeserializer`).
- **상수**: `PascalCase` (예: `DefaultOutputPath`).
- **인터페이스**: `IPascalCase` (예: `IContentParser`).

### CLI 상호작용 (`System.CommandLine`)
- **위치**: 모든 명령어는 `src/DotnetSsg/Commands`에 구현하세요.
- **현지화**: 명령어 설명 및 출력 메시지는 **반드시 한국어**여야 합니다.
- **구현 패턴**:
  1.  `Command`를 반환하는 정적 `Create()` 메서드 생성.
  2.  `Create()` 내부에서 옵션/인자 정의.
  3.  `CommandLineAction`을 상속받는 중첩된 비공개 클래스(예: `private class AsynchronousBuildAction`)에 로직 구현.
- **출력 스타일**:
  - **이모지**: 상태를 나타내기 위해 이모지를 사용하세요.
    - 🚀 시작/실행 (예: `🚀 새 프로젝트를 생성합니다`)
    - ✅ 성공 (예: `✅ 정리가 완료되었습니다`)
    - ⚠️ 경고 (예: `⚠️ 파일이 존재하지 않습니다`)
    - ❌ 오류/실패 (예: `❌ 빌드 중 오류 발생`)
    - 🗑️ 정리 (예: `🗑️ 디렉토리를 정리합니다`)
    - 🎨 UI/스타일 작업
  - **색상**: 강조를 위해 `Console.ForegroundColor`를 사용하되(오류는 빨간색, 경고는 노란색), **항상** 그 직후에 `ResetColor()`를 호출하세요.

### 파일 시스템 작업
- **경로**: 크로스 플랫폼 호환성(Windows/Linux/macOS)을 위해 항상 `Path.Combine()`을 사용하세요.
- **정규화**: 경로 구분자를 주의하세요. URL이나 내부 로직을 위해 경로를 정규화할 때 `Replace('\\', '/')`를 사용하세요.
- **안전**:
  - 작업 전에 `Directory.Exists` 또는 `File.Exists`를 확인하세요.
  - 일반적인 `Exception`보다 `UnauthorizedAccessException` 및 `IOException`을 명시적으로 처리하세요.
  - 기본 출력에 원시 스택 추적을 피하고 한국어로 사용자 친화적인 오류 메시지를 제공하세요.

### 데이터 및 직렬화
- **JSON**: `System.Text.Json`을 사용하세요.
- **설정**: 웹 친화적인 콘텐츠(예: HTML 문자가 불필요하게 이스케이프되지 않도록)를 보장하기 위해 `UnsafeRelaxedJsonEscaping`과 함께 `JsonSerializerOptions`를 사용하세요.
- **Markdown**: `UseAdvancedExtensions`, `UseYamlFrontMatter` 및 기타 관련 확장과 함께 `Markdig`를 사용하세요.
- **Front Matter**: YAML Front Matter 파싱에는 `YamlDotNet`을 사용하세요.

## 3. 프로젝트 구조 (Project Structure)

- **`src/DotnetSsg/Commands`**: CLI 명령어 정의 및 실행 로직.
- **`src/DotnetSsg/Components`**: HTML 생성을 위해 사용되는 Blazor/Razor 컴포넌트(`.razor`).
  - `Layout`: 공유 레이아웃 (Header, Footer).
  - `Pages`: 페이지 템플릿 (Post, Index).
- **`src/DotnetSsg/Services`**: 핵심 비즈니스 로직.
  - `MarkdownParser.cs`: MD 파일 및 Front Matter 파싱.
  - `HtmlGenerator.cs`: HTML 생성 프로세스 조정.
  - `BuildService.cs`: 상위 레벨 빌드 조정.
- **`src/DotnetSsg/Models`**: 데이터 전송 객체(DTO) (예: `Post`, `Page`, `ContentItem`).
- **`content/`**: 사용자 콘텐츠를 위한 기본 디렉토리 (마크다운 포스트, 정적 자산).

## 4. Git 커밋 가이드라인 (Git Commit Guidelines)

- **형식**: **Conventional Commits** 구조를 따르세요.
  - `feat`: 새로운 기능
  - `fix`: 버그 수정
  - `docs`: 문서 변경
  - `style`: 포맷팅, 세미콜론 누락 등; 코드 변경 없음
  - `refactor`: 프로덕션 코드 리팩토링
  - `test`: 누락된 테스트 추가, 테스트 리팩토링
  - `chore`: 빌드 작업 업데이트, 패키지 매니저 설정 등
- **언어**: 커밋 메시지(제목 및 본문)는 **한국어**(권장) 또는 영어여야 합니다.
- **예시**:
  ```text
  feat: Hot Reload를 지원하는 serve 명령어 구현 (#12)

  개발 서버와 자동 리로드 기능을 제공하는 serve 명령어를 추가했습니다.
  - FileSystemWatcher 기반 변경 감지
  - WebSocket을 이용한 브라우저 자동 새로고침
  ```

## 5. 워크플로우 및 환경 규칙 (Workflow & Environment Rules)

- **의존성**: 검증 없이 새로운 NuGet 패키지를 추가하지 마세요. 먼저 `DotnetSsg.csproj`를 확인하세요.
- **수정**: CLI 명령어를 수정할 때, 도움말 텍스트(`Description`)가 명확하고 한국어로 현지화되어 있는지 확인하세요.
- **파일**: 커밋된 코드에 `Console.WriteLine` 디버깅 문을 남기지 마세요. 필요한 경우 적절한 로깅을 사용하세요 (현재 CLI 피드백에는 Console 출력이 엄격하게 사용됨).
- **환경**: 이 프로젝트는 Linux (WSL), Windows, macOS에서 실행된다는 점을 인지하세요.
- **Razor 컴포넌트**: `.razor` 파일을 편집할 때 적절한 들여쓰기와 HTML 구조를 확인하세요. 스타일링에는 Tailwind CSS 클래스를 사용하세요.
