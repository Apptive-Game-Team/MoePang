using UnityEngine;
using UnityEngine.EventSystems;

namespace _01.Scripts._04.UI.MainScene
{
    public class GuidePanelHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private HabitatModeUI habitatModeUI;

        private void Awake()
        {
            if (habitatModeUI == null)
            {
                habitatModeUI = GetComponentInParent<HabitatModeUI>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            habitatModeUI?.ShowGuide();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            habitatModeUI?.HideGuide();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            habitatModeUI?.HideGuide();
        }
    }
}
