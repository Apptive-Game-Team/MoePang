# 모에팡 4등급 공격·피격 VFX

## 적용 대상과 분석

4등급 데이터는 모두 Friendly / TripleAttack / attackSpeed 0.5 / unitSize 0.8입니다. 한 공격 주기 2초 중 1/3 지점인 약 0.667초에 기존 TripleAttack이 실행됩니다. 실제 Attack 클립은 0.35초이며 Animator 속도 0.5가 적용됩니다. 5종 모두 Idle → 준비 → 타격 자세의 스프라이트를 사용합니다.

| 데이터 | 실제 모션 | 신규 공격 표현 | 신규 피격 표현 |
| --- | --- | --- | --- |
| Giraffe / 03.Giraffe | 목을 앞으로 뻗고 아래로 휘두름 | 황토·아이보리 단일 하강 곡선 | 작고 둔탁한 금빛 충격 쐐기 |
| Hippo / 03.Hippo | 입을 크게 벌린 후 강하게 닫음 | 청회색 상·하 압력 곡선과 작은 물방울 | 두꺼운 압착 충격과 물방울 |
| Hyena / 03.Hyena | 턱을 벌렸다 짧게 깨묾 | 모래색·적갈색의 각진 송곳니 궤적 | 작고 날카로운 모래색 충격 |
| Jaguar / 03.Leopard | 머리를 앞으로 뻗으며 물기 | 황록·아이보리의 가는 전방 속도선 | 양쪽이 모이는 작은 물기 섬광 |
| Reindeer / 03.Reindeer | 고개와 뿔을 아래로 내리침 | 옅은 얼음색의 가는 뿔 찌르기 속도선 | 작은 얼음빛 충격과 파편 |

게임 내부 enum과 애니메이션은 Jaguar이며, 데이터 파일명은 03.Leopard입니다. 기존 이름과 참조는 변경하지 않았습니다.

## 아트와 등급 차이

내장 image_gen을 사용했습니다. `prompts.json`에 전체 프롬프트와 참조 역할을 보관했습니다. 기존 5등급 tiger 시트는 선화·셀 채색·4열 2행 배치의 스타일 참조로만 사용했습니다. 기존 아트는 덮어쓰지 않았습니다.

각 PNG는 1774×887 RGBA이며 윗줄 공격 4프레임, 아랫줄 피격 4프레임입니다. 4등급은 1~2개의 주요 궤적과 적은 파편으로 표현하고, 5등급의 다중 궤적·큰 타격 표현보다 시각적 밀도를 낮췄습니다.

- 공격 재생: 0.20초 (5등급 기존 0.24초)
- 피격 재생: 0.18초 (5등급 기존 0.24초)
- 공격 폭 배수: 0.40~0.55 (5등급 기존 0.80)
- 피격 폭 배수: 0.42~0.50 (5등급 기존 0.90)

아트 설치 위치: `Assets/MoePang_Art/00.Sprites/00.Unit/02.UnitEffect/`

- giraffe-grade4-attack-hit-sheet.png
- hippo-grade4-attack-hit-sheet.png
- hyena-grade4-attack-hit-sheet.png
- jaguar-grade4-attack-hit-sheet.png
- reindeer-grade4-attack-hit-sheet.png

Importer는 기존 5등급과 동일한 Texture2D / Mipmap Off / NPOT None / Clamp / Bilinear / Uncompressed / Alpha Is Transparency / Max 2048입니다. 이미지 투명도는 그대로 보존했습니다.

## 설정 및 전투 연결

새 설정: `Assets/Resources/ScriptableObjects/00.Unit/02.UnitEffect/GradeFourEffects.asset`

기존 GradeFiveEffectLibrary 타입을 공유하므로 기존 5등급 ScriptableObject의 스크립트 GUID와 참조가 유지됩니다. 런타임은 GradeFiveEffects와 GradeFourEffects를 각각 읽고 등록된 4·5등급 Friendly 유닛만 처리합니다. 1~3등급과 Enemy 유닛은 제외됩니다.

