using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._00.Manager
{
    /// <summary>
    /// Scene 공통 캔버스 관리 매니저
    /// </summary>
    public class CommonCanvasManager : MonoBehaviour
    {
        public static CommonCanvasManager Instance { get; private set; }

        [Header("Text Setting")]
        private TextMeshProUGUI goldText;
        private TextMeshProUGUI diaText;

        [Header("Button Settings")] 
        private Button _optionButton;
        private Button _unitInfoButton;
        private Button _comboButton;
        private Button _habitatModeButton;
        private Button _shopButton;
        

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            FindTextObjects();
            SetButtons();
            UpdateButtons();
        }

        private void Start()
        {
            UpdateUI();
        }

        private void OnEnable()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged += UpdateUI;
                GoldManager.Instance.OnDiaChanged += UpdateUI;
            }
        }

        private void OnDisable()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= UpdateUI;
                GoldManager.Instance.OnDiaChanged -= UpdateUI;
            }
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

        private void SetButtons()
        {
            Transform buttons = transform.GetChild(2);
            
            _optionButton = buttons.Find("OptionButton").GetComponent<Button>();
            _unitInfoButton = buttons.Find("UnitInfoButton").GetComponent<Button>();
            _comboButton = buttons.Find("ComboButton").GetComponent<Button>();
            _habitatModeButton = buttons.Find("HabitatModeButton").GetComponent<Button>();
            _shopButton = buttons.Find("ReShopButton").GetComponent<Button>();
        }

        private void UpdateButtons()
        {
            _comboButton.interactable = GameManager.Instance.playData.MaxStages.Any(stage => stage.Value >= 5);
        }
    }
}
