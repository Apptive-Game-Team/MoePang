# TutorialManager 재사용

현재 전투 튜토리얼 20단계는 `02.3match&Battle.unity`의 TutorialManager > Steps에 저장됩니다.
대사 번호로 이벤트를 판단하지 않으므로 단계를 이동하거나 번역 키를 바꿔도 그 단계의 설정이 함께 이동합니다.
다만 퍼즐 교환 순서는 보드 배치에 의존합니다. 교환 순서를 바꿀 때는 보드와 보충 타일도 맞춰야 합니다.

## 단계 설정

| 설정 | 용도 |
|---|---|
| Dialogue Key | Localization Table에서 가져올 번역 키. 키를 쓰면 TutorialText 목록은 필요 없습니다. |
| Dialogue Index | 키를 비웠을 때만 사용하는 기존 TutorialText 목록의 인덱스. |
| Wait Type | Swap: 교환, Special Tile: 특수 타일, Next Click: 클릭/시간, Delay: 대사 후 추가 대기, Event: 외부 신호, Hold: 안내 종료 후 자유 플레이. |
| Panel | 위/아래 대사 패널 선택. |
| Dialogue Duration | -1이면 공통 시간을 사용. 0 이상이면 이 단계만 별도 시간 적용. 클릭으로 대기를 넘길 수 있습니다. |
| From / To / Special Type | 기다릴 퍼즐 조작. Swap/Special Tile은 대사와 동시에 조작과 힌트를 시작합니다. |
| Enter Action / Obstacle Position / Obstacle Type | 대사와 함께 지정한 장애물 생성. 생성 완료 후 대사 대기를 시작합니다. |
| Dim Background | 배경을 어둡게 하고 대사창과 종료 버튼을 강조. |
| Highlight Objects | 이미지·텍스트 모양을 따라 강조할 게임 오브젝트. 자식도 포함. |
| Highlight Rects | 사각형 영역 전체를 강조할 RectTransform. 여백까지 강조됩니다. |
| Highlight Cells | 강조할 퍼즐 좌표. 비워 둔 동작 대기 단계는 조작할 타일을 기본 강조합니다. |
| Highlight Spawn Stack / Show Camera Button | 연결된 에너지 영역 또는 카메라 버튼 강조. |
| Play Hint Animation / Show Hint Circle | 힌트 애니메이션과 From 위치의 원형 힌트 표시 여부. |
| Hide Panel Before Spawn | 첫 아군 소환 직전에 대사창 숨김. 퍼즐/소환 완료 후 Post Action Delay가 지나면 다음 대사를 표시합니다. |
| Action Friendly Grade / After Action Friendly Grade | 동작 시작/처리 완료 때 적용할 아군 등급. -1: 유지, 0: 일반 소환 규칙. |
| Record Spawned Units | 이 동작으로 소환된 아군을 기록. 이전 기록은 동작 시작 때 지웁니다. |
| Focus Spawned Unit / Focus Unit Grade | 기록된 아군 중 해당 등급의 선두 유닛을 강조하고 카메라 집중. |
| On Enter / On Action Started / On Exit | 단계 진입 / 퍼즐 동작 시작 / 단계 종료 때 실행할 추가 UnityEvent. |

하이라이트가 있는 단계는 대사창·종료 버튼을 항상 포함합니다.
대사 다시 보기는 진입 이벤트를 다시 실행하므로, 일회성 보상·결제 같은 작업을 On Enter에 연결하지 마세요.
Event 대기 중에는 전체 화면의 대사 넘기기 입력을 끄므로 기존 버튼을 누를 수 있습니다.

## 다른 전투 씬에 같은 튜토리얼 배치

