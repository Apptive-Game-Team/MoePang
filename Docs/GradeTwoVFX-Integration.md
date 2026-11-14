# 모에팡 2등급 공격·피격 VFX

## 범위

3등급 작업은 사용자 요청으로 중단했습니다. 해당 아트·코드는 준비 폴더에만 있으며 Unity 프로젝트에는 설치하지 않았습니다. 이번 변경은 2등급 5종의 단일 근접 공격과 Preview만 확장합니다. 3등급 투사체 및 혀 로직을 변경하지 않습니다.

## 모션 분석

모두 Friendly / MeleeAttack / attackSpeed 1.5 / unitSize 0.4 / 기본 attackDamage 6입니다. 기본 공격 주기 약 0.667초의 1/3 시점인 약 0.222초에 공격 판정이 발생합니다. 실제 Attack 클립 0.35초에 Animator 속도 1.5가 적용됩니다.

| 유닛 | 확인한 준비→타격 자세 | 공격 표현 | 피격 표현 |
| --- | --- | --- | --- |
| 사슴 (enum Dear) | 앞발을 들었다 내려찍음 | 짧은 하강 표시와 낮은 흙먼지 | 두 갈래의 둔한 발굽 압흔 |
| 바다거북 (Seaturtle) | 입을 벌린 후 목을 뻗으며 닫음 | 짧고 뭉툭한 물기 압점 | 작은 각진 집힘 자국 |
| 아르마딜로 (Armadillo) | 몸을 말았다 앞으로 굴러 부딪힘 | 짧은 직선 속도선과 각진 먼지 | 둔한 충돌점과 작은 자갈 |
| 카피바라 (Capybara) | 상체를 들었다 앞으로 낮추며 부딪힘 | 넓고 납작한 작은 압축 덩어리 | 둔탁한 몸통 충격점 |
| 펭귄 (Penguin) | 상체를 세웠다 부리를 앞으로 내밈 | 가늘고 곧은 부리 찌르기 표시 | 작은 비대칭 타점 |

기존 enum과 데이터 파일명은 유지했습니다. 실제 두 공격 스프라이트를 직접 확인하여 설계했습니다.

## 아트 스타일

내장 image_gen을 사용했습니다. 기존 동물 스프라이트의 굵고 단순한 색 선과 평면 채색을 참조했습니다. 사슴 시안을 먼저 제작·검토한 뒤 나머지 4종의 스타일 기준으로 사용했습니다. 전체 프롬프트는 prompts.json에 보관합니다.

이번 스타일 기준은 적은 수의 평면색, 단순한 외곽선, 작은 물리적 타점입니다. 초승달·발톱 궤적, 광채, 금속성 입체 채색, 유리 같은 하이라이트, 화려한 마법 파편을 사용하지 않도록 생성했습니다. 4·5등급보다 작은 크기와 짧은 재생으로 등급 차이를 둡니다.

각 PNG는 위쪽 공격 4칸, 아래쪽 피격 4칸의 4열×2행 투명 시트입니다. 생성 알파를 보존하며 기존 아트를 덮어쓰지 않습니다.

설치 위치: Assets/MoePang_Art/00.Sprites/00.Unit/02.UnitEffect/

- deer-grade2-attack-hit-sheet.png
- seaturtle-grade2-attack-hit-sheet.png
- armadillo-grade2-attack-hit-sheet.png
- capybara-grade2-attack-hit-sheet.png
- penguin-grade2-attack-hit-sheet.png

## 전투·설정·Preview

새 설정은 Assets/Resources/ScriptableObjects/00.Unit/02.UnitEffect/GradeTwoEffects.asset입니다. 기존 GradeFiveEffectLibrary 타입을 공유하여 기존 4·5등급의 스크립트 GUID와 참조를 보존합니다. Supports는 Friendly 2·4·5등급의 등록된 유닛만 허용하며 1·3등급은 제외합니다.

MeleeAttack의 타겟 검색, 사거리·방향 검사 후 공격자 자식으로 전용 공격 이펙트를 1회 생성합니다. 대상이 Unit이면 기존 TakeUnitEffectDamage를 사용해 대상의 자식으로 전용 피격 이펙트를 표시합니다. 공통 피격 효과 중복 억제·finally 복원은 기존 코드를 공유합니다. Castle에는 기존 피해 처리를 사용합니다. 피해량, 공격 주기, 판정, 3등급 투사체 코드는 유지합니다.

기본 공격 재생은 0.12초, 피격은 0.10초입니다. 유닛별 Attack Position Offset은 실제 발·입·몸통·부리 위치에 맞춥니다. Hit Position Offset은 대상 몸통 기준입니다. X는 공격 방향에 맞춰 반전되는 부모 로컬 좌표입니다.

Tools → MoePang → Unit VFX Preview에서 2·4·5등급을 선택합니다. 기존 Grade Five VFX Preview 메뉴도 같은 창을 엽니다. 실제 부모 계층의 비균일 스케일, 애니메이션, 시간 슬라이더, 이펙트 정점, 좌우 반전, 확대, 설정 Undo·저장을 유지합니다. 3등급은 선택 목록에서 제외합니다.

2등급에서는 작은 유닛을 편하게 볼 수 있도록 기본 카메라 프레이밍을 가까이 잡습니다. 카메라 배율만 변경하며 유닛·VFX의 월드 크기나 상대 좌표는 바꾸지 않습니다.

## 검증과 전달

검증 결과는 validation.txt와 preview-parity-check.txt, 실제 렌더는 previews/에 보관합니다. 에디터 전용 GradeTwoVFXProbe.cs는 UNITY_EDITOR로 감싸 빌드에서 제외합니다. Tools → MoePang → Validate Grade Two VFX로 피해 기록용 유닛과 임시 Preview 씬을 사용해 다시 검사할 수 있습니다. 기존 큐 singleton을 복구하고 SoundManager·게임 저장은 실행하지 않습니다.

- Unity 컴파일 통과.
- 2등급 5종 × 대상 없음·사거리 내·사거리 밖·뒤쪽 = 20개 근접 공격 검증 통과. 피해량·사거리·방향 검사와 공격·피격 이펙트 수·부모 연결 확인.
- 기존 4등급 20개 TripleAttack 회귀 검증 통과.
- Friendly 2·4·5등급 15종 지원, 1·3등급 10종 제외 확인.
- 신규 PNG 5종 1774×887, 유효한 40프레임, 투명 픽셀 96.5~97.0% 확인.
- 180개 Preview 일치 검증 통과: 2·4·5등급 15종 × 공격·피격 × 좌우 × 기본·0·임의 오프셋. 전투 부모 계층과 Preview의 유닛 월드 스케일, 이펙트 위치·bounds·방향·프레임이 일치합니다. 실제 전투 전체의 카메라 가림·동시 공격 성능을 자동 검증했다는 뜻은 아닙니다.

기존 4·5등급 설정과 3등급 투사체 코드의 작업 전 해시는 baseline-hashes.json으로 보관합니다. MoePang_Art는 별도 Git 서브모듈이므로 PNG/.meta와 메인 저장소의 코드·설정을 함께 전달해야 합니다. 커밋·푸시는 수행하지 않습니다.
