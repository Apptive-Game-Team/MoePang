using _01.Scripts._08.Utility;
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
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI diaText;

    [Header("컴포넌트")] 
    [SerializeField] private GameObject habitatPanel;

    private void Start()
    {
        if (stageText != null) stageText.text = $"Stage : {StageManager.Instance.MaxStage + 1}";
        if (goldText != null) goldText.text = $"{GoldManager.Instance.Gold}";
        if (diaText != null) diaText.text = $"{GoldManager.Instance.Dia}";

        StageManager.Instance.SetStage(StageManager.Instance.MaxStage);

        previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);
    }

    public void OnClickPlay()
    {
        SceneManager.sceneLoaded += OnPlaySceneLoaded;
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
    }

    public void SelectHabitatMode()
    {
        habitatPanel.SetActive(true);
    }

    // 게임 플레이 시 시작돼야 할 사항들
    private void OnPlaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StageManager.Instance.StartStage();
        
        SceneManager.sceneLoaded -= OnPlaySceneLoaded;
    }

    public void OnClickShop()
    {
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Shop));
    }

    /// <summary>
    /// 다음 스테이지 선택
    /// </summary>
    public void OnClickNextStage()
    {
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
        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddStage(-1);
        }
        
        previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);

        stageText.text = $"Stage : {StageManager.Instance.CurrentStage + 1}";
    }
}
