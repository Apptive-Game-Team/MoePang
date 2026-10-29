# 튜토리얼 퍼즐 배치 및 연결

`Assets/Resources/ScriptableObjects/06.Tutorial/TutorialPuzzleLayout.asset`에 7×21칸을 지정했다.
`PuzzleGenerator`, `TutorialManager`, 씬은 수정하지 않았다.

## 배치 규칙

- 화면은 7×7이다. SO의 Rows 14~20이 초기 보드이며, Row 14가 화면 위쪽이다.
- Rows 0~13은 보충용이다. 각 열에서 Row 13 → Row 12 → … → Row 0 순서로 소비한다.
- 아래 좌표는 모두 코드 기준 `(column, row)`이며 **왼쪽 아래가 `(0, 0)`**, 오른쪽 위가 `(6, 6)`이다.
- 이미지의 패턴과 서식지 종류를 참고해 7×7로 다시 구성했다. 사진을 그대로 잘라낸 배치는 아니다.
- 숲=잎, 초원=꽃, 극지방=눈, 바다=물, 사막=돌이다.

초기 보드 (위에서 아래 순서):

```text
숲   사막 초원 바다 극지 숲   초원
초원 극지 극지 숲   초원 극지 사막
바다 초원 극지 숲   극지 극지 숲
숲   극지 숲   사막 숲   사막 초원
초원 극지 초원 극지 극지 숲   사막
숲   초원 바다 초원 사막 바다 숲
바다 바다 극지 바다 바다 사막 극지
```

## 진행 순서와 장애물 좌표

각 행동 후 매치·낙하가 끝나고 `IsProcessing == false`가 된 뒤 다음 단계로 넘어간다.
대사 인덱스는 Tutorial00~Tutorial14 기준이다.

| 대사 | 행동 | 결과 |
|---|---|---|
| Tutorial00 | 숲 `(3,4)` → 사막 `(3,3)`으로 교환 | `(2,3)`~`(4,3)`에 숲 3매치 |
| Tutorial01~04 | 입력을 막고 설명만 진행 | 보드 유지 |
| Tutorial05 | 극지방 `(2,3)` → 초원 `(2,2)`로 교환 | `(1,2)`~`(4,2)`에 극지방 4매치 |
| Tutorial06 | 생성된 바람 `(2,2)` 클릭 | row 2 전체 제거 |
| Tutorial07 | 극지방 `(3,4)` → 바다 `(3,3)`으로 교환 | `(1,3)`~`(5,3)`에 극지방 5매치 |
| Tutorial08 | 생성된 햇빛 `(3,3)` 클릭 | 현재 CircleBomb 코드의 5×5 영역(모서리 제외) 제거 |
| Tutorial09~11 | 입력을 막고 설명만 진행 | 보드 유지 |
| Tutorial12 | 숲 `(4,2)`를 Fixed 자물쇠 타일로 교체한 다음, 숲 `(3,1)` → 사막 `(3,2)`로 교환 | `(2,2)`~`(4,2)` 숲 3매치에 자물쇠 타일 포함 |
| Tutorial13 | 극지방 `(3,3)`을 DeActivated 타일로 교체한 다음, 바다 `(2,1)` → 사막 `(2,2)`로 교환 | `(1,2)`~`(3,2)` 바다 3매치가 비활성화 타일 바로 아래에서 발생 |
| Tutorial14 | 이후 자유 플레이 | 남은 보충 타일 사용 후 랜덤 보충 |

장애물은 **해당 단계에 진입할 때만** 생성해야 한다. 처음부터 넣으면 중간 매치나 폭발에 영향을 준다.
자물쇠는 기존 숲 속성을 유지하고 `isMatchable = true`, `isSwappable = false`여야 한다.
비활성화 타일은 기존 극지방 속성을 유지해서 해제 후 극지방 타일로 복구되게 한다.

이 배치는 각 단계에서 표의 행동만 수행한다는 전제로 검증했다.
다른 교환, 특수 타일 이동, 아이템 사용 등을 허용하면 다음 단계의 좌표와 매치가 달라질 수 있다.
자동 랜덤 장애물은 계속 막고, 지정한 장애물 두 개만 TutorialManager에서 생성하는 흐름을 권장한다.
현재 `TutorialManager`에는 단계 진행·입력 제한·장애물 생성이 아직 구현되어 있지 않다.
Tutorial09의 '2단계 친구' 소환은 별도의 유닛 소환 제어가 필요하며, 이 SO로 등급까지 보장하지는 않는다.

## PuzzleGenerator 연결 코드

아래 코드는 직접 옮겨 넣을 수 있도록 작성한 안내이며, 실제 스크립트에는 적용하지 않았다.

### 1. 필드 추가

`PuzzleGenerator` 클래스 내부에 추가하고, 인스펙터의 Tutorial Puzzle Layout에 수정된 SO를 연결한다.

