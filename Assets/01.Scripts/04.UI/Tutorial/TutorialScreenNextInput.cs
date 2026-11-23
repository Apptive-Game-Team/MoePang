using System.Collections.Generic;
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
    private readonly List<ScrollRect> blockedScrollRects = new();

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
        if (actionButtonRect != actionButton)
        {
            RestoreScrolling();
            actionButtonRect = actionButton;
            if (IsActive())
                BlockActionButtonScrolling();
        }
        replayButtonRect = replayButton;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BlockActionButtonScrolling();
    }

    protected override void OnDisable()
    {
        RestoreScrolling();
        base.OnDisable();
    }

    private void BlockActionButtonScrolling()
    {
        if (actionButtonRect == null)
            return;

        // 버튼은 클릭을 받되, 부모 ScrollRect가 드래그와 관성 이동을 처리하지 못하게 합니다.
        foreach (ScrollRect scrollRect in actionButtonRect.GetComponentsInParent<ScrollRect>(true))
        {
            if (!scrollRect.enabled)
                continue;

            scrollRect.StopMovement();
            scrollRect.enabled = false;
            blockedScrollRects.Add(scrollRect);
        }
    }

    private void RestoreScrolling()
    {
        foreach (ScrollRect scrollRect in blockedScrollRects)
        {
            if (scrollRect != null)
                scrollRect.enabled = true;
        }
        blockedScrollRects.Clear();
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
