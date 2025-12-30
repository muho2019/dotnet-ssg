<div align="center">

# 🚀 dotnet-ssg

### .NET 개발자를 위한 현대적인 정적 사이트 생성기

**Blazor 컴포넌트 기반 템플릿 • 마크다운 콘텐츠 • 초고속 빌드**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen?style=flat-square)](CONTRIBUTING.md)

[시작하기](#-빠른-시작) • [문서](docs/) • [데모](https://muho2019.github.io/dotnet-ssg/) • [로드맵](docs/roadmap.md)

</div>

---

## ✨ 왜 dotnet-ssg인가?

기존 정적 사이트 생성기들(Hugo, Jekyll, Eleventy)은 훌륭하지만, **.NET 개발자**에게는 익숙하지 않은 언어와 템플릿 문법을 요구합니다. **dotnet-ssg**는 C#과 Blazor를 사용하여 .NET 생태계에서 자연스럽게 블로그를 구축할 수 있도록 설계되었습니다.

### 🎯 핵심 특징

| 특징                   | 설명                                                   |
| ---------------------- | ------------------------------------------------------ |
| **🧩 Blazor 컴포넌트** | Razor 문법으로 재사용 가능한 컴포넌트 기반 템플릿 작성 |
| **📝 마크다운 + YAML** | Front Matter를 지원하는 마크다운으로 콘텐츠 관리       |
| **⚡ 초고속 빌드**     | .NET의 성능을 활용한 밀리초 단위 빌드                  |
| **🎨 Tailwind CSS**    | 현대적이고 반응형인 디자인을 쉽게 적용                 |
| **🔍 SEO 최적화**      | sitemap, robots.txt, RSS, Open Graph 자동 생성         |
| **🏷️ 태그 시스템**     | 자동 태그 아카이브 페이지 생성                         |

---

## 📦 요구 사항

- [.NET 10 SDK](https://dotnet.microsoft.com/download) 이상
- [Node.js](https://nodejs.org/) 18+ (Tailwind CSS 빌드용)

---

## 🚀 빠른 시작

### 1. 저장소 클론

```bash
git clone https://github.com/muho2019/dotnet-ssg.git
cd dotnet-ssg
```

### 2. 의존성 설치

```bash
npm install
```

### 3. 사이트 빌드

```bash
npm run build
```

### 4. 로컬 서버 실행

```bash
npm run serve
```

브라우저에서 `http://localhost:8000`으로 접속하세요!

---

## 📁 프로젝트 구조

```
dotnet-ssg/
├── content/                # 콘텐츠 디렉토리
│   ├── posts/              # 블로그 포스트 (.md)
│   ├── static/             # 정적 파일 (CSS, 이미지 등)
│   ├── about.md            # 일반 페이지
│   └── 404.html            # 404 페이지
├── src/DotnetSsg/          # 소스 코드
│   ├── Components/         # Blazor 컴포넌트
│   │   ├── Layout/         # 레이아웃 (Header, Footer, MainLayout)
│   │   ├── Pages/          # 페이지 템플릿
│   │   └── Shared/         # 공유 컴포넌트
│   ├── Models/             # 데이터 모델
│   └── Services/           # 서비스 (파싱, 렌더링 등)
├── output/                 # 빌드 결과물 (배포용)
├── config.json             # 사이트 설정
└── docs/                   # 문서
```

---

## 📝 콘텐츠 작성

### 블로그 포스트

`content/posts/` 디렉토리에 마크다운 파일을 생성합니다:

```markdown
---
title: '나의 첫 번째 포스트'
date: 2025-12-30
tags: [블로그, dotnet]
author: muho
description: 'dotnet-ssg로 작성한 첫 포스트입니다.'
---

## 안녕하세요!

마크다운으로 쉽게 콘텐츠를 작성할 수 있습니다.
```

### 일반 페이지

`content/` 디렉토리에 마크다운 파일을 생성합니다:

```markdown
---
title: 'About'
description: '저에 대해 소개합니다.'
---

# About Me

이 블로그는 dotnet-ssg로 만들어졌습니다.
```

---

## ⚙️ 설정

`config.json` 파일에서 사이트를 설정합니다:

```json
{
  "title": "My Blog",
  "description": "A blog built with dotnet-ssg",
  "baseUrl": "https://example.com/",
  "author": "Your Name",
  "language": "ko",
  "ogImage": "https://example.com/static/images/og-image.png",
  "twitterSite": "@yourhandle",
  "githubUrl": "https://github.com/yourusername",
  "googleAnalyticsId": "G-XXXXXXXXXX"
}
```

---

## 🛠️ 기술 스택

| 기술                 | 용도                      |
| -------------------- | ------------------------- |
| **.NET 10**          | 런타임 및 빌드            |
| **Blazor**           | 컴포넌트 기반 템플릿 엔진 |
| **Markdig**          | 마크다운 파싱             |
| **YamlDotNet**       | Front Matter 파싱         |
| **Tailwind CSS**     | 스타일링                  |
| **System.Text.Json** | 설정 관리                 |

---

## 🗺️ 로드맵

자세한 개발 계획은 [로드맵 문서](docs/roadmap.md)를 참조하세요.

### 예정된 기능

- 🔧 CLI 도구 (`dotnet tool install -g dotnet-ssg`)
- 🔥 Hot Reload 개발 서버
- 📄 페이지네이션
- 🎨 테마 시스템
- 🔍 클라이언트 사이드 검색
- 🌐 다국어 지원 (i18n)

---

## 🤝 기여하기

기여를 환영합니다! 다음과 같은 방법으로 참여할 수 있습니다:

1. 🐛 [이슈 리포트](https://github.com/muho2019/dotnet-ssg/issues)
2. 💡 기능 제안
3. 🔧 Pull Request 제출

```bash
# 포크 후 클론
git clone https://github.com/your-username/dotnet-ssg.git

# 브랜치 생성
git checkout -b feature/amazing-feature

# 변경사항 커밋
git commit -m "feat: Add amazing feature"

# 푸시 및 PR 생성
git push origin feature/amazing-feature
```

---

## 📄 라이선스

이 프로젝트는 [MIT 라이선스](LICENSE)로 배포됩니다.

---

## 💬 커뮤니티

- 🐙 [GitHub Discussions](https://github.com/muho2019/dotnet-ssg/discussions)
- 🐦 [Twitter](https://twitter.com/muho2019)

---

<div align="center">

**⭐ 이 프로젝트가 마음에 드셨다면 Star를 눌러주세요!**

Made with ❤️ by [muho](https://github.com/muho2019)

</div>
