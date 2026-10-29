using _01.Scripts._00.Manager;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private void Start()
    {
        if (!GameManager.Instance.Istutorial)
            return;
    }
}
