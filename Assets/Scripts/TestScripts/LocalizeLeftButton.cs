using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizeLeftButton : MonoBehaviour
{
    private int currentLocaleIndex = 0;
    private void Start()
    {
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[i])
            {
                currentLocaleIndex = i;
                break;
            }
        }
    }
    public void LocalizeLeft(int num)
    {
        currentLocaleIndex += num;
        if (currentLocaleIndex < 0)
        {
            currentLocaleIndex = LocalizationSettings.AvailableLocales.Locales.Count - 1;
        }
        else if (currentLocaleIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
        {
            currentLocaleIndex = 0;
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLocaleIndex];
        PlayerPrefs.SetString("locale", LocalizationSettings.SelectedLocale.Identifier.Code);
        PlayerPrefs.Save();
    }
}
