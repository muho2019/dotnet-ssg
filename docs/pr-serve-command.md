# PR: Hot Reload를 지원하는 serve 명령어 구현

## 📋 개요

개발 서버와 자동 리로드 기능을 제공하는 `serve` 명령어를 추가했습니다.
파일을 수정하면 자동으로 빌드하고 브라우저가 새로고침되어 개발 경험이 크게 향상됩니다.

## ✨ 주요 기능

### 1. 개발 서버 (Kestrel 기반)

- 로컬에서 정적 사이트를 즉시 프리뷰
- 기본 포트: 5000 (커스터마이징 가능)
- 정적 파일 서빙 및 자동 인덱스 처리

### 2. Hot Reload (WebSocket 기반)

- 파일 변경 감지 시 자동으로 브라우저 새로고침
- 실시간 양방향 통신으로 빠른 반응 속도
- 연결 끊김 시 자동 재연결

### 3. 파일 감시 (FileSystemWatcher)

- 감시 대상:
  - `content/**/*.md` - 마크다운 콘텐츠
  - `Components/**/*.razor` - Blazor 컴포넌트
  - `config.json` - 사이트 설정
- 300ms debouncing으로 중복 빌드 방지
- 임시 파일 자동 필터링 (`~RF*.TMP`, `.tmp`, `.swp`, 숨김 파일)

### 4. Tailwind CSS 자동 리빌드

- 파일 변경 시 CSS도 함께 자동 빌드
- 스타일 변경 사항 즉시 반영

### 5. Base Href 자동 조정

- 프로덕션: `/dotnet-ssg/` (GitHub Pages 경로)
- 개발 서버: `/` (로컬 경로)
- HTML 파일 자동 변환으로 로컬에서도 정상 동작

### 6. 안정성 개선

- 파일 충돌 방지 retry 로직 (파일 잠김 해결)
- 안전한 디렉토리 삭제 (최대 3회 재시도)
- 중복 빌드 방지 플래그

## 🎯 CLI 옵션

```bash
dotnet run -- serve [options]
```

| 옵션         | 단축 | 설명                    | 기본값     |
| ------------ | ---- | ----------------------- | ---------- |
| `--port`     | `-p` | 서버 포트 번호          | `5000`     |
| `--output`   | `-o` | 출력 디렉토리 경로      | `"output"` |
| `--drafts`   | `-d` | Draft 포스트 포함 여부  | `true`     |
| `--no-watch` |      | 파일 변경 감시 비활성화 | `false`    |

### 사용 예시

```bash
# 기본 실행 (포트 5000, draft 포함)
dotnet run -- serve

# 다른 포트로 실행
dotnet run -- serve --port 8080

# Draft 제외하고 실행
dotnet run -- serve --drafts false

# 파일 감시 없이 실행 (일회성 서빙)
dotnet run -- serve --no-watch
```

## 🛠️ 구현 내용

### 새로 추가된 파일

1. **Commands/ServeCommand.cs**

   - CLI 명령어 정의 및 옵션 처리
   - 초기 빌드 및 자동 리빌드 오케스트레이션
   - Tailwind CSS 빌드 통합

2. **Services/DevServer.cs**

   - Kestrel 기반 HTTP 서버
   - WebSocket LiveReload 엔드포인트 (`/livereload`)
   - HTML 파일에 LiveReload 스크립트 자동 주입
   - Base href 자동 변환 (`/dotnet-ssg/` → `/`)

3. **Services/FileWatcher.cs**

   - FileSystemWatcher 래퍼
   - Debouncing 로직 (300ms)
   - 임시 파일 필터링
   - 중복 이벤트 방지

4. **Properties/launchSettings.json**
   - Visual Studio 디버깅 프로필
   - 다양한 시나리오별 프로필 제공

### 수정된 파일

1. **Services/BuildService.cs**

   - `DeleteDirectorySafe` 메서드 추가
   - 파일 잠김 시 재시도 로직 (3회, 딜레이 증가)

2. **Services/HtmlGenerator.cs**

   - `WriteFileWithRetryAsync` 메서드 추가
   - 파일 쓰기 충돌 방지 (3회 재시도)

3. **Program.cs**

   - ServeCommand 등록

4. **DotnetSsg.csproj**
   - `Microsoft.AspNetCore.App` 프레임워크 참조 추가

## 🎬 데모

```bash
$ dotnet run -- serve
📦 최신 상태로 빌드 중 (draft 포함)...

✅ 빌드 완료
   - 2개의 페이지 생성
   - 2개의 포스트 생성
   - 4개의 태그 아카이브 생성

🚀 개발 서버 시작: http://localhost:5000
   파일 변경 감시 중... (Ctrl+C로 종료)

[파일 수정 시]
📝 변경 감지: content/posts/my-first-post.md
🔄 자동 리빌드 중...
✅ 리빌드 완료 (0.8초)
🔄 브라우저 새로고침 전송됨
```

## 🧪 테스트 방법

1. 서버 시작:

   ```bash
   dotnet run -- serve
   ```

2. 브라우저에서 `http://localhost:5000` 접속

3. 파일 수정:

   - `content/posts/my-first-post.md` 수정
   - 브라우저가 자동으로 새로고침되는지 확인

4. CSS 변경:

   - `content/static/css/input.css` 수정
   - 스타일이 자동으로 반영되는지 확인

5. 컴포넌트 변경:
   - `Components/Layout/Header.razor` 수정
   - 레이아웃 변경이 반영되는지 확인

## 📚 관련 이슈

Closes #9

## 🔍 체크리스트

- [x] 코드 작성 완료
- [x] 로컬 테스트 완료
- [x] WebSocket 연결 정상 동작
- [x] Hot Reload 정상 동작
- [x] 파일 충돌 해결
- [x] 중복 빌드 방지
- [x] Tailwind CSS 자동 빌드
- [x] Draft 옵션 추가
- [x] Visual Studio 디버깅 프로필 추가

## 💡 향후 개선 사항

- [ ] 브라우저 자동 열기 옵션 (`--open`)
- [ ] HTTPS 지원
- [ ] 특정 파일/폴더 감시 제외 옵션
- [ ] 빌드 에러 시 브라우저 오버레이 표시
- [ ] 변경된 파일만 부분 빌드 (현재는 전체 빌드)
