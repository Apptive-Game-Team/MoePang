using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 기존 UI 위에서 다음 대사 클릭을 받되, 종료 버튼에는 입력을 통과시킵니다.
public class TutorialScreenNextInput : Image, IPointerClickHandler
{
    private TutorialManager tutorial;
    private RectTransform endButtonRect;
    private RectTransform actionButtonRect;
    private RectTransform replayButtonRect;

    public void Configure(TutorialManager owner, RectTransform endButton)
    {
        tutorial = owner;
        endButtonRect = endButton;
        color = Color.clear;
        raycastTarget = true;
        // 완전히 투명해도 클릭 영역은 유지합니다.
        canvasRenderer.cullTransparentMesh = false;
    }

    public void SetAllowedButtons(RectTransform actionButton, RectTransform replayButton)
    {
        actionButtonRect = actionButton;
        replayButtonRect = replayButton;
    }

    public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
    {
        if (ContainsScreenPoint(endButtonRect, screenPoint) ||
            ContainsScreenPoint(actionButtonRect, screenPoint) ||
            ContainsScreenPoint(replayButtonRect, screenPoint))
            return false;

        return base.Raycast(screenPoint, eventCamera);
    }

    private static bool ContainsScreenPoint(RectTransform rect, Vector2 point)
    {
        if (rect == null || !rect.gameObject.activeInHierarchy)
            return false;

        Canvas canvas = rect.GetComponentInParent<Canvas>()?.rootCanvas;
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, point, camera);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsActive() && eventData.button == PointerEventData.InputButton.Left &&
            !eventData.dragging && tutorial != null)
            tutorial.NextDialogue();
    }
}