1. 현재 TutorialManager의 컴포넌트 설정을 복사하고 대사 UI, 하이라이트 마스크, 퍼즐 생성기, 카메라, 소환 관련 씬 참조를 다시 연결합니다.
2. 종료 팝업을 사용한다면 End Accept Button / End Close Button을 직접 연결합니다. 팝업 없이 종료 버튼만 쓰면 누르는 즉시 StopTutorial을 실행합니다.
3. GameManager의 튜토리얼 모드를 씬 진입 전에 켜고, 기존과 같은 TutorialPuzzleLayout을 연결합니다.
4. Auto Start / Require Tutorial Mode / Use Battle Rules를 켭니다.
5. Apply Tutorial Stage, Tutorial Castle Hp, Initial Friendly Grade, 적 소환 인덱스는 해당 안내에 맞춰 지정합니다.

여러 TutorialManager를 동시에 실행하는 것은 지원하지 않습니다.
전투 스탯은 유닛과 성이 초기화될 때 적용됩니다. 같은 씬에서 진행 중인 일반 전투를 전투 튜토리얼로 바꾸려면 씬과 보드를 함께 초기화해야 합니다.

## 상점·강화 등 일반 UI 안내

1. Canvas 안에 대사창과 TutorialManager를 준비하고 Tutorial Panel / Tutorial Text를 연결합니다.
2. **Use Battle Rules와 Require Tutorial Mode를 끕니다.** 퍼즐·유닛·StageManager 없이 실행할 수 있습니다.
3. Pause Gameplay로 안내 중 게임 시간을 멈출지 선택합니다. 시간을 멈추는 경우 필요한 UI 애니메이션은 unscaled time을 사용해야 합니다.
4. Steps에 번역 키와 Highlight Objects 등을 지정합니다. 일반 안내에서는 Swap/Special Tile/장애물/소환 관련 설정을 사용하지 않습니다.
5. 특정 버튼 입력을 기다릴 단계는 Wait Type을 Event로 두고, 해당 버튼의 On Click에 `CompleteCurrentAction()`을 연결합니다.
6. 코드에서 시작할 경우 Auto Start를 끄고 필요한 시점에 `StartTutorial()`을 호출합니다. 안내가 끝나면 자동 정리하려면 마지막 단계를 Next Click 등으로 두고 Hold는 사용하지 않습니다.

## 시작·중지·재시작

```csharp
tutorialManager.StartTutorial();         // 현재 설정으로 시작. 이미 실행 중이면 무시.
tutorialManager.StopTutorial();          // 구독·힌트·입력 제한·시간·임시 UI/스테이지 설정 해제.
tutorialManager.RestartTutorial();       // 일반 UI는 현재 씬에서 재시작. 전투는 해당 씬 재로드.
tutorialManager.CompleteCurrentAction(); // Event 대기 단계 완료.
```

- `IsRunning`: 튜토리얼 세션 활성 여부. Hold 이후 자유 플레이에서도 전투 규칙을 유지하므로 true입니다.
- `IsInstructionComplete`: 마지막 설명까지 끝났는지 여부. On Completed는 설명 종료 때 실행됩니다.
- `CurrentStepIndex`: 현재 단계 배열 인덱스. 표시 중인 단계가 없으면 -1입니다.
- `On Stopped`: StopTutorial 또는 비활성화로 실행을 중지했을 때 호출합니다. 일반적인 단계 완료에는 호출하지 않습니다.
- 중지 후 컴포넌트를 다시 켜는 것만으로 자동 재시작하지 않습니다. StartTutorial 또는 RestartTutorial을 호출하세요.
- StartTutorial은 기존 보드·성 체력·유닛을 초기화하지 않습니다. 전투를 처음부터 반복하려면 RestartTutorial을 사용합니다.
- 전투 RestartTutorial은 씬 전체를 재로드하므로 그 씬이 빌드의 씬 목록에 포함되어 있어야 합니다.
- StopTutorial은 GameManager의 튜토리얼 모드 값을 바꾸지 않습니다. 일반 전투로 전환할 때의 모드 변경과 씬 이동은 호출하는 쪽에서 처리합니다.
