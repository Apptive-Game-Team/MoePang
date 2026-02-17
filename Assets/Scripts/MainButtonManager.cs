using TMPro;
using UnityEngine;

/// <summary>
/// 메인화면 버튼, 텍스트 관리 스크립트
/// </summary>
public class MainButtonManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private StageManager stageManager;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI stageText;


    private void Start()
    {
        stageText.text = $"Stage : {stageManager.CurrentStage}";
    }
    /// <summary>
    /// 다음 스테이지 선택
    /// </summary>
    public void OnClickNextStage()
    {
        if (stageManager != null)
        {
            stageManager.SetStage(1);
        }

        stageText.text = $"Stage : {stageManager.CurrentStage}";
    }

    /// <summary>
    /// 이전 스테이지 선택
    /// </summary>
    public void OnClickPrevStage()
    {
        if (stageManager.CurrentStage == 1) return;

        if (stageManager != null)
        {
            stageManager.SetStage(-1);
        }

        stageText.text = $"Stage : {stageManager.CurrentStage}";
    }
}
