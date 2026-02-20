using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인화면 버튼, 텍스트 관리 스크립트
/// </summary>
public class MainButtonManager : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI goldtext;

    [Header("씬")]
    [SerializeField] private string playScene;
    [SerializeField] private string shopScene;


    private void Start()
    {
        stageText.text = $"Stage : {StageManager.Instance.CurrentStage}";
        goldtext.text = $"Gold : {GoldManager.Instance.Gold}";
    }

    public void OnClickPlay()
    {
        SceneManager.LoadScene(playScene);
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

        stageText.text = $"Stage : {StageManager.Instance.CurrentStage}";
    }

    /// <summary>
    /// 이전 스테이지 선택
    /// </summary>
    public void OnClickPrevStage()
    {
        if (StageManager.Instance.CurrentStage == 1) return;

        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetStage(-1);
        }

        stageText.text = $"Stage : {StageManager.Instance.CurrentStage}";
    }
}
