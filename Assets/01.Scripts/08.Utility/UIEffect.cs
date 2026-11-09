using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace _01.Scripts._08.Utility
{
    public static class UIEffect
    {
        private static readonly Dictionary<Transform, Tween> ButtonPunches = new();

        public static void ButtonClickPunch(Transform target, float duration = 0.15f, float scaleAmount = 0.1f)
        {
            if (target == null)
            {
                return;
            }
            
            StopButtonClickPunch(target);
            if (!target.gameObject.activeInHierarchy || duration <= 0f)
            {
                return;
            }

            Vector3 originalScale = target.localScale;
            Tweener tween = target.DOPunchScale(originalScale * scaleAmount, duration, 1, 0.5f)
                .SetUpdate(true)
                .SetLink(target.gameObject, LinkBehaviour.KillOnDisable);
            ButtonPunches[target] = tween;
            tween.OnKill(() =>
            {
                if (target != null) target.localScale = originalScale;
                ButtonPunches.Remove(target);
            });
        }

        public static void StopButtonClickPunch(Transform target)
        {
            if (target != null && ButtonPunches.TryGetValue(target, out var tween))
            {
                tween.Kill();
            }
        }
    }
}
