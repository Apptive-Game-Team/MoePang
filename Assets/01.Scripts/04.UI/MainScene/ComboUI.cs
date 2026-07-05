using _01.Scripts._00.Manager;
using _01.Scripts._10.System.Combo;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace _01.Scripts._04.UI.MainScene
{
    public class ComboUI : MonoBehaviour
    {
        [SerializeField] private Button comboOrderButton;
        [SerializeField] private GameObject comboUIPrefab;
        [SerializeField] private GameObject content;
        [SerializeField] private GameObject upgradeUI;
        [SerializeField] private List<Combo> combos;

        private bool _isComboOrdering;
        
        private void Awake()
        {
            InitialSetting();
        }

        private void InitialSetting()
        {
            // 콤보 버튼 UI 설정
            CanvasGroup scrollRectCG = content.transform.parent.parent.GetComponent<CanvasGroup>();
            ScrollRect scrollRect = content.transform.parent.parent.GetComponent<ScrollRect>();
            
            foreach (var (type,idx) in GameManager.Instance.comboData.comboSequence.Select((value, index) => (value, index)))
            {
                Combo combo = combos.Find(c => c.info.comboType == type);
                GameObject comboUI = Instantiate(comboUIPrefab, content.transform);
                ComboUIObject obj = comboUI.GetComponent<ComboUIObject>();
                var comboLevels = GameManager.Instance.comboData.comboLevels;
                
                obj.Initialize(this);
                obj.habitat = type;
                
                TextMeshProUGUI comboOrder = comboUI.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
                Image comboImage = comboUI.transform.GetChild(2).GetComponent<Image>();
                TextMeshProUGUI comboLevel = comboUI.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI comboDescription = comboUI.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
                Button comboUpgradeButton =  comboUI.transform.GetChild(5).GetComponent<Button>();
                TextMeshProUGUI upgradeText = comboUpgradeButton.GetComponentInChildren<TextMeshProUGUI>();
                
                comboOrder.text = (idx + 1).ToString();
                comboImage.sprite = combo.info.comboImage;
                comboLevel.text = $"LV{comboLevels[type]}";
                comboDescription.text = combo.DynamicDescription();
                if (comboLevels[type] == combo.info.ComboMaxLevel)
                {
                    comboUpgradeButton.interactable = false;
                    upgradeText.text = "Level Max";
                }
                comboUpgradeButton.onClick.AddListener(() =>
                {
                    if (comboLevels[type] < combo.info.ComboMaxLevel)
                    {
                        if (!GoldManager.Instance.TrySpendDia(comboLevels[type] * 100))
                        {
                            return;
                        }
                        SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
                        upgradeUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                            $"{comboLevels[type] * 100}D\nLevel {type.ToString()} 콤보를 업그레이드 하시겠습니까?";
                        upgradeUI.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
                        upgradeUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GoldManager.Instance.AdjustDia(-comboLevels[type] * 100);
                            combo.UpgradeCombo();
                            GameManager.Instance.SaveGoldData();
                            GameManager.Instance.SaveComboData();
                            upgradeUI.SetActive(false);
                            
                            comboLevel.text = $"LV{comboLevels[type]}";
                            comboDescription.text = combo.DynamicDescription();
                        
                            if (comboLevels[type] == combo.info.ComboMaxLevel)
                            {
                                comboUpgradeButton.interactable = false;
                                upgradeText.text = "Level Max";
                            }
                            
                            scrollRectCG.interactable = true;
                            scrollRectCG.blocksRaycasts = true;
                            scrollRect.enabled = true;
                        });
                        
                        scrollRect.enabled = false;
                        scrollRectCG.interactable = false;
                        scrollRectCG.blocksRaycasts = false;
                        upgradeUI.SetActive(true);
                    }
                });
            }
            
            upgradeUI.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                upgradeUI.SetActive(false);
                scrollRectCG.interactable = true;
                scrollRectCG.blocksRaycasts = true;
                scrollRect.enabled = true;
            });
            
            // 콤보 정렬 버튼 설정
            comboOrderButton.onClick.AddListener(() =>
            {
                _isComboOrdering = !_isComboOrdering;
                
                SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
                comboOrderButton.GetComponentInChildren<TextMeshProUGUI>().text = _isComboOrdering ? "콤보 정렬 완료" : "콤보 정렬";
                foreach (ComboUIObject ui in content.transform.GetComponentsInChildren<ComboUIObject>())
                {
                    ui.transform.GetChild(5).GetComponent<Button>().interactable = !_isComboOrdering;
                    ui.transform.GetChild(1).gameObject.SetActive(_isComboOrdering);
                }

                if (!_isComboOrdering)
                {
                    GameManager.Instance.SaveComboData();
                }
            });
        }
        
        public void OnOrderChanged()
        {
            List<ComboUIObject> objs = content.transform.GetComponentsInChildren<ComboUIObject>().ToList();
            List<Habitat> list = objs.ConvertAll(c => c.habitat);

            for (int i = 0; i < list.Count; i++)
            {
                GameManager.Instance.comboData.comboSequence[i] = list[i];
                objs[i].gameObject.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            }
        }
    }
}
