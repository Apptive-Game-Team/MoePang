using _01.Scripts._00.Manager;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup timeInfo;
    [SerializeField] private CanvasGroup lockButton;
    [SerializeField] private CanvasGroup tutorialPanel;
    
    [SerializeField] private TutorialText tutorialText;
    
    private void Start()
    {
        if (!GameManager.Instance.Istutorial)
            return;

        HideUi();
        ShowTutorialPanel();
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
