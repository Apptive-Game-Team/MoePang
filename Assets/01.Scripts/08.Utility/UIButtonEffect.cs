using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _01.Scripts._08.Utility
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField, Min(0.01f)] private float duration = 0.15f;
        [SerializeField, Min(0f)] private float scaleAmount = 0.1f;
        [SerializeField] private RectTransform visualTarget;
        [SerializeField, Range(0f, 0.1f)] private float pressScaleAmount = 0.03f;
        [SerializeField, Range(0f, 1f)] private float pressedBrightness = 0.88f;

        private Button _button;
        private Transform _animatedTarget;
        private Vector3 _restScale;
        private Tween _scaleTween;
        private int? _pointerId;
        private Graphic _tintedGraphic;
        private Color _restColor;

        private bool CanAnimate => isActiveAndEnabled && _button.IsActive() && _button.IsInteractable();

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _pointerId.HasValue || !CanAnimate)
            {
                return;
            }

            _pointerId = eventData.pointerId;
            Press();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_pointerId == eventData.pointerId && CanAnimate)
            {
                Press();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_pointerId == eventData.pointerId)
            {
                Release();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && _pointerId == eventData.pointerId)
            {
                _pointerId = null;
                Release();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && CanAnimate)
            {
                PlayConfirmation();
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (CanAnimate)
            {
                PlayConfirmation();
            }
        }

        private void LateUpdate()
        {
            if (_pointerId.HasValue && !CanAnimate)
            {
                ResetEffect();
            }
        }

        private void Press()
        {
            BeginScale();
            _scaleTween = _animatedTarget.DOScale(_restScale * (1f - pressScaleAmount), 0.06f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            Graphic graphic = _button.targetGraphic != null ? _button.targetGraphic : _button.image;
            if (_button.transition == Selectable.Transition.None && graphic != null && _tintedGraphic == null)
            {
                _tintedGraphic = graphic;
                _restColor = graphic.color;
                graphic.color = new Color(_restColor.r * pressedBrightness, _restColor.g * pressedBrightness,
                    _restColor.b * pressedBrightness, _restColor.a);
            }
        }

        private void Release()
        {
            RestoreColor();
            if (_animatedTarget == null)
            {
                return;
            }

            _scaleTween?.Kill();
            TrackReturn(_animatedTarget.DOScale(_restScale, 0.08f).SetEase(Ease.OutQuad));
        }

        private void PlayConfirmation()
        {
            RestoreColor();
            BeginScale();

            float expansion = Mathf.Min(scaleAmount, 0.05f);
            Sequence sequence = DOTween.Sequence()
                .Append(_animatedTarget.DOScale(_restScale * (1f + expansion), duration * 0.4f).SetEase(Ease.OutQuad))
                .Append(_animatedTarget.DOScale(_restScale, duration * 0.6f).SetEase(Ease.OutQuad));
            TrackReturn(sequence);
        }

        private void BeginScale()
        {
            _scaleTween?.Kill();
            if (_animatedTarget != null)
            {
                return;
            }

            Graphic graphic = _button.targetGraphic != null ? _button.targetGraphic : _button.image;
            _animatedTarget = visualTarget != null ? visualTarget : graphic != null ? graphic.rectTransform : transform;
            _restScale = _animatedTarget.localScale;
        }

        private void TrackReturn(Tween tween)
        {
            _scaleTween = tween.SetUpdate(true).OnComplete(() =>
            {
                _animatedTarget = null;
                _scaleTween = null;
            });
        }

        private void RestoreColor()
        {
            if (_tintedGraphic != null)
            {
                _tintedGraphic.color = _restColor;
            }

            _tintedGraphic = null;
        }

        private void ResetEffect()
        {
            _pointerId = null;
            _scaleTween?.Kill();
            _scaleTween = null;

            if (_animatedTarget != null)
            {
                _animatedTarget.localScale = _restScale;
                _animatedTarget = null;
            }

            RestoreColor();
        }

        private void OnDisable()
        {
            ResetEffect();
        }
    }
}
