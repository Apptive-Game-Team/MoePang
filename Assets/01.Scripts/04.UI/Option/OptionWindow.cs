using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionWindow : MonoBehaviour
{
    private const string LanguageCodeKey = "LanguageCode";

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider SFXSlider;
    
    [Header("Language")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] public Button optionButton;//이 창을 생성하는 버튼을 생성 시 연결해줘야함
    /// <summary>
    /// 옵션 창 생성 시 초기화
    /// </summary>
    /*float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
    SoundManager.Instance.MasterSoundVolume = masterVolume;
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
    SoundManager.Instance.BGMSoundVolume = bgmVolume;
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    SoundManager.Instance.SFXSoundVolume = sfxVolume;*/ //GameManager생성 시 Start에 넣어주세요
    public void Init()
    {
        masterSlider.minValue = 0.0001f;
        masterSlider.value = SoundManager.Instance.MasterSoundVolume;
        masterSlider.onValueChanged.AddListener((float value) => { SoundManager.Instance.MasterSoundVolume = value; PlayerPrefs.SetFloat("MasterVolume", value); PlayerPrefs.Save(); });

        bgmSlider.minValue = 0.0001f;
        bgmSlider.value = SoundManager.Instance.BGMSoundVolume;
        bgmSlider.onValueChanged.AddListener((float value) => { SoundManager.Instance.BGMSoundVolume = value; PlayerPrefs.SetFloat("BGMVolume", value); PlayerPrefs.Save(); });

        SFXSlider.minValue = 0.0001f;
        SFXSlider.value = SoundManager.Instance.SFXSoundVolume;
        SFXSlider.onValueChanged.AddListener((float value) => { SoundManager.Instance.SFXSoundVolume = value; PlayerPrefs.SetFloat("SFXVolume", value); PlayerPrefs.Save(); });

        StartCoroutine(InitLanguageDropdown());
    }

    private IEnumerator InitLanguageDropdown()
    {
        yield return LocalizationSettings.InitializationOperation;

        TMP_Dropdown dropdown = GetLanguageDropdown();
        if (dropdown == null)
        {
            yield break;
        }

        IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        List<string> options = new();

        foreach (Locale locale in locales)
        {
            options.Add(locale.LocaleName);
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        string savedLanguageCode = PlayerPrefs.GetString(LanguageCodeKey, string.Empty);
        if (!string.IsNullOrEmpty(savedLanguageCode))
        {
            foreach (Locale locale in locales)
            {
                if (locale.Identifier.Code == savedLanguageCode)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    break;
                }
            }
        }

        int selectedIndex = Mathf.Max(0, locales.IndexOf(LocalizationSettings.SelectedLocale));
        dropdown.SetValueWithoutNotify(selectedIndex);
        dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < 0 || index >= locales.Count)
        {
            return;
        }

        Locale selectedLocale = locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;
        PlayerPrefs.SetString(LanguageCodeKey, selectedLocale.Identifier.Code);
        PlayerPrefs.Save();
    }

    private TMP_Dropdown GetLanguageDropdown()
    {
        if (languageDropdown == null)
        {
            languageDropdown = GetComponentInChildren<TMP_Dropdown>(true);
        }

        return languageDropdown;
    }

    public void OnEndButton()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        optionButton.interactable = true;
        SceneManager.LoadScene(0);
    }

    public void ExitGame()//게임종료
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnBackButton()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        optionButton.interactable = true;
        Destroy(this.gameObject);
    }

    public void OnTitleButton()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        SceneManager.LoadScene("00.TitleScene");
    }
}
