using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _01.Scripts._08.Utility
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonEffect : MonoBehaviour, IPointerDownHandler, ISubmitHandler
    {
        [SerializeField, Min(0.01f)] private float duration = 0.15f;
        [SerializeField, Min(0f)] private float scaleAmount = 0.1f;
        private Button _button;

        private void Awake() => _button = GetComponent<Button>();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Play();
            }
        }

        public void OnSubmit(BaseEventData eventData) => Play();

        private void Play()
        {
            if (!isActiveAndEnabled || _button == null || !_button.IsActive() || !_button.IsInteractable())
            {
                return;
            }
            
            UIEffect.ButtonClickPunch(transform, duration, scaleAmount);
        }

        private void OnDisable() => UIEffect.StopButtonClickPunch(transform);
    }
}
