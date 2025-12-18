# GEMINI: .NET 10 기반 정적 사이트 생성기 설계 문서

## 0. 프로젝트명

dotnet-ssg

## 1. 개요

이 문서는 .NET 10을 사용하여 정적 사이트 생성기(Static Site Generator, SSG)를 구축하기 위한 기술적인 접근 방법을 정의합니다. 최종 목표는 이 생성기를 사용하여 개인 블로그를 제작하고 운영하는 것입니다.

보다 상세한 기술 요구사항은 [docs/development-requirements.md](docs/development-requirements.md) 문서를 참고하십시오.

## 2. 핵심 목표

- **콘텐츠 중심**: 마크다운(`.md`) 파일로 블로그 포스트와 페이지를 작성합니다.
- **템플릿 기반 렌더링**: Scriban 템플릿을 사용하여 HTML 레이아웃을 정의하고, 콘텐츠를 동적으로 주입합니다.
- **고성능**: 빠르고 효율적인 빌드 프로세스를 지향합니다.
- **단순성**: 복잡한 설정 없이 쉽게 사용할 수 있도록 설계합니다.
- **SEO 최적화**: 검색 엔진 최적화(SEO)를 고려한 메타데이터 및 구조를 생성합니다.
- **확장성**: 향후 플러그인이나 추가 기능을 통합할 수 있는 구조를 고려합니다.

## 3. 기술 스택 및 라이브러리

- **프레임워크**: .NET 10
- **프로젝트 타입**: 콘솔 애플리케이션
- **패키지**: (자세한 내용은 [docs/nuget-packages.md](docs/nuget-packages.md) 참조)
  - Markdig (마크다운 파싱)
  - YamlDotNet (Front Matter 파싱)
  - System.Text.Json (설정 관리)
- **템플릿 엔진**: `Scriban`
  - 이유: 빠르고 강력하며 활발하게 유지보수되는 Liquid 기반 템플릿 엔진입니다. 유연한 문법과 확장성을 제공하여 다양한 레이아웃과 콘텐츠를 효과적으로 렌더링할 수 있습니다.
- **CSS 프레임워크**: `Tailwind CSS`
  - 이유: 유틸리티 우선(utility-first) 방식으로 CSS를 작성하여 개발 속도를 높이고, 유연한 디자인 커스터마이징을 가능하게 합니다. 빌드 시 사용하지 않는 CSS를 제거하여 최종 번들 크기를 최적화할 수 있습니다.

## 4. 프로젝트 구조 제안

```
/dotnet-ssg/
├── src/
│   └── DotnetSsg/
│       ├── DotnetSsg.csproj
│       ├── Program.cs
│       ├── Models/          # Post.cs, SiteConfig.cs 등 데이터 모델 (자세한 내용은 [docs/data-models.md](docs/data-models.md) 참조)
│       ├── Services/        # MarkdownParser.cs, TemplateRenderer.cs 등
│       └── ...
├── content/                 # 블로그 콘텐츠 (사용자 영역)
│   ├── posts/
│   │   └── my-first-post.md
│   ├── pages/
│   │   └── about.md
│   └── static/              # CSS, JS, 이미지 등 정적 파일
│       └── css/
│           └── style.css
├── templates/               # Scriban 템플릿 (사용자 영역)
│   ├── layout.liquid       # 기본 레이아웃
│   ├── post.liquid          # 포스트 렌더링용
│   └── page.liquid          # 일반 페이지 렌더링용
├── output/                  # 생성된 정적 사이트 결과물
└── config.json              # 사이트 전역 설정
```

## 5. 핵심 동작 흐름

1.  **설정 로드**: `config.json` 파일에서 사이트 전역 설정을 로드합니다.
2.  **콘텐츠 스캔**: `content/` 디렉토리를 순회하며 모든 `.md` 파일을 찾습니다.
3.  **정적 파일 복사**: `content/static/` 디렉토리의 모든 파일을 `output/static/`으로 그대로 복사합니다.
4.  **마크다운 파싱 및 렌더링**:
    - 각 `.md` 파일에 대해:
      a. `YamlDotNet`으로 Front Matter(메타데이터)를 추출합니다.
      b. `Markdig`로 마크다운 본문을 HTML로 변환합니다.
      c. 추출된 메타데이터와 변환된 HTML을 `Post` 또는 `Page` C# 객체에 담습니다.
