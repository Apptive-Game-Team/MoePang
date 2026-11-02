using UnityEngine;
using DG.Tweening;

namespace _01.Scripts._08.Utility
{
    public static class UIEffect
    {
        public static void ButtonClickPunch(Transform target, float duration = 0.15f, float scaleAmount = 0.1f)
        {
            target.DOKill();

            target.DOPunchScale(target.localScale * scaleAmount, duration, 1, 0.5f);
        }
    }
}
