using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer battleBackgroundSr;
        
        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minOrthographicSize = 2f;
        [SerializeField] private float maxOrthographicSize = 5f;
        
        [Header("Drag")]
        [SerializeField] private float dragSpeed = 0.5f;

        private Camera _battleCam;
        private bool _isDragging;
        private Vector3 _lastMousePos;
        private float _targetZoom;
        private float _groundYAnchor;

        private void Awake()
        {
            _battleCam = GetComponent<Camera>();
            _targetZoom = _battleCam.orthographicSize;
            _groundYAnchor = battleBackgroundSr.bounds.min.y;
        }

        private void Update()
        {
            HandleZoom();
            HandleDrag();
        }

        private void HandleZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            
            if (scroll == 0)
            {
                return;
            }
            
            _targetZoom -= scroll * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minOrthographicSize, maxOrthographicSize);
            _battleCam.orthographicSize = _targetZoom;

            float t = (maxOrthographicSize - _battleCam.orthographicSize) / (maxOrthographicSize - minOrthographicSize);
            
            Vector3 currentPos = transform.position;
            
            float targetY = Mathf.Lerp(currentPos.y, _groundYAnchor, t * 0.5f);
    
            transform.position = new Vector3(currentPos.x, targetY, currentPos.z);
            
            ClampCameraInsideBackground();
        }

        private void HandleDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 currentMousePos = Input.mousePosition;
                Vector3 delta = currentMousePos - _lastMousePos;
                Vector3 moveDirection = new Vector3(-delta.x, 0, 0) * (dragSpeed * 0.01f * _battleCam.orthographicSize);
                
                transform.position += moveDirection;
                _lastMousePos = currentMousePos;

                ClampCameraInsideBackground();
            }
        }
        
        private void ClampCameraInsideBackground()
        {
            if (battleBackgroundSr == null)
            {
                return;
            }

            Bounds bgBounds = battleBackgroundSr.bounds;
            
            float camHeight = _battleCam.orthographicSize;
            float camWidth = camHeight * _battleCam.aspect;

            Vector3 camPos = transform.position;
            
            float minX = bgBounds.min.x + camWidth;
            float maxX = bgBounds.max.x - camWidth;
            float minY = bgBounds.min.y + camHeight;
            float maxY = bgBounds.max.y - camHeight;

            camPos.x = minX > maxX ? bgBounds.center.x : Mathf.Clamp(camPos.x, minX, maxX);
            camPos.y = minY > maxY ? bgBounds.center.y : Mathf.Clamp(camPos.y, minY, maxY);

            transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
        }
    }
}