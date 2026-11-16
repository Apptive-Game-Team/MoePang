# 게임 클리어 골드 카운트업

GameClearUI에서 기존 AddStageClearedGold 호출 직전·직후 Gold 값을 기록합니다. 결과 패널은 지급 전 금액으로 시작하고, 숫자 오른쪽에 실제 차액(+보상량)을 표시한 뒤 최종 금액까지 카운트업합니다. 기존 보상 공식, 지급 횟수, 일반/서식지 분기, 다이아 보상 및 SavePlayData 호출은 유지합니다.

CurrencyAmountEffect는 표시만 담당합니다. GoldManager를 호출하거나 저장 데이터를 변경하지 않습니다. 실제 보상 지급·저장은 기존 시점에 끝나므로, 패널을 일찍 닫아도 골드는 유지됩니다. 클리어 패널의 coinText에만 자동 연결하며 다른 재화 UI의 이벤트에는 연결하지 않습니다.

기본 연출: +보상량 페이드인 0.12초 → 잠깐 대기 0.25초 → 숫자 카운트업 0.9초 → 유지 0.15초 → 0.25초 동안 위로 떠오르며 페이드아웃. GameClearUI Inspector의 Gold Count Delay / Gold Count Duration으로 대기·카운트업 시간을 조절합니다.

숫자는 N0 형식(천 단위 구분)이고, 실제 렌더링된 숫자의 오른쪽 경계에 +보상량을 배치합니다. 기존 TMP 폰트·재질·스타일과 부모 스케일을 사용합니다. 큰 금액은 줄바꿈을 방지하고 글자 크기를 조절합니다. 보간은 double을 사용하고 최종값을 정확한 int로 고정하여 큰 골드 금액의 부동소수점 반올림 손실을 피합니다.

클리어 화면은 timeScale=0이므로 DOTween Sequence에 SetUpdate(true)를 적용했습니다. 연출 중 다시 호출하면 이전 연출을 정리하고, 비활성화 시 Tween을 취소·최종 금액으로 표시·증감량을 숨깁니다. GameUI의 OnDisable 시간·DepthOfField 복원은 변경하지 않습니다. 별도 CurrencyAmountEffect 컴포넌트가 자체 정리합니다.

변경 파일:

- Assets/01.Scripts/04.UI/InGame/GameClearUI.cs
- Assets/01.Scripts/04.UI/InGame/CurrencyAmountEffect.cs (추가)

검증:

- Unity 컴파일 통과.
- 실제 전투 씬의 CoinInfo UI를 임시 Preview 씬에 복제해 검사. 보상 전, +400 표시, 중간값, 최종값, 증감 표시 종료, 오른쪽 배치 확인.
- timeScale=0에서 독립 업데이트 설정과 Tween 진행값 확인.
- 1,999,999,999 → 2,000,000,000 정수 정확도 및 한 줄 표시 확인.
- 0원 차액, 재호출, 라벨 재사용, OnDisable 정리 경로 확인. 편집 모드에서는 일반 MonoBehaviour 생명주기 메시지가 실행되지 않으므로 종료 검사는 OnDisable을 명시 호출했습니다.
- 검증은 GoldManager·실제 클리어 저장을 실행하지 않았습니다. 실제 보상 통합은 기존 호출 앞뒤의 스냅샷과 coinText 표시 교체로 한정했습니다.

Validate.cs.txt는 Unity MCP execute_code에 전달한 검증 코드, validation.txt는 결과입니다. reward-*.png는 기존 CoinInfo를 복제한 표시 단계별 캡처입니다. 원래 배경 머티리얼은 CameraSortingLayerTexture를 샘플링하므로 UI만 렌더하는 검증 복제본에서는 기본 UI 배경 머티리얼을 사용했습니다. 실제 게임의 배경 머티리얼·저장 파일·전투 씬 에셋은 바꾸지 않았습니다.
