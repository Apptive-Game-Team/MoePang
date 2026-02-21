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

    [Header("이동할 씬")]
    [SerializeField] private string nextSceneName;

    void Update()
    {
        float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) * 0.5f;

        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;

        if (Input.GetMouseButtonDown(0))
        {
            MoveToNextScene();
        }
    }

    private void MoveToNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
