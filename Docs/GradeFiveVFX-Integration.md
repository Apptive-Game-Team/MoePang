# 5등급 광역공격 VFX 추가 내역

## 목적과 적용 대상

초원 Tiger, 바다 Whale, 사막 Lion, 숲 Elephant, 극지 Polarbear의 공격·타격 아트를 기존 전투 코드에 연결했습니다. Friendly 진영이면서 UnitGrade가 5이고 라이브러리에 등록된 기물만 사용합니다.

아트 위치는 요청한 `Assets/MoePang_Art/00.Unit/02.UnitEffect/`입니다. 기존 `00.Sprites/00.Unit` 아래가 아닌, 요청에 적힌 `MoePang_Art/00.Unit` 경로를 사용했습니다.

## 변경 파일

### 기존 파일: Assets/01.Scripts/02.Unit/Unit.cs

1. `suppressCommonHitEffect` 필드 추가: 전용 피격 시트가 재생되는 호출에 한해서 기존 공통 구름 이펙트 중복을 방지합니다. 직렬화 필드가 아니므로 기존 프리팹 수정은 필요 없습니다.
2. `RangeAttack()`에 전용 공격 이펙트 호출 추가: 기존 OverlapBoxAll 결과가 있을 때 현재 공격자의 아트를 전방 박스 중심과 몸통 높이에 표시합니다. 판정 지점은 기존 AttackCoroutine → AttackByType → RangeAttack 흐름 그대로입니다.
3. `RangeAttack()`의 피해 전달 부분: 대상이 Unit이고 전용 이펙트 대상인 공격자라면 `TakeGradeFiveAreaDamage()`를 호출하고, 나머지는 기존 `target.TakeDamage()`를 호출합니다. Castle 같은 비 Unit 대상은 기존 피해 처리·이펙트를 유지합니다.
4. `TakeGradeFiveAreaDamage()` 비공개 메서드 추가: 기존 `isDying/isDamaging` 가드를 동일하게 확인하고 공격자에 맞는 타격 아트를 재생한 뒤, 원래 `TakeDamage(damage)`를 호출합니다. `finally`에서 공통 이펙트 억제 플래그를 이전 값으로 복구합니다.
5. 기존 `TakeDamage()`에서는 `PlayHitEffect()` 호출에 조건 하나만 추가했습니다. 소리, HP 차감, 반피격 전환, 넉백, 사망 처리 등 나머지 코드는 유지했습니다.

**유지한 사항:** 공격 타입 enum, IDamageable 시그니처, 스탯, 공격 주기, 공격 박스 크기, 타겟 탐색, 피해량, 기존 콜라이더 순회 방식, 기존 애니메이션·프리팹. 다중 콜라이더의 중복 타격 여부도 이번 범위에서 변경하지 않았습니다.

### 추가: Assets/01.Scripts/02.Unit/GradeFiveEffectLibrary.cs

- ScriptableObject 설정과 기물별 아트 매핑을 담당합니다.
- `Resources.Load<GradeFiveEffectLibrary>("GradeFiveEffects")`로 설정을 읽습니다. 이 리소스는 빌드에도 포함됩니다.
- 위쪽 행 4칸=공격, 아래쪽 행 4칸=피격으로 런타임 Sprite를 생성하고 재사용합니다.
- 원본 크기가 1774×887로 4열·2행에 정확히 나뉘지 않으므로, 정수 경계로 443/444픽셀 셀을 나눠 끝 픽셀 손실을 방지합니다.
- 텍스처나 설정이 없으면 false를 반환해 기존 피해 처리·공통 이펙트가 계속 작동합니다.

### 추가: Assets/01.Scripts/02.Unit/GradeFiveEffectPlayer.cs

- 짧게 존재하는 독립 GameObject와 SpriteRenderer로 4프레임을 재생합니다.
- 기본 재생 시간은 공격/피격 각각 0.24초이며, 마지막 프레임은 페이드아웃합니다.
- Time.deltaTime을 사용하므로 게임 일시정지에 따라 정지합니다.
- 공격 방향이 음수이면 좌우 반전하고, 기준 SpriteRenderer의 Sorting Layer와 Order+10을 사용합니다.
- 재생 종료 후 오브젝트를 제거합니다. 유닛의 자식이 아니므로 유닛 사망이나 풀 반환 때문에 잘리지 않습니다.
- 이번 버전은 이펙트 오브젝트 풀을 추가하지 않았습니다. 동시 공격이 많은 실제 전투의 프로파일링 결과에 따라 풀링을 별도 도입할 수 있습니다.

### 추가: Assets/Resources/GradeFiveEffects.asset

Inspector에서 기물별 Sheet, Attack Duration, Hit Duration, Attack Width Multiplier, Hit Width Multiplier를 조절할 수 있습니다. 기본 폭 배수는 공격 1.6, 피격 0.9이며, 각각 공격자의 attackRange와 피격자의 unitSize를 기준으로 합니다. 표시 위치 높이는 Unit.cs에서 unitSize×0.45입니다.

### 추가: Assets/01.Scripts/02.Unit/Editor/GradeFiveEffectPreview.cs

`Tools → MoePang → Grade Five VFX Preview`로 아트 재생 창을 엽니다. 5종 공격/타격 시퀀스를 반복 재생하며 Play, Face left, Replay, Speed를 제공합니다. Play를 끄면 정점 프레임을 표시합니다. 씬을 바꾸거나 게임 데이터·저장 파일을 변경하지 않는 아트 확인용 Editor 도구입니다. 전투 판정을 테스트하는 창은 아닙니다.

### 추가 아트

`Assets/MoePang_Art/00.Unit/02.UnitEffect/` 안에 아래 파일과 각 `.meta`를 추가했습니다.

