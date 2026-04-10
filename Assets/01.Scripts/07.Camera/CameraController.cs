using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;

    [Header("Input Area")]
    [SerializeField] private RectTransform scrollArea; // [중요] 방금 만든 CameraArea 등록

    [Header("Drag & Zoom 스탯은 기존과 동일...")]
    [SerializeField] private float dragSpeed = 0.01f;
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;

    private float calculatedMaxX;

    private Vector2 lastTouch;
    private bool isInputValid; // 영역 내에서 시작되었는지 확인

    void Start()
    {
        // 시작할 때 카메라를 가장 크게 확대된 상태(minZoom)로 설정합니다.
        if (cam != null)
        {
            cam.orthographicSize = maxZoom;
        }
    }

    void Update()
    {
        // 1. 입력이 유효한 영역(상단)에서 시작되었는지 체크
        CheckInputStart();

        if (isInputValid)
        {
            HandleDrag();
            HandleZoom();
        }

        ClampCamera();

        // 손을 떼면 유효성 초기화
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
            isInputValid = false;
    }

    private void CheckInputStart()
    {
        // 터치 시작 시점에 영역 확인
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            isInputValid = IsPointerInScrollArea(Input.GetTouch(0).position);
        }
        // 마우스 클릭 시점에 영역 확인
        else if (Input.GetMouseButtonDown(0))
        {
            isInputValid = IsPointerInScrollArea(Input.mousePosition);
        }
    }

    private bool IsPointerInScrollArea(Vector2 screenPos)
    {
        if (scrollArea == null) return true;

        return RectTransformUtility.RectangleContainsScreenPoint(
            scrollArea,
            screenPos,
            cam
        );
    }

    void HandleDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) lastTouch = touch.position;

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastTouch;
                transform.position += new Vector3(-delta.x * dragSpeed, 0, 0);
                lastTouch = touch.position;
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) lastTouch = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouch;
            transform.position += new Vector3(-delta.x * dragSpeed, 0, 0);
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
            float diff = currentMag - prevMag;

            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - diff * zoomSpeed, minZoom, maxZoom);
        }

        // 마우스 휠 (마우스가 상단 영역 위에 있을 때만)
        if (IsPointerInScrollArea(Input.mousePosition))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * 5f, minZoom, maxZoom);
            }
        }
    }

    void ClampCamera()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float currentHalfWidth = cam.orthographicSize * aspectRatio;
        float maxHalfWidth = maxZoom * aspectRatio;

        calculatedMaxX = Mathf.Max(0, maxHalfWidth - currentHalfWidth);

        float clampedX = Mathf.Clamp(transform.position.x, -calculatedMaxX, calculatedMaxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}