5.  **병렬 처리**: 빌드 시간을 단축하기 위해 다수의 마크다운 파일 파싱 및 렌더링 작업을 병렬로 처리합니다. 이는 시스템의 멀티코어 프로세서의 이점을 활용하여 전체적인 사이트 생성 속도를 향상시킵니다.
6.  **HTML 페이지 생성**:
    - 각 `Post`, `Page` 객체에 대해:
      a. 콘텐츠 유형에 맞는 Scriban 템플릿(`post.liquid`, `page.liquid`)을 결정합니다.
      b. `layout.liquid`을 포함한 템플릿에 객체 데이터를 전달하여 최종 HTML 문자열을 생성합니다.
      c. 생성된 HTML을 `output/` 디렉토리에 파일로 저장합니다. (예: `output/posts/my-first-post/index.html`)
7.  **SEO 메타데이터 주입**: 각 페이지의 Front Matter에서 추출된 SEO 관련 메타데이터(예: `<title>`, `<meta name="description">`, Open Graph 태그 등)를 최종 HTML의 `<head>` 섹션에 동적으로 삽입합니다. 이는 검색 엔진이 페이지의 콘텐츠를 정확하게 이해하고 랭킹하는 데 도움을 줍니다.
8.  **완료**: `output/` 디렉토리에 완전한 정적 사이트가 생성됩니다.

## 6. 구현 로드맵

### Phase 1: 프로젝트 설정 및 핵심 모델

- [x] .NET 10 콘솔 프로젝트(`DotnetSsg`) 생성
- [x] `docs/nuget-packages.md`에 명시된 NuGet 패키지 설치
- [ ] `Models` 디렉토리 생성 및 데이터 모델 정의 (`SiteConfig.cs`, `Post.cs`, `Page.cs` 등)

### Phase 2: 설정 및 파일 처리

- [ ] `config.json` 파일 로드 및 `SiteConfig` 객체로 파싱하는 기능 구현
- [ ] `content` 디렉토리 스캔 및 `.md` 파일 목록 반환 기능 구현
- [ ] `static` 디렉토리의 파일을 `output` 디렉토리로 복사하는 기능 구현

### Phase 3: 콘텐츠 파싱 및 처리

- [ ] 마크다운 파일 읽기 기능 구현
- [ ] `YamlDotNet`을 사용하여 Front Matter 파싱 및 `Post`/`Page` 모델에 데이터 바인딩
- [ ] `Markdig`를 사용하여 마크다운 본문을 HTML로 변환

### Phase 4: 템플릿 및 HTML 생성

- [ ] `Scriban`을 사용하여 Scriban 템플릿(`.liquid`)을 렌더링하는 서비스 구현
- [ ] `layout.liquid`을 포함한 기본 템플릿 구조 생성
- [ ] `Post` 및 `Page` 데이터를 템플릿에 주입하여 최종 HTML 생성
- [ ] 생성된 HTML을 올바른 경로의 `output` 디렉토리에 파일로 저장

### Phase 5: 고급 기능 및 최적화

- [ ] 다수의 콘텐츠 파일을 병렬로 처리하여 빌드 성능 최적화
- [ ] Front Matter의 메타데이터를 사용하여 `<title>`, `<meta>` 등 SEO 관련 태그 생성 기능 구현
- [ ] 인덱스 페이지(포스트 목록) 생성 기능 구현
- [ ] 태그별 아카이브 페이지 생성 기능 구현

### Phase 6: 마무리 및 배포

- [ ] 빌드 프로세스를 실행하는 최종 `Program.cs` 로직 통합
- [ ] 기본적인 오류 처리 및 로깅 추가
- [ ] (선택) GitHub Actions를 통한 자동 빌드 및 배포 워크플로우 설정
