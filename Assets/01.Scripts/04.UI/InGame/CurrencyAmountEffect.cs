using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    // Presentation only. The caller has already granted and saved the currency.
    [DisallowMultipleComponent]
    public sealed class CurrencyAmountEffect : MonoBehaviour
    {
        [SerializeField] private Color gainColor = new(0.55f, 1f, 0.55f);
        [SerializeField] private Color spendColor = new(1f, 0.45f, 0.4f);
        [SerializeField, Min(0)] private float rightGap = 12f;
        [SerializeField, Min(0)] private float floatDistance = 18f;

        private TextMeshProUGUI _total;
        private TextMeshProUGUI _delta;
        private Sequence _sequence;
        private int _finalValue;
        private float _baseFontSize;
        private float _rise;

        public void Play(TextMeshProUGUI total, int from, int to, float delay = 0.5f, float duration = 0.9f)
        {
            Finish();
            if (_total != total && total) _baseFontSize = total.fontSize;
            _total = total;
            _finalValue = to;
            if (!_total) return;
            _total.textWrappingMode = TextWrappingModes.NoWrap;
            _total.enableAutoSizing = true;
            _total.fontSizeMax = _baseFontSize;
            _total.fontSizeMin = Mathf.Max(10f, _baseFontSize * 0.5f);
            SetDisplayedValue(from);
            long change = (long)to - from;
            if (change == 0 || !isActiveAndEnabled || !_total.isActiveAndEnabled)
            {
                SetDisplayedValue(to);
                return;
            }

            EnsureDelta();
            _delta.text = (change > 0 ? "+" : "") + change.ToString("N0");
            _delta.color = change > 0 ? gainColor : spendColor;
            _delta.fontSize = _total.fontSize * 0.78f;
            _delta.alpha = 0;
            _rise = 0;
            _delta.gameObject.SetActive(true);
            _delta.rectTransform.sizeDelta = new Vector2(Mathf.Min(140f, _delta.GetPreferredValues(_delta.text).x + 4f),
                Mathf.Max(_total.rectTransform.rect.height, _total.fontSize * 1.5f));
            UpdateDeltaPosition();

            // Independent update is required: GameClearUI pauses the battle with timeScale = 0.
            _sequence = DOTween.Sequence().SetUpdate(true);
            float alpha = 0;
            _sequence.Append(DOTween.To(() => alpha, value => { alpha = value; if (_delta) _delta.alpha = value; }, 1f, 0.12f));
            _sequence.AppendInterval(Mathf.Max(0, delay));
            double progress = 0;
            _sequence.Append(DOTween.To(() => progress, value =>
            {
                progress = value;
                SetDisplayedValue((int)Math.Round(from + ((double)to - from) * value));
            }, 1d, Mathf.Max(0.01f, duration)).SetEase(Ease.OutCubic));
            _sequence.AppendCallback(() => SetDisplayedValue(to));
            _sequence.AppendInterval(0.15f);
            _sequence.Append(DOTween.To(() => alpha, value => { alpha = value; if (_delta) _delta.alpha = value; }, 0f, 0.25f));
            _sequence.Join(DOTween.To(() => _rise, value => { _rise = value; UpdateDeltaPosition(); }, floatDistance, 0.25f));
            _sequence.OnComplete(() =>
            {
                _sequence = null;
                SetDisplayedValue(_finalValue);
                if (_delta) _delta.gameObject.SetActive(false);
            });
        }

        private void EnsureDelta()
        {
            if (_delta) return;
            var instance = new GameObject("CurrencyDelta", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            instance.layer = _total.gameObject.layer;
            instance.transform.SetParent(_total.transform, false);
            _delta = instance.GetComponent<TextMeshProUGUI>();
            _delta.font = _total.font;
            _delta.fontSharedMaterial = _total.fontSharedMaterial;
            _delta.fontStyle = _total.fontStyle;
            _delta.fontWeight = _total.fontWeight;
            _delta.alignment = TextAlignmentOptions.MidlineLeft;
            _delta.textWrappingMode = TextWrappingModes.NoWrap;
            _delta.enableAutoSizing = true;
            _delta.fontSizeMin = Mathf.Max(10f, _total.fontSize * 0.45f);
            _delta.fontSizeMax = _total.fontSize * 0.78f;
            _delta.overflowMode = TextOverflowModes.Overflow;
            _delta.raycastTarget = false;
            var rect = _delta.rectTransform;
            rect.anchorMin = rect.anchorMax = _total.rectTransform.pivot;
            rect.pivot = new Vector2(0, 0.5f);
        }

        private void SetDisplayedValue(int value)
        {
            if (!_total) return;
            _total.text = value.ToString("N0");
            _total.ForceMeshUpdate();
            UpdateDeltaPosition();
        }

        private void UpdateDeltaPosition()
        {
            if (!_total || !_delta) return;
            // Follow the rendered digits, not the text box edge, including digit-count changes.
            Bounds bounds = _total.textBounds;
            _delta.rectTransform.anchoredPosition = new Vector2(bounds.max.x + rightGap, bounds.center.y + _rise);
        }

        public void Finish()
        {
            _sequence?.Kill();
            _sequence = null;
            if (_total) SetDisplayedValue(_finalValue);
            if (_delta) _delta.gameObject.SetActive(false);
        }

        private void OnDisable() => Finish();
        private void OnDestroy() => _sequence?.Kill();
    }
}
