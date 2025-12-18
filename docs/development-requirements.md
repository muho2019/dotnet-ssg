# dotnet-ssg 상세 개발 요구사항

이 문서는 `GEMINI.md`에 명시된 `dotnet-ssg` 프로젝트의 각 구현 단계를 위한 구체적이고 상세한 기술 요구사항을 정의합니다. 개발자는 이 문서를 기준으로 기능을 구현해야 합니다.

## 공통 요구사항

- **코딩 컨벤션**: .NET 최신 코딩 스타일 가이드를 준수합니다. (예: `async/await` 비동기 처리, 파일 범위 네임스페이스, `var` 키워드 사용 등)
- **오류 처리**: 모든 파일 I/O 및 네트워크 작업에는 `try-catch` 블록을 사용하여 예외를 처리해야 합니다. 사용자에게 친화적인 오류 메시지를 `stderr`로 출력하고, 가능한 경우 부분적인 성공을 허용해야 합니다. (예: 특정 포스트 파싱 실패 시, 해당 포스트만 건너뛰고 빌드 계속 진행)
- **로깅**: Serilog 또는 .NET 내장 로깅 라이브러리를 사용하여 상세한 로그를 남깁니다. 로그 레벨(Debug, Info, Warning, Error)을 구분하여 빌드 과정의 추적성을 높입니다.
- **단위 테스트**: xUnit 또는 MSTest를 사용하여 각 서비스 및 핵심 로직에 대한 단위 테스트를 작성합니다. 100% 코드 커버리지를 목표로 하되, 최소 85% 이상을 달성해야 합니다.

---

## Phase 1: 프로젝트 설정 및 핵심 모델

### 1.1. .NET 10 콘솔 프로젝트 생성
- **요구사항**: `dotnet new console` 템플릿을 사용하여 `DotnetSsg` 이름의 프로젝트를 생성합니다.
- **검증**: `.csproj` 파일의 `TargetFramework`이 `net10.0`(또는 이에 상응하는 .NET 10 버전)으로 설정되었는지 확인합니다.

### 1.2. NuGet 패키지 설치
- **요구사항**: `docs/nuget-packages.md`에 명시된 모든 패키지(`Markdig`, `YamlDotNet`, `Scriban`)의 최신 안정 버전을 설치합니다.
- **검증**: `.csproj` 파일에 해당 `PackageReference`가 올바르게 추가되었는지 확인합니다.

### 1.3. 데이터 모델 정의
- **요구사항**: `docs/data-models.md`에 정의된 `SiteConfig.cs`, `ContentItem.cs`, `Post.cs`, `Page.cs`를 `src/DotnetSsg/Models/` 디렉토리에 생성합니다.
  - `SiteConfig.cs`: 모든 속성에 대해 기본값을 설정하여 `config.json` 파일에 특정 값이 누락되더라도 프로그램이 안정적으로 동작하도록 보장해야 합니다.
  - `ContentItem.cs`: `abstract` 클래스로 선언하고, 모든 파생 클래스가 공통적으로 사용할 속성(제목, 설명, 원본/결과 경로, URL, HTML 콘텐츠)을 포함해야 합니다.
  - `Post.cs`: `Date` 속성은 `DateTime` 타입이어야 하며, Front Matter 파싱 시 `yyyy-MM-dd` 형식의 문자열을 올바르게 변환해야 합니다. 날짜 파싱 실패 시, 빌드 날짜를 기본값으로 사용하고 경고 로그를 남겨야 합니다. `Tags`는 대소문자를 구분하지 않고 저장되어야 합니다 (예: "C#", "c#"을 동일 태그로 취급).
  - `Page.cs`: `Post.cs`와 구별되는 독립적인 클래스로 유지하여 향후 페이지별 속성(예: `Order` for navigation menus) 추가에 대비해야 합니다.
- **검증**: 각 모델 파일의 내용이 명세서와 일치하는지, 네임스페이스 및 접근 제어자가 올바르게 설정되었는지 확인합니다.

---

## Phase 2: 설정 및 파일 처리

