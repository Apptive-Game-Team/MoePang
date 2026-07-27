using _01.Scripts._08.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬 이벤트 담당 스크립트
/// </summary>
public class TitleSceneEvent : MonoBehaviour
{
    [Header("타이틀 텍스트")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float blinkSpeed = 2.0f; // 깜빡임 속도

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) * 0.5f;
        textComponent.color = Color.Lerp(Color.black, Color.gray, t);

        if (Input.GetMouseButtonUp(0))
        {
            MoveToNextScene();
        }
    }

    private void MoveToNextScene()
    {
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
    }
}
