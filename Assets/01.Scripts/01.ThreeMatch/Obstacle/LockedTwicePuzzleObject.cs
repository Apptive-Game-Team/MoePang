using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class LockedTwicePuzzleObject : ObstaclePuzzleObject
    {
        [SerializeField] private Image[] lockedImages;
        public bool isLocked = true;
        private int _lockedCount = 2;

        public IEnumerator Unlock(PuzzleGenerator generator, int curX, int curY)
        {
            _lockedCount--;

            int index = _lockedCount;
            Sequence seq = DOTween.Sequence();

            if (index >= 0 && index < lockedImages.Length)
            {
                Image target = lockedImages[index];
                
                seq.Append(target.DOFade(0, 0.2f).SetEase(Ease.Linear));
            }

            if (_lockedCount <= -1)
            {
                isLocked = false;
            }

            yield return null;
        }
    }
}