TripleAttack의 PeekTargets(enemyTeam, 3), 순서, Castle 사거리 검사, 피해량과 공격 타이밍은 유지했습니다. 첫 유효 대상에서 공격자 자식 이펙트를 한 번 생성하고, 선택된 Unit 대상마다 그 대상의 자식으로 공격자 스타일의 피격 이펙트를 생성합니다. 대상이 없으면 생성하지 않습니다. Castle에는 기존 피해 처리를 사용합니다.

기존 TakeGradeFiveAreaDamage 비공개 헬퍼는 TakeUnitEffectDamage로 일반화했습니다. 공통 피격 이펙트 억제와 finally 복원 로직은 유지했습니다. GradeFiveEffectPlayer의 부모 연결·스케일 보존·레이어·정렬·프레임·페이드는 그대로 사용합니다.

유닛별 Attack Position Offset은 실제 타격 자세의 머리·입·뿔 높이에 맞춰 다르게 설정했습니다. Hit Position Offset은 피격 대상의 몸통 위치를 기준으로 하며 기본값 0입니다. X는 공격 방향으로 자동 반전되는 부모 로컬 좌표이고 Y는 위입니다.

## Preview

`Tools → MoePang → Unit VFX Preview`에서 4등급 / 5등급을 선택합니다. 기존 `Grade Five VFX Preview` 메뉴도 같은 창을 여는 호환 경로로 유지했습니다.

등급마다 해당 ScriptableObject와 5종의 UnitData를 읽습니다. 기존 부모 계층 재현, 실제 Attack 클립, 타이밍, 공격·피격 모드, 좌우 반전, 시간 슬라이더, 확대, 이펙트 정점, 실시간 설정·Undo·저장을 그대로 사용합니다. 피격 모드는 선택한 대상 하나와의 위치를 보여주며 실제 TripleAttack의 최대 3대상 선택을 시뮬레이션하는 전투 모드는 아닙니다.

BattleArea → UnitPool의 비균일 부모 스케일을 읽습니다. 기본 전투 씬에서 유닛 worldScale은 약 (0.043277, 0.045179, 1)입니다. 창의 확대는 표시 배율만 바꾸며 유닛과 VFX의 월드 비율을 바꾸지 않습니다.

## 검증

- PNG 5종: 각 1774×887, 투명 픽셀 87.8~94.5%, 40개 셀에 유효한 알파 픽셀 존재.
- 총 80개 프레임(4·5등급): 참조 및 시트 영역 정상.
- 4·5등급 10종 지원 / 1~3등급 15종 제외 확인.
- 20개 TripleAttack 검증: 5종 × 대상 0/1/3/4개. 기존 최근접 최대 3대상의 피해 횟수·피해량, 공격 VFX 1회, 대상별 자식 피격 VFX, 대상 없음의 무출력 통과.
- 120개 Preview 일치 검증: 4·5등급 10종 × 공격·피격 × 좌우 × 기본·0·임의 오프셋. 실제 전투 부모 계층에서 생성한 VFX와 Preview의 유닛 worldScale, 이펙트 worldPosition·bounds·방향·프레임 일치 통과. 결과는 `preview-parity-check.txt`에 보관합니다.
- `previews/`는 실제 Preview 렌더 캡처입니다.

검증 도구: `Tools → MoePang → Validate Grade Four VFX`. GradeFourVFXProbe.cs 전체는 UNITY_EDITOR 조건부이므로 빌드에는 포함되지 않습니다. 임시 프리뷰 씬과 피해 기록용 유닛을 사용하고 기존 큐 singleton을 복구합니다. 실제 SoundManager·HP 애니메이션·게임 저장은 실행하지 않습니다. 실제 전투 전체의 성능이나 카메라 가림까지 자동 검증했다는 의미는 아닙니다.

## 전달

MoePang_Art는 별도 Git 서브모듈이므로 새 PNG/.meta와 메인 프로젝트의 C#·설정 에셋을 함께 전달해야 합니다. 이번 작업은 커밋·푸시하지 않습니다. 기존 GameData.json 등 별도 변경은 작업 범위에 포함하지 않습니다.
