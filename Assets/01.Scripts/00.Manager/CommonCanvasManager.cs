using TMPro;
using UnityEngine;

namespace _01.Scripts._00.Manager
{
    /// <summary>
    /// Scene 공통 캔버스 관리 매니저
    /// </summary>
    public class CommonCanvasManager : MonoBehaviour
    {
        [Header("Text Setting")]
        private TextMeshProUGUI goldText;
        private TextMeshProUGUI diaText;

        private void Awake()
        {
            FindTextObjects();
        }

        private void Start()
        {
            UpdateUI();
        }

        /// <summary>
        /// GoldText / DiaText 오브젝트 찾아서 연결
        /// </summary>
        private void FindTextObjects()
        {
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var textComp in allTexts)
            {
                if (textComp.gameObject.name == "GoldCardText")
                {
                    goldText = textComp;
                }
                else if (textComp.gameObject.name == "DiaCardText")
                {
                    diaText = textComp;
                }
            }

            if (goldText == null) Debug.LogWarning("[CommonCanvasManager] 'GoldCardText' 이름을 가진 오브젝트를 찾을 수 없습니다!");
            if (diaText == null) Debug.LogWarning("[CommonCanvasManager] 'DiaCardText' 이름을 가진 오브젝트를 찾을 수 없습니다!");
        }

        /// <summary>
        /// 텍스트 데이터 갱신
        /// </summary>
        public void UpdateUI()
        {
            if (goldText != null) goldText.text = $"{GoldManager.Instance.Gold}";
            if (diaText != null) diaText.text = $"{GoldManager.Instance.Dia}";
        }
    }
}
