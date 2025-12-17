# NuGet 패키지 목록

이 문서는 `dotnet-ssg` 프로젝트의 핵심 기능을 구현하는 데 필요한 NuGet 패키지를 정의합니다.

### 1. Markdig

- **패키지 ID**: `Markdig`
- **역할**: 마크다운 파싱
- **설명**: CommonMark 표준을 완벽하게 지원하는 고성능, 고확장성 마크다운 프로세서입니다. 마크다운 콘텐츠를 HTML로 변환하는 핵심적인 역할을 수행합니다. Front Matter, 테이블, 자동 링크 등 다양한 고급 기능을 지원합니다.

### 2. YamlDotNet

- **패키지 ID**: `YamlDotNet`
- **역할**: Front Matter 파싱
- **설명**: 마크다운 파일 상단에 위치한 YAML 형식의 Front Matter(예: `title`, `date`, `tags`)를 파싱하는 데 사용됩니다. 추출된 메타데이터는 C# 객체에 매핑되어 템플릿에서 활용됩니다.

### 3. Scriban

- **패키지 ID**: `Scriban`
- **역할**: Liquid 템플릿 렌더링
- **설명**: 빠르고 강력하며 활발하게 유지보수되는 Liquid 기반 템플릿 엔진입니다. 간단한 문법과 뛰어난 성능으로 다양한 종류의 템플릿을 유연하게 처리할 수 있습니다.

### 4. System.Text.Json

- **패키지 ID**: `System.Text.Json`
- **역할**: 설정 관리
- **설명**: .NET에 내장된 고성능 JSON 라이브러리입니다. `config.json`과 같은 설정 파일을 읽고 C# 객체(`SiteConfig`)로 직렬화/역직렬화하는 데 사용됩니다. 별도의 NuGet 설치가 필요 없을 수 있으나, 프로젝트 SDK 버전에 따라 명시적으로 추가해야 할 수 있습니다.
