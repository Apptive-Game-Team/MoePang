using UnityEngine;
using UnityEngine.EventSystems;

namespace _01.Scripts._04.UI.InGame
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Connected Components")]
        [Tooltip("전투 구역의 배경이나 맵의 SpriteRenderer를 넣어주세요. (카메라 범위를 제한하는 기준)")]
        [SerializeField] private SpriteRenderer battleBackgroundSr;
        
        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minOrthographicSize = 2f;  // 숫자가 작을수록 확대(Zoom In)
        [SerializeField] private float maxOrthographicSize = 5f;  // 숫자가 클수록 축소(Zoom Out)
        
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
            // UI를 클릭했을 때만 작동하도록 마우스 위치 필터링 (RawImage 영역 안에서 스크롤/드래그 되도록)
            // 만약 하단 퍼즐 UI를 만지고 있을 때는 작동하지 않게 방지합니다.
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // TODO: 필요한 경우, 오직 상단 전투 RawImage 위에서만 작동하게 체크하는 로직을 넣을 수 있습니다.
            }

            HandleZoom();
            HandleDrag();
        }

        private void HandleZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll == 0) return;

            // 1. 타겟 줌 수치 변경 및 제한
            _targetZoom -= scroll * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minOrthographicSize, maxOrthographicSize);
            _battleCam.orthographicSize = _targetZoom;

            // 2. [핵심] 현재 줌 상태가 얼마나 확대되었는지 비율 계산 (0: 최대 축소, 1: 최대 확대)
            // t 수치가 1에 가까워질수록(바짝 확대할수록) 카메라는 groundYAnchor(바닥)에 가까워집니다.
            float t = (maxOrthographicSize - _battleCam.orthographicSize) / (maxOrthographicSize - minOrthographicSize);

            // 3. 카메라의 현재 Y 위치를 줌 배율에 따라 바닥 쪽으로 보정
            Vector3 currentPos = transform.position;
    
            // 최대 축소 상태일 때는 원래 자유롭게 드래그하던 Y값을 유지하되, 
            // 확대될수록 지정한 groundYAnchor(바닥)에 강제로 붙도록 Lerp를 걸어줍니다.
            float targetY = Mathf.Lerp(currentPos.y, _groundYAnchor, t * 0.5f); // 0.5f는 고정 강도(원하는 대로 조절)
    
            transform.position = new Vector3(currentPos.x, targetY, currentPos.z);

            // 4. 화면 밖으로 안 나가게 최종 락 걸기
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

                // 카메라가 가만히 있고 월드가 움직이는 게 아니라, 카메라가 직접 움직이는 것이므로
                // 마우스 드래그 방향과 카메라 이동 방향을 일치시키기 위해 음수(-)를 곱해줍니다.
                Vector3 moveDirection = new Vector3(-delta.x, 0, 0) * (dragSpeed * 0.01f * _battleCam.orthographicSize);
                
                transform.position += moveDirection;
                _lastMousePos = currentMousePos;

                ClampCameraInsideBackground();
            }
        }

        /// <summary>
        /// 카메라가 전투 배경(맵) 바깥으로 벗어나지 못하도록 제한하는 메서드
        /// </summary>
        private void ClampCameraInsideBackground()
        {
            if (battleBackgroundSr == null) return;

            Bounds bgBounds = battleBackgroundSr.bounds;

            // 현재 카메라의 해상도 비율에 따른 시야 크기 계산
            float camHeight = _battleCam.orthographicSize;
            float camWidth = camHeight * _battleCam.aspect;

            Vector3 camPos = transform.position;

            // 카메라가 가둘 수 있는 최소/최대 좌표 범위 계산
            float minX = bgBounds.min.x + camWidth;
            float maxX = bgBounds.max.x - camWidth;
            float minY = bgBounds.min.y + camHeight;
            float maxY = bgBounds.max.y - camHeight;

            // 만약 맵 크기가 카메라 시야보다 작다면 중앙에 고정
            if (minX > maxX) camPos.x = bgBounds.center.x;
            else camPos.x = Mathf.Clamp(camPos.x, minX, maxX);

            if (minY > maxY) camPos.y = bgBounds.center.y;
            else camPos.y = Mathf.Clamp(camPos.y, minY, maxY);

            transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
        }
    }
}