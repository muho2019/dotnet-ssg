# 기여 가이드라인

dotnet-ssg에 기여해 주셔서 감사합니다! 🎉

이 문서는 프로젝트에 기여하는 방법을 안내합니다.

## 📋 목차

- [기여 가이드라인](#기여-가이드라인)
  - [📋 목차](#-목차)
  - [🤝 행동 강령](#-행동-강령)
  - [🚀 시작하기](#-시작하기)
  - [🐛 이슈 리포트](#-이슈-리포트)
    - [버그 리포트 체크리스트](#버그-리포트-체크리스트)
    - [좋은 버그 리포트 예시](#좋은-버그-리포트-예시)
  - [💡 기능 제안](#-기능-제안)
  - [🔧 Pull Request](#-pull-request)
    - [PR 프로세스](#pr-프로세스)
    - [PR 체크리스트](#pr-체크리스트)
  - [🛠️ 개발 환경 설정](#️-개발-환경-설정)
    - [요구 사항](#요구-사항)
    - [설치](#설치)
    - [프로젝트 구조](#프로젝트-구조)
  - [📝 코드 스타일](#-코드-스타일)
    - [C# 코드](#c-코드)
    - [Blazor 컴포넌트](#blazor-컴포넌트)
  - [📨 커밋 메시지](#-커밋-메시지)
    - [형식](#형식)
    - [타입](#타입)
    - [예시](#예시)
  - [❓ 질문이 있으신가요?](#-질문이-있으신가요)

---

## 🤝 행동 강령

이 프로젝트는 모든 참여자가 존중받는 환경을 유지하기 위해 노력합니다.

- 서로를 존중하고 배려해 주세요
- 건설적인 피드백을 제공해 주세요
- 다양한 의견과 경험을 환영합니다

---

## 🚀 시작하기

1. 이 저장소를 [Fork](https://github.com/muho2019/dotnet-ssg/fork)합니다
2. 로컬에 클론합니다:
   ```bash
   git clone https://github.com/your-username/dotnet-ssg.git
   cd dotnet-ssg
   ```
3. upstream 리모트를 추가합니다:
   ```bash
   git remote add upstream https://github.com/muho2019/dotnet-ssg.git
   ```

---

## 🐛 이슈 리포트

버그를 발견하셨나요? 다음 정보를 포함하여 [이슈를 생성](https://github.com/muho2019/dotnet-ssg/issues/new)해 주세요:

### 버그 리포트 체크리스트

- [ ] 기존 이슈 중에 유사한 이슈 있는지 검색
- [ ] .NET SDK 버전 (예: .NET 10.0)
- [ ] 운영체제 및 버전
- [ ] 버그 재현 단계
- [ ] 예상 동작
- [ ] 실제 동작
- [ ] 에러 메시지 또는 로그 (있는 경우)

### 좋은 버그 리포트 예시

```
- [x] 기존 이슈 중에 유사한 이슈 있는지 검색해봤습니다.

### 환경
- OS: Windows 11
- .NET SDK: 10.0.100
- Node.js: 20.10.0

### 재현 단계
1. `npm run build` 실행
2. Front Matter에 한글 태그 사용
3. 태그 아카이브 페이지 확인

### 예상 동작
한글 태그가 정상적으로 URL 인코딩되어 표시

### 실제 동작
404 오류 발생

### 에러 로그
[에러 메시지 붙여넣기]
```

---

## 💡 기능 제안

새로운 기능을 제안하고 싶으시다면:

1. 먼저 [기존 이슈](https://github.com/muho2019/dotnet-ssg/issues)를 검색하여 중복을 피해주세요
2. 다음 내용을 포함하여 이슈를 생성해 주세요:
   - 기능 설명
   - 사용 사례 (어떤 문제를 해결하나요?)
   - 가능하다면 구현 아이디어

---

## 🔧 Pull Request

### PR 프로세스

1. **브랜치 생성**

   ```bash
   git checkout -b feature/your-feature-name
   # 또는
   git checkout -b fix/issue-number-description
   ```

2. **변경사항 작성**

   - 코드 변경
   - 테스트 추가 (해당되는 경우)
   - 문서 업데이트 (필요한 경우)

3. **커밋**

   ```bash
   git add .
   git commit -m "feat: Add amazing feature"
   ```

4. **최신 코드 동기화**

   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

5. **푸시 및 PR 생성**
   ```bash
   git push origin feature/your-feature-name
   ```

### PR 체크리스트

- [ ] 코드가 빌드되고 정상 작동합니다
- [ ] 기존 테스트가 통과합니다
- [ ] 새로운 기능에 대한 테스트를 추가했습니다 (해당되는 경우)
- [ ] 문서를 업데이트했습니다 (해당되는 경우)
- [ ] 커밋 메시지가 규칙을 따릅니다

---

## 🛠️ 개발 환경 설정

### 요구 사항

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- 권장 IDE: [Visual Studio](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/) 또는 [Rider](https://www.jetbrains.com/rider/)

### 설치

```bash
# 의존성 설치
npm install

# 빌드
npm run build

# 로컬 서버 실행
npm run serve
```

### 프로젝트 구조

```
src/DotnetSsg/
├── Components/     # Blazor 컴포넌트
├── Models/         # 데이터 모델
├── Services/       # 비즈니스 로직
└── Program.cs      # 진입점
```

---

## 📝 코드 스타일

### C# 코드

- .NET 코딩 규칙을 따릅니다
- 파일 범위 네임스페이스 사용
- `var` 키워드는 타입이 명확할 때 사용
- 비동기 메서드는 `Async` 접미사 사용

```csharp
// ✅ Good
public async Task<string> RenderAsync(Post post)
{
    var html = await _renderer.RenderComponentAsync<PostPage>(post);
    return html;
}

// ❌ Bad
public async Task<string> Render(Post post)
{
    string html = await _renderer.RenderComponentAsync<PostPage>(post);
    return html;
}
```

### Blazor 컴포넌트

- PascalCase 파일명 사용
- 컴포넌트당 하나의 책임

---

## 📨 커밋 메시지

[Conventional Commits](https://www.conventionalcommits.org/) 규칙을 따릅니다.

### 형식

```
<type>: <description>

[optional body]

[optional footer]
```

### 타입

| 타입       | 설명                         |
| ---------- | ---------------------------- |
| `feat`     | 새로운 기능                  |
| `fix`      | 버그 수정                    |
| `docs`     | 문서 변경                    |
| `style`    | 코드 포맷팅 (기능 변경 없음) |
| `refactor` | 리팩토링                     |
| `test`     | 테스트 추가/수정             |
| `chore`    | 빌드, 설정 등 기타 변경      |

### 예시

```bash
feat: Add pagination support for post listing
fix: Resolve Korean tag URL encoding issue
docs: Update README with new configuration options
refactor: Extract markdown parsing to separate service
```

---

## ❓ 질문이 있으신가요?

- [GitHub Discussions](https://github.com/muho2019/dotnet-ssg/discussions)에서 질문해 주세요
- 이슈에 `question` 라벨을 붙여 질문할 수도 있습니다

---

다시 한번 기여에 감사드립니다! 🙏
