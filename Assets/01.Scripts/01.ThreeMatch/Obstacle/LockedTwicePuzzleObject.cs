using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class LockedTwicePuzzleObject : ObstaclePuzzleObject
    {
        [SerializeField] private Image lockedImage;
        private TextMeshProUGUI _lockedCountText;
        public bool isLocked = true;
        private int _lockedCount = 2;

        private void Awake()
        {
            _lockedCountText = lockedImage.GetComponentInChildren<TextMeshProUGUI>();
            _lockedCountText.text = _lockedCount.ToString();
        }

        public IEnumerator Unlock()
        {
            _lockedCount--;

            switch (_lockedCount)
            {
                case > 0:
                    _lockedCountText.text = _lockedCount.ToString();
                    break;
                case 0:
                    Destroy(lockedImage.gameObject);
                    break;
                case <= -1:
                    isLocked = false;
                    break;
            }

            yield return null;
        }
    }
}