```csharp
[Header("Tutorial")]
[SerializeField] private TutorialPuzzleLayout tutorialPuzzleLayout;

private readonly int[] _tutorialRefillIndices =
    new int[TutorialPuzzleLayout.Width];
```

현재 파일에는 이미 `using System;`과 `using _01.Scripts._00.Manager;`가 있다.

### 2. GenerateBoard() 맨 위에 추가

기존 `_puzzles = new PuzzleObject[x, y];`보다 위에 넣는다.
보드를 새로 생성하면 보충 진행 위치도 처음으로 되돌린다.

```csharp
if (GameManager.Instance.Istutorial)
{
    if (tutorialPuzzleLayout == null)
    {
        Debug.LogError("PuzzleGenerator: Assign the tutorial puzzle layout.", this);
        yield break;
    }

    x = TutorialPuzzleLayout.Width;
    y = TutorialPuzzleLayout.BoardHeight;
    Array.Clear(_tutorialRefillIndices, 0, _tutorialRefillIndices.Length);
}
```

### 3. SetStartRandomPuzzle() 맨 위에 추가

`bool isObstacle = false;`보다 위에 넣는다. SO에 지정한 서식지로 생성하고 반환하므로,
튜토리얼 초기 배치에서는 랜덤 장애물·골드·교환 보너스가 붙지 않는다.

```csharp
if (GameManager.Instance.Istutorial)
{
    Habitat tutorialType = tutorialPuzzleLayout.GetInitialTile(col, row);
    GameObject tutorialPuzzle = Instantiate(
        normalPuzzlePrefabs[(int)tutorialType], puzzleFrame);

    tutorialPuzzle.transform.localPosition = CalculateDropPos(col, row);
    return tutorialPuzzle;
}
```

일반 모드는 아래에 남아 있는 기존 코드를 그대로 실행한다.

### 4. 보충 서식지를 가져오는 함수 추가

`PuzzleGenerator` 클래스 내부에 추가한다. 열마다 진행 위치를 따로 저장한다.
각 열의 14개 예약 타일을 모두 쓰면 그 열부터 랜덤 보충으로 전환한다.

```csharp
private Habitat GetTutorialRefillType(int column)
{
    int refillIndex = _tutorialRefillIndices[column]++;

    if (refillIndex < TutorialPuzzleLayout.RefillCountPerColumn)
        return tutorialPuzzleLayout.GetRefillTile(column, refillIndex);

    // 예약 타일을 소진한 이후에는 일반 서식지 중 무작위로 선택한다.
    var types = (Habitat[])Enum.GetValues(typeof(Habitat));
    return types[Random.Range(0, types.Length)];
}
```

### 5. SetRandomPuzzle() 맨 위에 추가

기존 `var types = Enum.GetValues(typeof(Habitat));`보다 위에 넣는다.
`spawnOrder`는 낙하 연출 위치 계산용이고, 예약 타일의 순서는 위 함수의 별도 배열로 관리한다.

```csharp
if (GameManager.Instance.Istutorial)
{
    Habitat tutorialType = GetTutorialRefillType(col);
    GameObject tutorialPuzzle = Instantiate(
        normalPuzzlePrefabs[(int)tutorialType], puzzleFrame);

    tutorialPuzzle.transform.localPosition = CalculateDropPos(col, spawnOrder);
    tutorialPuzzle.name = $"Puzzle({col + 1},{row + 1})";
    return tutorialPuzzle;
}
```

이 경로에서는 보충 타일에도 골드·교환 보너스를 붙이지 않는다.
SO를 모두 소비한 뒤에도 튜토리얼 모드인 동안은 동일하게 보너스가 붙지 않는다.

### 6. 기존 장애물 생성 제한 유지

현재 `SpawnObstaclePuzzle()`에는 `Istutorial`일 때 `yield break`하는 처리가 이미 있다.
이 처리를 유지해야 안내 중 임의의 위치에 장애물이 생기지 않는다.
입력 횟수 제한으로 중간에 게임오버가 되지 않도록 튜토리얼의 `maxSwapCount`도 확인한다.

## 검증 범위

- 저장된 SO의 enum 배열을 다시 읽어 초기 49칸과 열별 보충 순서를 재생했다.
- 초기 자동 매치 없음, 지정한 3·4·5매치, 두 특수 타일의 폭발 범위를 확인했다.
- 두 장애물 연습까지 총 7개 행동 이후 각각 의도하지 않은 추가 매치가 없음을 확인했다.
- 자물쇠는 교환 대상이 아니면서 매치에 포함되고, 비활성화 타일은 다음 매치와 상하좌우로 인접한다.
- 최종 열별 보충 사용량은 왼쪽부터 `1, 7, 10, 10, 10, 5, 1`개로 각 열의 14개 이내다.
- Unity 플레이 모드, 애니메이션, UI 클릭 전달, 유닛 소환 등은 실행 검증하지 않았다.
