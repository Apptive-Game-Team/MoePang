using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts._04.UI.InGame
{
    public class ClearUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;
        private DepthOfField _depthOfField;

        private void Awake()
        {
            if (Camera.main.GetComponent<Volume>().profile.TryGet(out DepthOfField dof))
            {
                _depthOfField = dof;
            }
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Time.timeScale = 0.1f;
            _depthOfField.active = true;
            stageText.text = $"지켜낸 서식지 {StageManager.Instance.CurrentStage}";
            // todo : 시간 설정
            coinText.text = $"{GoldManager.Instance.Gold}";
        }

        private void OnDisable()
        {
            Time.timeScale = 1;
            _depthOfField.active = false;
        }
    }
}
