using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Camera battleCam; // 본인 카메라(Battle Camera)

    [Header("조작 영역 설정 (0~1 사이 값)")]
    [Range(0, 1)][SerializeField] private float viewYMin = 0.5f; // 상단 절반이라면 0.5
    [Range(0, 1)][SerializeField] private float viewYMax = 1.0f;

    [Header("움직임 스탯")]
    [SerializeField] private float dragSpeed = 0.01f;
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;

    [Header("좌우 제한 (배틀 필드 크기에 맞춰 설정)")]
    [SerializeField] private float leftLimit = -10f;
    [SerializeField] private float rightLimit = 10f;

    private Vector2 lastTouch;
    private bool isInputValid;

    void Start()
    {
        if (battleCam == null) battleCam = GetComponent<Camera>();

        // 1. 카메라가 상단만 찍도록 설정 (Viewport Rect)
        // X:0, Y:0.5, W:1, H:0.5 (하단 퍼즐 0.5 제외하고 상단만 점유)
        battleCam.rect = new Rect(0, viewYMin, 1, viewYMax - viewYMin);
    }

    void Update()
    {
        HandleInputCheck();

        if (isInputValid)
        {
            HandleDrag();
            HandleZoom();
        }

        ClampCamera();

        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
            isInputValid = false;
    }

    private void HandleInputCheck()
    {
        // 터치나 클릭이 '상단 배틀 구역' 안에서 시작되었는지만 체크
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            isInputValid = IsInBattleZone(Input.GetTouch(0).position);
        else if (Input.GetMouseButtonDown(0))
            isInputValid = IsInBattleZone(Input.mousePosition);
    }

    private bool IsInBattleZone(Vector2 screenPos)
    {
        // 화면 좌표를 0~1 사이의 뷰포트 좌표로 변환
        Vector3 vp = battleCam.ScreenToViewportPoint(screenPos);

        // 중요: 배틀 카메라의 실제 Viewport Rect 범위 안에 있는지 확인
        // 만약 카메라 Rect를 Y:0.5로 잡았다면 vp.y는 0~1 사이로 환산되어서 나옵니다.
        // 하지만 전체 화면 기준으로 판단하기 위해 아래와 같이 체크합니다.
        float normalizedY = screenPos.y / Screen.height;
        return normalizedY >= viewYMin && normalizedY <= viewYMax;
    }

    void HandleDrag()
    {
        // 모바일 터치 드래그
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) lastTouch = touch.position;
            if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = (touch.position.x - lastTouch.x) * dragSpeed * (battleCam.orthographicSize / 10f);
                transform.position += new Vector3(-deltaX, 0, 0);
                lastTouch = touch.position;
            }
        }

        // 에디터 마우스 드래그
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) lastTouch = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            float deltaX = (Input.mousePosition.x - lastTouch.x) * dragSpeed * (battleCam.orthographicSize / 10f);
            transform.position += new Vector3(-deltaX, 0, 0);
            lastTouch = Input.mousePosition;
        }
#endif
    }

    void HandleZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevMag = (prev0 - prev1).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;

            battleCam.orthographicSize = Mathf.Clamp(battleCam.orthographicSize - (currentMag - prevMag) * zoomSpeed, minZoom, maxZoom);
        }

        // 마우스 휠
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f && IsInBattleZone(Input.mousePosition))
        {
            battleCam.orthographicSize = Mathf.Clamp(battleCam.orthographicSize - scroll * 5f, minZoom, maxZoom);
        }
    }

    void ClampCamera()
    {
        // 좌우 이동 제한 (배틀 필드 밖으로 못 나가게)
        float clampedX = Mathf.Clamp(transform.position.x, leftLimit, rightLimit);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}