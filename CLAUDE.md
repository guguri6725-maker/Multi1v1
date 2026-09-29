# 프로젝트 개요 (Iron Duel)

Unity로 만든 1대1 파이터 대전 프로토타입입니다. Netcode for GameObjects 기반 온라인 대전과, 해당 밸런스를 검증하는 자체 테스트 스크립트들로 구성되어 있습니다. 게임 자체에 대한 상세 설명(조작법, 전투 규칙, 검증 이력)은 [README-FighterArena.md](README-FighterArena.md)에 있습니다. 이 문서는 코드베이스 구조를 빠르게 파악하기 위한 요약입니다.

## 기술 스택

- Unity (URP 템플릿 기반)
- Unity Netcode for GameObjects 2.13.3 + Unity Transport
- Unity Relay(DTLS) — 방코드 기반 매치메이킹, IP/포트 포워딩 불필요
- TextMesh Pro (HUD/UI)

## 폴더 구조

- `Assets/Scenes/` — `FighterArena.unity`(메인 씬), 기존 `SampleScene` 유지
- `Assets/Scripts/` — 전투/네트워킹/UI/검증 로직 전체 (`Assets/Scripts/ArenaFighter.cs` 등)
- `Assets/Materials`, `Assets/Fonts`, `Assets/TextMesh Pro`, `Assets/Settings` — 아트/설정 리소스
- `Builds/Windows/` — 배포용 Windows 빌드(`IronDuel.exe`, 폴더째 복사해야 실행됨)
- `TestResults/` — 각 검증 스크립트가 남기는 JSON/로그 결과
- `README-FighterArena.md` — 조작법, 전투 규칙, 각 변경 사항의 검증 기록(한국어)

## 핵심 스크립트 ([Assets/Scripts](Assets/Scripts))

씬의 오브젝트/UI/버튼 참조는 런타임 생성 없이 전부 Inspector에서 연결하는 방식입니다.

| 파일 | 역할 |
| --- | --- |
| [ArenaFighter.cs](Assets/Scripts/ArenaFighter.cs) | 캐릭터 이동/공격/방어/1인칭 렌더링 등 핵심 전투 컴포넌트 |
| [ArenaFighter.Prediction.cs](Assets/Scripts/ArenaFighter.Prediction.cs) | 손님(클라이언트) 이동 예측 및 서버 재조정(reconciliation) |
| [ArenaCombat.cs](Assets/Scripts/ArenaCombat.cs) | 서버와 테스트가 공유하는 전투 수치/판정 규칙 |
| [ArenaSession.cs](Assets/Scripts/ArenaSession.cs) | 네트워크/카메라/HUD 연결, Relay 방 생성·참가·재접속 흐름 |
| [ArenaMatch.cs](Assets/Scripts/ArenaMatch.cs) | 두 캐릭터를 접속자에게 배정하고 라운드 진행을 관리 |
| [ArenaMenu.cs](Assets/Scripts/ArenaMenu.cs) | 메뉴 UI 버튼(방 생성/참가/연습/재경기 등) 연결 |
| [ArenaThrowables.cs](Assets/Scripts/ArenaThrowables.cs) | 투척 무기(번개, 벽 수류탄), 투사체, 폭발 처리 |
| [ArenaStoneWall.cs](Assets/Scripts/ArenaStoneWall.cs) | 벽 수류탄이 만드는 돌기둥 4세트의 생성/파괴/소멸 |
| [ArenaDiagnostics.cs](Assets/Scripts/ArenaDiagnostics.cs) 외 `Arena*Checks.cs` 다수 | Play 모드에서 실행하는 자체 회귀 검증 스위트 (전투, 밸런스, 시야, 예측, 벽 충돌 등 항목별 분리) |

## 실행 방법

1. `Assets/Scenes/FighterArena.unity`를 열고 Play
2. 방장은 **CREATE ROOM** 후 코드 복사, 손님은 **ROOM CODE** 입력 후 **JOIN ROOM**
3. 오프라인 연습은 **PRACTICE** 버튼 사용

자세한 조작키와 밸런스 수치는 [README-FighterArena.md](README-FighterArena.md)를 참고하세요.

## 한계 / 주의사항

- 원작(Dungeonborne) 자산을 가져오지 않고 Unity 기본 도형으로 제작한 프로토타입이며, 수치는 원작 정확 복원이 아닙니다.
- 추적/회피형 AI 없음(연습 상대는 판정 확인용 고정 패턴).
- 명중 판정은 서버 권위이며, 되감기식 지연 보상(lag compensation)은 아직 없습니다.