### 2.1. `config.json` 로드
- **요구사항**: `System.Text.Json`을 사용하여 프로젝트 루트의 `config.json` 파일을 비동기적으로 읽고, `SiteConfig` 객체로 역직렬화하는 `ConfigLoader` 서비스를 구현합니다.
- **엣지 케이스**:
  - `config.json` 파일이 존재하지 않을 경우: `new SiteConfig()` 기본값으로 계속 진행하고, 경고 메시지를 출력합니다.
  - JSON 형식이 잘못된 경우: 빌드를 중단하고, 구문 오류 위치를 포함한 명확한 오류 메시지를 출력합니다.
  - 특정 키가 누락된 경우: 모델에 정의된 기본값으로 대체되어야 합니다.

### 2.2. 콘텐츠 파일 스캔
- **요구사항**: `content` 디렉토리와 그 하위 디렉토리를 재귀적으로 탐색하여 모든 `.md` 파일의 전체 경로 목록을 반환하는 `FileScanner` 서비스를 구현합니다.
- **엣지 케이스**:
  - `content` 디렉토리가 존재하지 않을 경우: 빈 목록을 반환하고 경고 메시지를 출력합니다.
  - 읽기 권한이 없는 파일/디렉토리가 있을 경우: 해당 항목을 건너뛰고 경고 로그를 남깁니다.

### 2.3. 정적 파일 복사
- **요구사항**: `content/static/` 디렉토리의 모든 파일과 디렉토리 구조를 `output/static/`으로 그대로 복사하는 `StaticFileCopier` 서비스를 구현합니다.
- **최적화**: 이미 `output` 디렉토리에 파일이 존재하고, `content`의 원본 파일보다 최신일 경우 복사를 건너뛰는 로직을 추가하여 빌드 시간을 단축합니다.
- **엣지 케이스**:
  - `content/static/` 디렉토리가 존재하지 않을 경우: 아무 작업도 수행하지 않고 정상 종료합니다.
  - `output/static/` 디렉토리가 존재하지 않을 경우: 자동으로 생성해야 합니다.

---

## Phase 3: 콘텐츠 파싱 및 처리

### 3.1. 마크다운 파일 읽기 및 Front Matter 파싱
- **요구사항**: `MarkdownParser` 서비스를 구현합니다. 이 서비스는 `.md` 파일 경로를 입력받아 다음을 수행합니다.
  1. `YamlDotNet`을 사용하여 파일 상단의 YAML Front Matter(---로 둘러싸인 부분)를 파싱합니다.
  2. Front Matter 데이터를 기반으로 `Post` 또는 `Page` 객체의 메타데이터 속성(`Title`, `Date`, `Tags` 등)을 채웁니다. 파일 경로가 `content/posts/`로 시작하면 `Post` 객체를, `content/pages/`로 시작하면 `Page` 객체를 생성합니다.
  3. `Markdig`를 사용하여 Front Matter를 제외한 나머지 마크다운 본문을 HTML로 변환하고, `HtmlContent` 속성에 저장합니다.
- **파이프라인 설정**: `Markdig` 파이프라인을 설정하여 GFM(GitHub-Flavored Markdown) 사양을 지원해야 합니다. (예: 테이블, 취소선, 자동 링크 등) 코드 블록에 대한 구문 강조(syntax highlighting) CSS 클래스가 생성되도록 설정해야 합니다.
- **엣지 케이스**:
  - Front Matter가 없는 경우: `Title`은 파일명을 기반으로 생성하고(예: `my-first-post.md` -> `My First Post`), `Date`는 파일 생성일로 설정합니다.
  - Front Matter 형식이 잘못된 경우: 해당 파일 처리를 건너뛰고, 파일 경로와 함께 명확한 오류 로그를 남깁니다.
  - 필수 메타데이터(예: `Title`)가 누락된 경우: 위와 동일하게 파일명을 기반으로 자동 생성하고 경고 로그를 남깁니다.

---

## Phase 4: 템플릿 및 HTML 생성

### 4.1. Scriban 템플릿 렌더링 서비스
- **요구사항**: `TemplateRenderer` 서비스를 구현합니다. 이 서비스는 템플릿 경로와 C# 객체(데이터 모델)를 입력받아 렌더링된 HTML 문자열을 반환합니다.
- **템플릿 로딩**: `templates` 디렉토리의 `.liquid` 파일을 읽고 캐싱하여, 동일한 템플릿을 반복적으로 읽지 않도록 최적화해야 합니다.

