using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Camera battleCam;

    [Header("조작 영역 설정 (전체 화면 대비 비율)")]
    [Range(0, 1)][SerializeField] private float viewYMin = 0.5f;
    [Range(0, 1)][SerializeField] private float viewYMax = 1.0f;

    [Header("움직임 스탯")]
    [SerializeField] private float dragSpeed = 0.5f; // 감도 조절
    [SerializeField] private float zoomSpeed = 0.05f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;

    [Header("전투 구역 한계 좌표 (World Space)")]
    [SerializeField] private float worldLeftLimit = -20f;
    [SerializeField] private float worldRightLimit = 20f;

    private Vector2 lastTouch;
    private bool isInputValid;

    void Start()
    {
        if (battleCam == null) battleCam = GetComponent<Camera>();

        // URP Overlay 카메라는 Base 카메라의 Rect를 따르므로 
        // 렌더링 영역 자체를 제한하려면 UI Mask나 별도의 설정을 권장하지만,
        // 일단 로직상으로는 입력 영역만 제한해도 충분합니다.
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

        // 입력 종료 시 플래그 초기화
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
            isInputValid = false;
    }

    private void HandleInputCheck()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
                isInputValid = IsInBattleZone(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            isInputValid = IsInBattleZone(Input.mousePosition);
        }
    }

    private bool IsInBattleZone(Vector2 screenPos)
    {
        // 화면 하단(0)에서 상단(1)까지의 비율 계산
        float normalizedY = screenPos.y / Screen.height;
        return normalizedY >= viewYMin && normalizedY <= viewYMax;
    }

    void HandleDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                // 화면 이동 비율에 맞춰 월드 좌표 이동
                float moveX = touch.deltaPosition.x * dragSpeed * (battleCam.orthographicSize / 10f) * Time.unscaledDeltaTime * -100f;
                transform.Translate(new Vector3(moveX, 0, 0));
            }
        }
#if UNITY_EDITOR
        else if (Input.GetMouseButton(0))
        {
            float moveX = Input.GetAxis("Mouse X") * dragSpeed * (battleCam.orthographicSize / 10f) * -50f;
            transform.Translate(new Vector3(moveX, 0, 0));
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

            float delta = (currentMag - prevMag) * zoomSpeed;
            battleCam.orthographicSize = Mathf.Clamp(battleCam.orthographicSize - delta, minZoom, maxZoom);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f && IsInBattleZone(Input.mousePosition))
        {
            battleCam.orthographicSize = Mathf.Clamp(battleCam.orthographicSize - scroll * 10f, minZoom, maxZoom);
        }
    }

    void ClampCamera()
    {
        // 중요: 카메라의 가로 반 너비(Half Width)를 계산하여 배경 밖이 보이지 않게 함
        float camHalfWidth = battleCam.orthographicSize * battleCam.aspect;

        float minX = worldLeftLimit + camHalfWidth;
        float maxX = worldRightLimit - camHalfWidth;

        // 만약 카메라가 배경보다 크다면 중앙 고정
        if (minX > maxX)
        {
            transform.position = new Vector3((worldLeftLimit + worldRightLimit) / 2f, transform.position.y, transform.position.z);
        }
        else
        {
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }
    }
}