using UnityEngine;
using UnityEngine.EventSystems;

namespace _01.Scripts._10.System.Combo
{
    public class ComboDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ComboUIObject _parentComboUIObject;

        private void Awake()
        {
            _parentComboUIObject = GetComponentInParent<ComboUIObject>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_parentComboUIObject != null)
            {
                _parentComboUIObject.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_parentComboUIObject != null)
            {
                _parentComboUIObject.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_parentComboUIObject != null)
            {
                _parentComboUIObject.OnEndDrag(eventData);
            }
        }
    }
}