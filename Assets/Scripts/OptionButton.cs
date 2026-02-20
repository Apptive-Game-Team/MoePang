using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    public TextMeshProUGUI test; //테스트
    private GameObject optionWindow;
    public void OpenOptions()
    {
        if (optionWindow != null) return;
        SoundManager.Instance.PlaySFX(SFX.SFX0_Default);
        optionWindow = Instantiate(Resources.Load<GameObject>("UI/UI_Option"));
        test.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Exit");

        if (this.gameObject != null)
        {
            //this.gameObject.transform.DOPunchScale(new Vector3(0.35f, 0.7f, 1f) * -0.2f, 0.2f).SetEase(Ease.InBack); dotween 임포트 필요
            this.gameObject.GetComponent<Button>().interactable = false;
            optionWindow.GetComponent<OptionWindow>().optionButton = this.gameObject.GetComponent<Button>();
        }

        optionWindow.GetComponent<OptionWindow>().Init();
    }
}
