using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(fileName = "TutorialText", menuName = "Scriptable Objects/TutorialText")]
public class TutorialText : ScriptableObject
{
    [SerializeField] private string tableName = "LocalizationDataTable";

    [Tooltip("Enter localization keys in order: Tutorial00, Tutorial01, ... Tutorial14.")]
    [SerializeField]
    private List<string> dialogues = new List<string>();

    public IReadOnlyList<string> Dialogues => dialogues;
    public int Count => dialogues.Count;

    /// <summary>Returns the dialogue in the currently selected language. Indices start at 0.</summary>
    public string GetDialogue(int index)
    {
        if (index < 0 || index >= dialogues.Count)
        {
            Debug.LogWarning($"TutorialText: Dialogue index {index} is out of range.", this);
            return string.Empty;
        }

        string key = dialogues[index];
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning($"TutorialText: Missing localization key at index {index}.", this);
            return string.Empty;
        }

        return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
    }
}
