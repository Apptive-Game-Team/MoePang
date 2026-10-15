using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using _01.Scripts._12.Backend;
using DG.Tweening;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleSceneEvent : MonoBehaviour
{
    [Header("Title Objects")]
    [SerializeField] private RectTransform tiger;
    [SerializeField] private RectTransform title;
    [FormerlySerializedAs("textComponent")]
    [SerializeField] private TextMeshProUGUI touchToScreen;

    [Header("Intro Animation")]
    [SerializeField] private float introDelay = 1f;
    [SerializeField] private float hiddenOffsetY = 900f;
    [SerializeField] private float tigerMoveDuration = 1f;
    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private float titleDropDuration = 0.45f;
    [SerializeField] private float titleBounceHeight = 120f;
    [SerializeField] private float touchBlinkDuration = 0.6f;

    [Header("Login UI")] 
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button localPlayButton;
    [SerializeField] private Button editorPlayButton;
    [SerializeField] private Image localPlayUI;
    [SerializeField] private Button localPlayYesButton;
    [SerializeField] private Button localPlayNoButton;

    private Vector2 tigerTargetPosition;
    private Vector2 titleTargetPosition;
    private CanvasGroup tigerCanvasGroup;
    private CanvasGroup titleCanvasGroup;
    private Sequence introSequence;
    private Tween touchBlinkTween;
    private bool canStartGame;
    private int _loginType;
    private bool _isLoggingIn;

    private void Awake()
    {
        Application.targetFrameRate = 65;
        
        _loginType = PlayerPrefs.GetInt("LoginType", 0);
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
        introSequence.AppendCallback(ExecuteLoginFlow);
    }

    private async void ExecuteLoginFlow()
    {
        if (_loginType == 0)
        {
            AddButtonListeners();
            loginPanel.gameObject.SetActive(true);
        }
        else
        {
            try
            {
                if (await GoogleSignInTest.Instance.ExecuteGoogleLoginFlow())
                {
                    EnableTouchToScreen();
                }
            }
            catch (Exception)
            {
                AddButtonListeners();
                loginPanel.gameObject.SetActive(true);
            }
        }
    }

    private void AddButtonListeners()
    {
#if UNITY_EDITOR
        editorPlayButton.onClick.AddListener(async () =>
        {
            if (_isLoggingIn)
            {
                return;
            }
            _isLoggingIn = true;
                
            await SupabaseLoginManager.Instance.StartGame();
            EnableTouchToScreen();
        });
        editorPlayButton.gameObject.SetActive(true);
#endif
        
        googleLoginButton.onClick.AddListener(async () =>
        {
            if (_isLoggingIn)
            {
                return;
            }
            _isLoggingIn = true;

            try
            {
                bool success = await GoogleSignInTest.Instance.ExecuteGoogleLoginFlow();

                if (!success)
                {
                    return;
                }

                PlayerPrefs.SetInt("LoginType", 1);
                PlayerPrefs.Save();

                EnableTouchToScreen();
            }
            finally
            {
                _isLoggingIn = false;
            }
        });
        
        localPlayButton.onClick.AddListener(async () =>
        {
            if (_isLoggingIn)
            {
                return;
            }
            _isLoggingIn = true;
            
            localPlayUI.gameObject.SetActive(true);
        });
        
        localPlayYesButton.onClick.AddListener(async () =>
        {
            GameManager.Instance.isLoggedIn = false;
            await GameManager.Instance.LoadData();
            await Task.Yield();
            EnableTouchToScreen();
        });
        localPlayNoButton.onClick.AddListener(() =>
        {
            _isLoggingIn = false;
            localPlayUI.gameObject.SetActive(false);
        });
    }

    private void EnableTouchToScreen()
    {
        if (loginPanel.activeInHierarchy)
        {
            loginPanel.gameObject.SetActive(false);
        }
        
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
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
    }
}
