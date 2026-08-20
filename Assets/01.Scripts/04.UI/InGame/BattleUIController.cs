using _01.Scripts._00.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.InGame
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RawImage battleRawImage;
        [SerializeField] private SpriteRenderer battleBackgroundSr;
        [SerializeField] private Transform friendlySpawnPosition;
        [SerializeField] private Transform enemySpawnPosition;
        
        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minOrthographicSize = 2f;
        [SerializeField] private float maxOrthographicSize = 5f;
        
        [Header("Drag")]
        [SerializeField] private float dragSpeed = 0.5f;
        
        [Header("Camera Lock")]
        [SerializeField] private bool isCameraLocked;
        [SerializeField] private float followSpeed = 8f;

        private Camera _battleCam;
        private bool _isDragging;
        private Vector3 _lastMousePos;
        private float _targetZoom;
        private float _cameraBottomY;
        private bool _canDrag;

        private void Awake()
        {
            _battleCam = GetComponent<Camera>();
            _targetZoom = _battleCam.orthographicSize;
            _cameraBottomY = _battleCam.transform.position.y - _battleCam.orthographicSize;
        }

        private void Start()
        {
            bool savedCameraLock = GameManager.Instance == null || GameManager.Instance.gameData.cameraLockEnabled;
            SetCameraLock(savedCameraLock, false);
        }

        private void Update()
        {
            if (Time.timeScale == 0)
            {
                return;
            }
            
            if (isCameraLocked)
            {
                FollowFrontFriendlyTarget();
                return;
            }

            ApplyUnlockedCameraView();
        }

        private void HandleZoom()
        {
            float zoomDelta = 0f;
            Vector3 zoomScreenPos = Vector3.zero;
            
            if (Input.touchCount < 2)
            {
                zoomDelta = Input.mouseScrollDelta.y;
                zoomScreenPos = Input.mousePosition;

                if (!IsPointerInsideBattleCamera(zoomScreenPos))
                {
                    return;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                if (!IsPointerInsideBattleCamera(touchZero.position))
                {
                    return;
                }
                
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                
                zoomDelta = (touchDeltaMag - prevTouchDeltaMag) * 0.01f;
                
                zoomScreenPos = (touchZero.position + touchOne.position) * 0.5f;
            }
            
            if (Mathf.Approximately(zoomDelta, 0f))
            {
                return;
            }
            
            Vector3 worldPosBeforeZoom = _battleCam.ScreenToWorldPoint(new Vector3(zoomScreenPos.x, zoomScreenPos.y, _battleCam.nearClipPlane));
            
            _targetZoom -= zoomDelta * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minOrthographicSize, maxOrthographicSize);
            _battleCam.orthographicSize = _targetZoom;
            
            Vector3 worldPosAfterZoom = _battleCam.ScreenToWorldPoint(new Vector3(zoomScreenPos.x, zoomScreenPos.y, _battleCam.nearClipPlane));
            
            Vector3 difference = worldPosBeforeZoom - worldPosAfterZoom;
            transform.position += new Vector3(difference.x, difference.y, 0);
            
            Vector3 pos = transform.position;
            pos.y = _cameraBottomY + _battleCam.orthographicSize;
            transform.position = pos;

            ClampCameraInsideBackground();
        }
        
        private bool IsPointerInsideBattleCamera(Vector2 screenPosition)
        {
            if (battleRawImage == null)
            {
                return false;
            }
            
            Canvas canvas = battleRawImage.canvas;
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(battleRawImage.rectTransform, screenPosition, uiCam);
        }

        private void HandleDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _canDrag = IsPointerInsideBattleCamera(Input.mousePosition);
                _lastMousePos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                _canDrag = false;
            }
            
            if (!_isDragging || !_canDrag)
            {
                return;
            }
            
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 delta = currentMousePos - _lastMousePos;
            Vector3 moveDirection = new Vector3(-delta.x, 0, 0) * (dragSpeed * 0.01f * _battleCam.orthographicSize);
            
            transform.position += moveDirection;
            _lastMousePos = currentMousePos;

            ClampCameraInsideBackground();
        }
        
        private void ClampCameraInsideBackground()
        {
            if (!battleBackgroundSr)
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
        
        public void ToggleCameraLock()
        {
            SetCameraLock(!isCameraLocked);
        }

        public void SetCameraLock(bool isOn)
        {
            SetCameraLock(isOn, true);
        }

        private void SetCameraLock(bool isOn, bool saveGameData)
        {
            isCameraLocked = isOn;

            if (isCameraLocked)
            {
                _targetZoom = minOrthographicSize;
                _battleCam.orthographicSize = _targetZoom;

                Vector3 pos = transform.position;
                pos.y = _cameraBottomY + _battleCam.orthographicSize;
                transform.position = pos;

                ClampCameraInsideBackground();
            }
            else
            {
                ApplyUnlockedCameraView();
            }

            if (!saveGameData || GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.gameData.cameraLockEnabled = isCameraLocked;
            GameManager.Instance.SaveGameData();
        }

        private void ApplyUnlockedCameraView()
        {
            _targetZoom = maxOrthographicSize;
            _battleCam.orthographicSize = _targetZoom;

            Vector3 pos = transform.position;

            if (friendlySpawnPosition != null && enemySpawnPosition != null)
            {
                pos = (friendlySpawnPosition.position + enemySpawnPosition.position) * 0.5f;
                pos.z = transform.position.z;
            }
            else
            {
                pos.y = _cameraBottomY + _battleCam.orthographicSize;
            }

            transform.position = pos;
            ClampCameraInsideBackground();
        }
        
        private void FollowFrontFriendlyTarget()
        {
            if (UnitTransformQueue.Instance == null)
            {
                return;
            }

            IDamageable target = UnitTransformQueue.Instance.PeekFrontUnitForCamera(TeamType.Friendly);

            if (target == null)
            {
                target = UnitTransformQueue.Instance.PeekCastle(TeamType.Friendly);
            }

            if (target == null)
            {
                return;
            }

            Vector3 targetPos = target.GetTransform().position;
            Vector3 camPos = transform.position;

            camPos.x = Mathf.Lerp(camPos.x, targetPos.x, followSpeed * Time.deltaTime);
            camPos.y = _cameraBottomY + _battleCam.orthographicSize;

            transform.position = camPos;

            ClampCameraInsideBackground();
        }
    }
}
