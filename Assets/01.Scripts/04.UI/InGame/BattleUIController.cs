using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 0.2f;
        [SerializeField] private float dragSpeed = 0.1f;
        [SerializeField] private float minScale = 1f;
        [SerializeField] private float maxScale = 3f;
        
        private Camera _cam;
        private SpriteRenderer _sr;
        private SpriteRenderer _parentSr;

        private bool _isDragging;
        private Vector3 _offset;
        private Vector3 _originScale;
        private Vector3 _lastMousePos;


        private void Awake()
        {
            _cam = Camera.main;
            _sr = GetComponent<SpriteRenderer>();
            _parentSr = transform.parent.GetComponent<SpriteRenderer>();
            _originScale = transform.localScale;
        }

        private void Update()
        {
            if (IsMouseInsideParent())
            {
                HandleZoom();
                HandleDrag();
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _isDragging = false;
                }
            }
        }
        
        private bool IsMouseInsideParent()
        {
            if (_parentSr == null) return false;

            Vector3 mouse = MouseWorld();
            Bounds bounds = _parentSr.bounds;

            return bounds.Contains(mouse);
        }

        private void HandleZoom()
        {
            float scroll = Input.mouseScrollDelta.y;

            if (scroll == 0) return;

            float scaleX = transform.localScale.x + scroll * zoomSpeed;
            float scaleY = transform.localScale.y + scroll * zoomSpeed * _originScale.y / _originScale.x;
            scaleX = Mathf.Clamp(scaleX, _originScale.x * minScale, _originScale.x * maxScale);
            scaleY = Mathf.Clamp(scaleY, _originScale.y * minScale, _originScale.y * maxScale);

            transform.localScale = new Vector3(scaleX, scaleY, 1);

            ClampInsideParent();
        }

        private void HandleDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePos = MouseWorld();
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 current = MouseWorld();
                Vector3 delta = current - _lastMousePos;

                transform.position += delta * dragSpeed;
                _lastMousePos = current;

                ClampInsideParent();
            }
        }

        private Vector3 MouseWorld()
        {
            Vector3 pos = Input.mousePosition;
            pos.z = Mathf.Abs(_cam.transform.position.z);
            return _cam.ScreenToWorldPoint(pos);
        }

        private void ClampInsideParent()
        {
            if (transform.parent == null || _sr == null || _parentSr == null) return;

            Bounds child = _sr.bounds;
            Bounds parent = _parentSr.bounds;

            Vector3 pos = transform.position;

            float offsetX = 0f;
            float offsetY = 0f;

            
            if (child.min.x > parent.min.x)
            {
                offsetX = parent.min.x - child.min.x;
            }
            
            if (child.max.x < parent.max.x)
            {
                offsetX = parent.max.x - child.max.x;
            }
            
            if (child.min.y > parent.min.y)
            {
                offsetY = parent.min.y - child.min.y;
            }
            
            if (child.max.y < parent.max.y)
            {
                offsetY = parent.max.y - child.max.y;
            }

            pos.x += offsetX;
            pos.y += offsetY;

            transform.position = pos;
        }
    }
}