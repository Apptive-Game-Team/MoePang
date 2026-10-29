using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialText", menuName = "Scriptable Objects/TutorialText")]
public class TutorialText : ScriptableObject
{
    [SerializeField, TextArea(3, 10)]
    private List<string> dialogues = new List<string>();

    public IReadOnlyList<string> Dialogues => dialogues;
}
