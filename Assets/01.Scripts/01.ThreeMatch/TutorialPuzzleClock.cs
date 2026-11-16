using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    // 튜토리얼 퍼즐과 에너지 스택만 전투 정지 중에도 진행합니다.
    public static class TutorialPuzzleClock
    {
        public static float DeltaTime => TutorialManager.UseUnscaledPuzzleTime
            ? (TutorialManager.IsTutorialPopupOpen ? 0f : Time.unscaledDeltaTime)
            : Time.deltaTime;

        public static T UseTutorialPuzzleClock<T>(this T tween, object owner) where T : Tween
        {
            if (TutorialManager.UseUnscaledPuzzleTime)
            {
                tween.SetUpdate(true).SetId(owner);
                if (TutorialManager.IsTutorialPopupOpen)
                    tween.Pause();
            }
            return tween;
        }

        public static IEnumerator WaitForSeconds(float seconds)
        {
            // 일반 플레이의 기존 scaled wait 동작은 유지합니다.
            if (!TutorialManager.UseUnscaledPuzzleTime)
            {
                yield return new UnityEngine.WaitForSeconds(seconds);
                yield break;
            }

            float elapsed = 0f;
            while (TutorialManager.IsTutorialPopupOpen || elapsed < seconds)
            {
                bool popupWasOpen = TutorialManager.IsTutorialPopupOpen;
                yield return null;
                if (!popupWasOpen && !TutorialManager.IsTutorialPopupOpen)
                    elapsed += DeltaTime;
            }
        }
    }
}
