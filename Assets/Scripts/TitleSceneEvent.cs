using TMPro;
using UnityEngine;

public class TitleSceneEvent : MonoBehaviour
{
    [Header("타이틀 텍스트")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float blinkSpeed = 2.0f; // 깜빡임 속도

    void Update()
    {
        float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) * 0.5f;

        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;
    }
}
