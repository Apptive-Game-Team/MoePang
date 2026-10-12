using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleSceneEvent : MonoBehaviour
{
    [Header("Title Objects")]
    [SerializeField] private RectTransform tiger;
    [SerializeField] private RectTransform title;
    [FormerlySerializedAs("textComponent")]
    [SerializeField] private TextMeshProUGUI touchToScreen;
    [SerializeField] private Image loginImage;

    [Header("Intro Animation")]
    [SerializeField] private float introDelay = 1f;
    [SerializeField] private float hiddenOffsetY = 900f;
    [SerializeField] private float tigerMoveDuration = 1f;
    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private float titleDropDuration = 0.45f;
    [SerializeField] private float titleBounceHeight = 120f;
    [SerializeField] private float touchBlinkDuration = 0.6f;

    private Vector2 tigerTargetPosition;
    private Vector2 titleTargetPosition;
    private CanvasGroup tigerCanvasGroup;
    private CanvasGroup titleCanvasGroup;
    private Sequence introSequence;
    private Tween touchBlinkTween;
    private bool canStartGame;

    private void Awake()
    {
        Application.targetFrameRate = 65;
    }

    private void Start()
    {
        InitializeIntroObjects();
        PlayIntroAnimation();
    }

    private void Update()
    {
        if (canStartGame && Input.GetMouseButtonUp(0))
        {
            MoveToNextScene();
        }
    }

    private void OnDestroy()
    {
        introSequence?.Kill();
        touchBlinkTween?.Kill();
    }

    private void InitializeIntroObjects()
    {
        canStartGame = false;

        if (loginImage != null)
        {
            loginImage.gameObject.SetActive(false);
        }

        if (tiger != null)
        {
            tigerTargetPosition = tiger.anchoredPosition;
            tiger.anchoredPosition = tigerTargetPosition + Vector2.down * hiddenOffsetY;
            tigerCanvasGroup = GetOrAddCanvasGroup(tiger.gameObject);
            tigerCanvasGroup.alpha = 0f;
        }

        if (title != null)
        {
            titleTargetPosition = title.anchoredPosition;
            title.anchoredPosition = titleTargetPosition + Vector2.up * hiddenOffsetY;
            titleCanvasGroup = GetOrAddCanvasGroup(title.gameObject);
            titleCanvasGroup.alpha = 0f;
        }

        if (touchToScreen != null)
        {
            touchToScreen.gameObject.SetActive(false);
        }
    }

    private void PlayIntroAnimation()
    {
        introSequence?.Kill();
        introSequence = DOTween.Sequence();
        introSequence.AppendInterval(introDelay);

        if (tiger != null)
        {
            introSequence.AppendCallback(() => tigerCanvasGroup.alpha = 1f);
            introSequence.Append(tiger.DOAnchorPos(tigerTargetPosition, tigerMoveDuration).SetEase(Ease.OutCubic));
        }

        introSequence.AppendInterval(stepDelay);

        if (title != null)
        {
            introSequence.AppendCallback(() => titleCanvasGroup.alpha = 1f);
            introSequence.Append(title.DOAnchorPos(titleTargetPosition, titleDropDuration).SetEase(Ease.InQuad));
            introSequence.Append(title.DOAnchorPosY(titleTargetPosition.y + titleBounceHeight, 0.18f).SetEase(Ease.OutQuad));
            introSequence.Append(title.DOAnchorPosY(titleTargetPosition.y, 0.16f).SetEase(Ease.InQuad));
            introSequence.Append(title.DOAnchorPosY(titleTargetPosition.y + titleBounceHeight * 0.45f, 0.13f).SetEase(Ease.OutQuad));
            introSequence.Append(title.DOAnchorPosY(titleTargetPosition.y, 0.12f).SetEase(Ease.InQuad));
            introSequence.Append(title.DOAnchorPosY(titleTargetPosition.y + titleBounceHeight * 0.18f, 0.09f).SetEase(Ease.OutQuad));
            introSequence.Append(title.DOAnchorPosY(titleTargetPosition.y, 0.08f).SetEase(Ease.InQuad));
        }

        introSequence.AppendInterval(stepDelay);
        introSequence.AppendCallback(EnableTouchToScreen);
    }

    private void EnableTouchToScreen()
    {
        if (touchToScreen != null)
        {
            touchToScreen.gameObject.SetActive(true);
            Color color = touchToScreen.color;
            color.a = 1f;
            touchToScreen.color = color;
            touchBlinkTween = touchToScreen.DOFade(0.25f, touchBlinkDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        canStartGame = true;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (!target.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private void MoveToNextScene()
    {
        if (loginImage != null)
        {
            loginImage.gameObject.SetActive(true);
        }
    }
}
