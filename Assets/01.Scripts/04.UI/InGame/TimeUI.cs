using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class TimeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
        
        private void OnEnable()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnTimeChanged += UpdateTimerText;
            }
        }

        private void OnDisable()
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnTimeChanged -= UpdateTimerText;
            }
        }

        private void UpdateTimerText(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = $"{minutes}:{seconds:00}";
        }
    }
}
