using _01.Scripts._04.UI.MainScene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _01.Scripts._10.System.Combo
{
    public class ComboUIObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Habitat habitat;
        
        private Transform _parentContent;
        private GameObject _placeholder;
        private LayoutElement _layoutElement;
        private ComboUI _comboUI;
        
        private ScrollRect _scrollRect;
        private RectTransform _scrollWindow;
        private bool _isDragging;
        private Vector2 _currentMousePosition;
        
        [Header("Auto Scroll Settings")]
        [SerializeField] private float scrollSpeed = 2.5f;
        [SerializeField] private float scrollThreshold = 100f;

        private void Awake()
        {
            _parentContent = transform.parent;
            _layoutElement = GetComponent<LayoutElement>();
        
            if (_layoutElement == null)
            {
                _layoutElement = gameObject.AddComponent<LayoutElement>();
            }
            
            _scrollRect = GetComponentInParent<ScrollRect>();
            if (_scrollRect != null)
            {
                _scrollWindow = _scrollRect.GetComponent<RectTransform>();
            }
        }

        public void Initialize(ComboUI comboUI)
        {
            _comboUI = comboUI;
        }

        private void Update()
        {
            if (_isDragging && _scrollRect != null && _scrollWindow != null)
            {
                HandleAutoScroll();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _currentMousePosition = eventData.position;
            
            _placeholder = new GameObject("Combo_Placeholder");
            _placeholder.transform.SetParent(_parentContent);
        
            LayoutElement placeHolderElement = _placeholder.AddComponent<LayoutElement>();
            RectTransform myRect = GetComponent<RectTransform>();
            placeHolderElement.preferredWidth = myRect.rect.width;
            placeHolderElement.preferredHeight = myRect.rect.height;

            _placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());
            
            _layoutElement.ignoreLayout = true;

            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            _currentMousePosition = eventData.position;
            
            int newSiblingIndex = _parentContent.childCount - 1;

            for (int i = 0; i < _parentContent.childCount; i++)
            {
                Transform child = _parentContent.GetChild(i);

                if (child == transform || child == _placeholder.transform) continue;

                if (eventData.position.y > child.position.y)
                {
                    newSiblingIndex = child.GetSiblingIndex();
                
                    if (_placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                    {
                        newSiblingIndex--;
                    }
                    break;
                }
            }

            _placeholder.transform.SetSiblingIndex(newSiblingIndex);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            
            transform.SetSiblingIndex(_placeholder.transform.GetSiblingIndex());

            _layoutElement.ignoreLayout = false;

            Destroy(_placeholder);

            _comboUI.OnOrderChanged();
        }
        
        private void HandleAutoScroll()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_scrollWindow, _currentMousePosition, null, out Vector2 localMousePos);
            
            float rectTop = _scrollWindow.rect.yMax;
            float rectBottom = _scrollWindow.rect.yMin;
            
            if (rectTop - localMousePos.y < scrollThreshold)
            {
                _scrollRect.verticalNormalizedPosition += scrollSpeed * Time.deltaTime;
                _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition);
            }
            else if (localMousePos.y - rectBottom < scrollThreshold)
            {
                _scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
                _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition);
            }
        }
    }
}