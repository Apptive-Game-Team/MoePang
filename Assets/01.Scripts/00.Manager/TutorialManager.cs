using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup timeInfo;
    [SerializeField] private CanvasGroup lockButton;
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private CanvasGroup underTutorialPanel;
    
    [SerializeField] private TutorialText tutorialTextList;
    [SerializeField] private TextMeshProUGUI tutorialText;

    private int textIndex = 0;
    
    private void Start()
    {
        if (!GameManager.Instance.Istutorial)
            return;

        textIndex = 0;

        HideUi();
        ShowTutorialPanel();
        ShowMessage();
    }

    private void ShowMessage()
    {
        if (tutorialTextList == null || tutorialText == null)
        {
            Debug.LogError("TutorialManager: Assign the tutorial text list and text UI.", this);
            return;
        }

        if (textIndex < 0 || textIndex >= tutorialTextList.Count)
            return;

        tutorialText.text = tutorialTextList.GetDialogue(textIndex);
    }

    private void HideUi()
    {
        SetUi(timeInfo, 0f, false);
        SetUi(lockButton, 0f, false);
    }

    private void ShowTutorialPanel()
    {
        SetUi(tutorialPanel, 1f);
    }

    private void SetUi(CanvasGroup group, float alpha, bool interactable = false)
    {
        group.alpha = alpha;
        group.interactable = interactable;
    }
}