### 4.2. HTML 페이지 생성 및 저장
- **요구사항**: 파싱된 각 `ContentItem`(`Post` 또는 `Page`) 객체에 대해 다음을 수행하는 `HtmlGenerator` 서비스를 구현합니다.
  1. 콘텐츠 유형에 맞는 템플릿을 결정합니다. `Post` 객체는 `templates/post.liquid`를, `Page` 객체는 `templates/page.liquid`를 사용합니다.
  2. `layout.liquid`를 기본 레이아웃으로 사용하며, `{{ content }}` 부분에 `post.liquid` 또는 `page.liquid`의 렌더링 결과가 삽입되어야 합니다.
  3. `SiteConfig`와 개별 `ContentItem` 데이터를 모두 템플릿에 전달하여 최종 HTML을 생성합니다. (예: `site.title`, `post.title`, `post.html_content` 등으로 접근 가능)
  4. 생성된 HTML을 올바른 출력 경로에 저장합니다. 출력 경로는 원본 파일 경로를 기반으로 생성됩니다.
     - `content/posts/my-first-post.md` -> `output/posts/my-first-post/index.html`
     - `content/about.md` -> `output/about/index.html`
- **URL 생성**: `ContentItem.Url` 속성은 `BaseUrl`과 출력 경로를 조합하여 절대 URL을 생성해야 합니다. (예: `https://example.com/posts/my-first-post/`)

---

## Phase 5: 고급 기능 및 최적화

### 5.1. 병렬 처리
- **요구사항**: `Parallel.ForEachAsync` 또는 `Task.WhenAll`을 사용하여 다수의 `.md` 파일에 대한 파싱 및 HTML 생성 작업을 병렬로 처리합니다.
- **제어**: `MaxDegreeOfParallelism` 옵션을 설정하여 시스템 리소스 사용량을 제어하고, CPU 코어 수에 맞춰 최적의 병렬 처리 수준을 동적으로 결정하는 로직을 구현해야 합니다.

### 5.2. SEO 메타 태그 생성
- **요구사항**: `layout.liquid` 템플릿 내에서 Front Matter 데이터를 사용하여 동적으로 `<head>` 섹션의 메타 태그를 생성해야 합니다.
- **필수 태그**:
  - `<title>{{ post.title }} | {{ site.title }}</title>`
  - `<meta name="description" content="{{ post.description | default: site.description }}">`
  - Open Graph 태그: `og:title`, `og:description`, `og:url`, `og:type`
  - Canonical URL: `<link rel="canonical" href="{{ post.url }}">`

### 5.3. 인덱스 페이지 및 아카이브 생성
- **인덱스 페이지**:
  - **요구사항**: 모든 `Post` 객체를 날짜 내림차순으로 정렬하여 `output/index.html`을 생성합니다.
  - **템플릿**: `templates/index.liquid`를 사용하며, 이 템플릿은 `Post` 객체 목록을 순회하며 각 포스트의 제목, 날짜, 요약, 링크를 표시해야 합니다.
- **태그별 아카이브**:
  - **요구사항**: 모든 포스트를 분석하여 사용된 모든 태그 목록을 수집합니다. 각 태그에 대해, 해당 태그를 포함하는 포스트 목록 페이지를 생성합니다.
  - **경로**: `output/tags/{tag-name}/index.html` (예: `output/tags/csharp/index.html`)
  - **템플릿**: `templates/tag_archive.liquid`를 사용하며, 이 템플릿은 현재 태그 이름과 해당 태그가 달린 포스트 목록을 표시해야 합니다.

---

## Phase 6: 마무리 및 배포

### 6.1. 최종 실행 로직 통합
- **요구사항**: `Program.cs`의 `Main` 메서드에서 위에서 정의된 모든 서비스(Loader, Scanner, Parser, Generator 등)를 순서대로 호출하여 전체 빌드 프로세스를 실행합니다.
- **CLI 인수**: (선택적) `--source`, `--output`, `--config` 와 같은 CLI 인수를 받아 기본 디렉토리 및 설정 파일 경로를 재정의할 수 있는 기능을 추가합니다. `System.CommandLine` 패키지 사용을 권장합니다.

### 6.2. GitHub Actions 워크플로우
- **요구사항**: `.github/workflows/build.yml` 파일을 작성하여, `main` 브랜치에 푸시될 때마다 자동으로 프로젝트를 빌드하고 테스트하는 워크플로우를 설정합니다.
- **단계**:
  1. .NET SDK 설치
  2. NuGet 패키지 복원 (`dotnet restore`)
  3. 프로젝트 빌드 (`dotnet build`)
  4. 단위 테스트 실행 (`dotnet test`)
