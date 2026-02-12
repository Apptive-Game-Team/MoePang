using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionWindow : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider SFXSlider;
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
    }

    public void OnEndButton()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX0_Default);
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
        SoundManager.Instance.PlaySFX(SFX.SFX0_Default);
        optionButton.interactable = true;
        Destroy(this.gameObject);
    }
}
