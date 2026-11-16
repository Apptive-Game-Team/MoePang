using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace _01.Scripts._10.System.Combo
{
    public class ComboAnimation : MonoBehaviour
    {
        [SerializeField] private List<GameObject> comboImages = new List<GameObject>();
        [SerializeField] private Vector2 travelOffset = new Vector2(900f, 400f);
        [SerializeField, Min(0.01f)] private float enterDuration = 0.55f;
        [SerializeField, Min(0.01f)] private float exitDuration = 0.35f;

        private RectTransform[] _images;
        private Vector2[] _originalPositions;
        private Sequence[] _sequences;

        private void Awake()
        {
            _images = new RectTransform[comboImages.Count];
            _originalPositions = new Vector2[comboImages.Count];
            _sequences = new Sequence[comboImages.Count];

            for (int i = 0; i < comboImages.Count; i++)
            {
                if (comboImages[i] == null) continue;

                _images[i] = comboImages[i].GetComponent<RectTransform>();
                if (_images[i] == null) continue;

                _originalPositions[i] = _images[i].anchoredPosition;
                comboImages[i].SetActive(false);
            }
        }

        // List order: Meadow, Ocean, Desert, Forest, Polar (Habitat enum order).
        public void PlayCombo(Habitat habitat)
        {
            int index = (int)habitat;
            if (!isActiveAndEnabled || _images == null || index < 0 || index >= _images.Length) return;

            RectTransform image = _images[index];
            if (image == null) return;

            _sequences[index]?.Kill();
            image.anchoredPosition = _originalPositions[index] + travelOffset;
            image.gameObject.SetActive(true);

            _sequences[index] = DOTween.Sequence()
                .Append(image.DOAnchorPos(_originalPositions[index], enterDuration).SetEase(Ease.OutQuart))
                .Append(image.DOAnchorPos(_originalPositions[index] - travelOffset, exitDuration).SetEase(Ease.InQuart))
                .OnComplete(() =>
                {
                    image.gameObject.SetActive(false);
                    image.anchoredPosition = _originalPositions[index];
                    _sequences[index] = null;
                });
        }

        private void OnDisable()
        {
            if (_images == null) return;

            for (int i = 0; i < _images.Length; i++)
            {
                _sequences[i]?.Kill();
                _sequences[i] = null;
                if (_images[i] == null) continue;

                _images[i].gameObject.SetActive(false);
                _images[i].anchoredPosition = _originalPositions[i];
            }
        }
    }
}