- tiger-attack-hit-sheet.png
- whale-attack-hit-sheet.png
- lion-attack-hit-sheet.png
- elephant-attack-hit-sheet.png
- polarbear-attack-hit-sheet.png

임포트: Default / Texture2D, NPOT None, Mipmap Off, Alpha Is Transparency On, Clamp, Bilinear, Uncompressed, Max Size 2048. 특히 2:1 이미지가 Cubemap으로 자동 감지되지 않도록 Texture2D를 명시했습니다. 원본 PNG 아트는 재가공하지 않았습니다.

## 검증 결과와 남은 확인

- Unity 컴파일 오류 0건 확인.
- 다섯 5등급 데이터와 텍스처 참조 연결 확인.
- 40프레임의 사각형 범위와 전체 시트 픽셀 커버리지 검증 통과.
- 공격/타격 재생 오브젝트 10개 생성과 좌측 공격 5개의 flipX 확인 후 테스트 오브젝트 제거.
- 하위 등급 20종 제외와 null 설정 폴백 검증 통과.
- 실제 스테이지의 공격 밀도·가림·넉백·사망 상황에서 최종 시인성과 위치/크기는 플레이 확인이 필요합니다. 실제 전투를 실행해 피해량까지 검증했다는 의미는 아닙니다.

## 협업 시 Git 주의사항

`Assets/MoePang_Art`는 별도 Git 서브모듈입니다. 메인 저장소의 C# 및 Resources 설정과 아트 저장소의 PNG/.meta를 함께 전달해야 합니다. 아트 서브모듈 커밋 후 메인 저장소에서 해당 서브모듈 참조도 갱신해야 다른 개발자에게 아트가 전달됩니다. 이번 작업에서는 커밋·푸시·스테이징을 하지 않았습니다.

기존 Earth 머티리얼, Packages 변경과 다른 작업자의 아트 변경은 이번 VFX 변경 범위에 포함하지 않았습니다. Unity 실행 중 GvhProjectSettings 같은 도구 설정 파일이 자동 갱신될 수 있으므로 커밋 전 변경 목록을 구분해 검토하세요.

## 되돌리기

Unit.cs에서 위 5개 연결 부분을 되돌리면 기존 전투 이펙트로 복구됩니다. 새 스크립트, Resources 설정, PNG/.meta는 참조가 제거된 뒤 선택적으로 제거할 수 있습니다. Resources 설정만 임시로 비우거나 해당 기물의 Sheet를 None으로 설정하면 그 기물은 기존 공통 효과로 돌아갑니다.

## 후속 수정: BattleCamera에서 이펙트가 보이지 않던 문제

- 원인: BattleCamera의 Culling Mask는 704(Friendly=6, Enemy=7, BattleArea=9)입니다. 새 VFX는 기본 Default=0 레이어였으므로 Scene에는 보여도 전투 RenderTexture에는 렌더되지 않았습니다. Sprite Sorting Layer/Order와 GameObject Layer는 별도 설정입니다.
- 수정: GradeFiveEffectPlayer.Play()에서 기준 SpriteRenderer의 GameObject Layer를 새 VFX GameObject에 복사하도록 추가했습니다. 공격 효과는 공격자 레이어, 타격 효과는 피격자 레이어를 따릅니다.
- 범위: 기존 Sorting Layer/Order 복사와 같은 조건 안에서 처리합니다. 기준 렌더러가 없는 단독 호출은 기존 Default를 유지합니다. 실제 Unit 경로는 해당 유닛의 SpriteRenderer를 전달합니다.
- BattleCamera, Main Camera, RenderTexture, 전투 판정, Unit.cs는 이 수정에서 변경하지 않았습니다.
## 후속 수정: 폴더 정리 후 VFX 미출력

- GradeFiveEffects.asset이 Assets/Resources/ScriptableObjects/00.Unit/02.UnitEffect로 이동하면서 기존 Resources.Load("GradeFiveEffects")가 null을 반환했습니다. 전용 이펙트는 설정 미발견 폴백에 의해 비활성 상태였습니다.
- 현재 위치에 맞춰 GradeFiveEffectLibrary.cs의 Resources.Load 경로를 "ScriptableObjects/00.Unit/02.UnitEffect/GradeFiveEffects"로 수정했습니다. Resources.Load 경로는 Resources 폴더 기준 상대경로이고 확장자는 생략합니다.
- 스크립트의 현재 위치는 Assets/01.Scripts/02.Unit/02.Effect/GradeFiveEffectLibrary.cs입니다. 스크립트 이동 자체는 이번 문제의 원인이 아닙니다.
- 기물 5종의 attackWidthMultiplier=0.8, hitWidthMultiplier=0.9 및 아트 참조는 변경하지 않았습니다.
- 설정 asset을 Resources 하위의 다른 폴더로 다시 이동할 경우 이 로딩 경로도 갱신해야 합니다.

## Follow-up: unit children and per-unit position offsets

- Attack VFX are parented to the attacking Unit transform; hit VFX are parented to the struck Unit transform. They follow movement and are removed if the pooled unit is disabled.
- GradeFiveEffects.asset now exposes Attack Position Offset and Hit Position Offset per entry. These are additional local-space offsets from the previous spawn position. Zero preserves the original placement. Positive X follows the attack direction (mirrored for leftward attacks), positive Y is up, and Z adjusts depth. Parent scale affects these local offsets.
- Hit art and Hit Position Offset still belong to the attacking unit's entry, while placement and parenting use the struck unit. Existing damage logic, timing, width, layer and sorting settings are unchanged.
- Initial world size is preserved when parenting, so the unit's scale is not applied twice.