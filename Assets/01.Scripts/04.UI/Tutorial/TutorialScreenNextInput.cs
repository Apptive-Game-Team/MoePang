using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 기존 UI 위에서 다음 대사 클릭을 받되, 종료 버튼에는 입력을 통과시킵니다.
public class TutorialScreenNextInput : Image, IPointerClickHandler
{
    private TutorialManager tutorial;
    private RectTransform endButtonRect;

    public void Configure(TutorialManager owner, RectTransform endButton)
    {
        tutorial = owner;
        endButtonRect = endButton;
        color = Color.clear;
        raycastTarget = true;
        // 완전히 투명해도 클릭 영역은 유지합니다.
        canvasRenderer.cullTransparentMesh = false;
    }

    public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
    {
        if (endButtonRect != null && endButtonRect.gameObject.activeInHierarchy)
        {
            Canvas endCanvas = endButtonRect.GetComponentInParent<Canvas>();
            Canvas rootCanvas = endCanvas != null ? endCanvas.rootCanvas : null;
            Camera endCamera = rootCanvas != null &&
                               rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? rootCanvas.worldCamera
                : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                    endButtonRect, screenPoint, endCamera))
                return false;
        }

        return base.Raycast(screenPoint, eventCamera);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsActive() && eventData.button == PointerEventData.InputButton.Left &&
            !eventData.dragging && tutorial != null)
            tutorial.NextDialogue();
    }
}
