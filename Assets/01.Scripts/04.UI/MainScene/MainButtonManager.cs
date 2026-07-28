using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using _01.Scripts._11.HabitatMode;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인화면 버튼, 텍스트 관리 스크립트
/// </summary>
public class MainButtonManager : MonoBehaviour
{
    [Header("버튼")] 
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI stageText;

    private void Start()
    {
        if (stageText != null) stageText.text = $"Stage : {StageManager.Instance.CurrentStage + 1}";
        if (previousButton != null) previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        if (nextButton != null) nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);
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
        
        previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);

        stageText.text = $"Stage : {StageManager.Instance.CurrentStage + 1}";
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
        
        previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);

        stageText.text = $"Stage : {StageManager.Instance.CurrentStage + 1}";
    }
}
