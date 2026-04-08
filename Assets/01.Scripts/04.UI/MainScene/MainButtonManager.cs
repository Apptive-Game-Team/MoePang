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

    [Header("씬")]
    [SerializeField] private string playScene;
    [SerializeField] private string shopScene;


    private void Start()
    {
        stageText.text = $"Stage : {StageManager.Instance.CurrentStage + 1}";
        goldText.text = $"Gold : {GoldManager.Instance.Gold}";

        previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);
    }

    public void OnClickPlay()
    {
        SceneManager.sceneLoaded += OnPlaySceneLoaded;
        SceneManager.LoadScene(playScene);
    }

    // 게임 플레이 시 시작돼야 할 사항들
    private void OnPlaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StageManager.Instance.StartStage();
        
        SceneManager.sceneLoaded -= OnPlaySceneLoaded;
    }

    public void OnClickShop()
    {
        SceneManager.LoadScene(shopScene);
    }

    /// <summary>
    /// 다음 스테이지 선택
    /// </summary>
    public void OnClickNextStage()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetStage(1);
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
            StageManager.Instance.SetStage(-1);
        }
        
        previousButton.SetActive(StageManager.Instance.CurrentStage > 0);
        nextButton.SetActive(StageManager.Instance.CurrentStage < StageManager.Instance.MaxStage);

        stageText.text = $"Stage : {StageManager.Instance.CurrentStage + 1}";
    }
}
