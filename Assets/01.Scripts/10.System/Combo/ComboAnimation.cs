using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace _01.Scripts._10.System.Combo
{
    public class ComboAnimation : MonoBehaviour
    {
        [SerializeField] private List<GameObject> comboImages = new List<GameObject>();
        [Tooltip("기준 위치에서 등장하는 방향과 이동 거리입니다. 퇴장은 반대 방향으로 이동합니다.")]
        [SerializeField] private Vector2 travelOffset = new Vector2(900f, 400f);

        [Header("1. 등장 (빠르게 시작 → 감속)")]
        [Tooltip("초당 평균 이동 거리(UI 단위). 값이 클수록 빠르게 등장합니다.")]
        [SerializeField, Min(0.01f)] private float enterSpeed = 1800f;

        [Header("2. 머무름 (일정한 저속 이동)")]
        [Tooltip("머무는 동안 초당 이동 거리(UI 단위). 0이면 제자리에 머뭅니다.")]
        [SerializeField, Min(0f)] private float staySpeed = 25f;
        [Tooltip("천천히 이동하며 머무는 시간(초). 0이면 머무름을 건너뜁니다.")]
        [SerializeField, Min(0f)] private float stayDuration = 0.6f;

        [Header("3. 퇴장 (느리게 시작 → 가속)")]
        [Tooltip("초당 평균 이동 거리(UI 단위). 값이 클수록 빠르게 퇴장합니다.")]
        [SerializeField, Min(0.01f)] private float exitSpeed = 2800f;

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
            Vector2 originalPosition = _originalPositions[index];
            float travelDistance = travelOffset.magnitude;
            float enterDuration = Mathf.Max(0.01f, travelDistance / Mathf.Max(0.01f, enterSpeed));
            float exitDuration = Mathf.Max(0.01f, travelDistance / Mathf.Max(0.01f, exitSpeed));
            float driftDuration = Mathf.Max(0f, stayDuration);
            Vector2 stayEndPosition = originalPosition -
                travelOffset.normalized * Mathf.Max(0f, staySpeed) * driftDuration;

            image.anchoredPosition = originalPosition + travelOffset;
            image.gameObject.SetActive(true);

            Sequence sequence = DOTween.Sequence()
                .Append(image.DOAnchorPos(originalPosition, enterDuration).SetEase(Ease.OutQuart));

            if (driftDuration > 0f)
                sequence.Append(image.DOAnchorPos(stayEndPosition, driftDuration).SetEase(Ease.Linear));

            _sequences[index] = sequence
                .Append(image.DOAnchorPos(stayEndPosition - travelOffset, exitDuration).SetEase(Ease.InQuart))
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

