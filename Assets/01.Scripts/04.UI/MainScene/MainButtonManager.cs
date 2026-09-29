using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using _01.Scripts._11.HabitatMode;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

/// <summary>
/// 메인화면 버튼, 텍스트 관리 스크립트
/// </summary>
public class MainButtonManager : MonoBehaviour
{
    [Header("버튼")] 
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;

    [Header("화면 이미지")] 
    [SerializeField] private Image stageImage;
    [SerializeField] private List<Sprite> diorama = new List<Sprite>();

    [Header("텍스트")] 
    [SerializeField] private TextMeshProUGUI chapterText;
    [SerializeField] private TextMeshProUGUI stageText;

    private void Start()
    {
        RefreshStageUI();
    }

    public void OnClickPlay()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        HabitatModeManager.Instance.IsHabitatBattle = false;
        SceneManager.sceneLoaded += OnPlaySceneLoaded;
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
    }

    // 게임 플레이 시 시작돼야 할 사항들
    private void OnPlaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StageManager.Instance.StartStage();
        GameManager.Instance.PlayBattleBGM();
        
        SceneManager.sceneLoaded -= OnPlaySceneLoaded;
    }

    public void OnClickShop()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Shop));
    }

    /// <summary>
    /// 다음 스테이지 선택
    /// </summary>
    public void OnClickNextStage()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddStage(1);
        }
        
        RefreshStageUI();
    }

    /// <summary>
    /// 이전 스테이지 선택
    /// </summary>
    public void OnClickPrevStage()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddStage(-1);
        }
        
        RefreshStageUI();
    }

    private void RefreshStageUI()
    {
        if (stageText != null)
        {
            stageText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Stage")} : {StageManager.Instance.CurrentStage + 1}";
        }

        if (chapterText != null)
        {
            chapterText.text = GetChapterName(StageManager.Instance.CurrentStage);
        }

        if (previousButton != null)
        {
            previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);
        }

        SetDioramaImage();
    }

    private string GetChapterName(int stage)
    {
        return ((stage / 10) % 5) switch
        {
            0 => "Meadow",
            1 => "Ocean",
            2 => "Desert",
            3 => "Forest",
            4 => "Polar",
            _ => "Meadow"
        };
    }

    private void SetDioramaImage()
    {
        if (stageImage == null || diorama == null || diorama.Count == 0)
        {
            return;
        }

        int index = StageManager.Instance.CurrentStage % diorama.Count;
        stageImage.sprite = diorama[index];
    }
}
