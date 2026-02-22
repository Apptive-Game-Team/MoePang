using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.Audio;
using System.Collections;

// 각 배경음악
public enum BGM
{
    //규칙 : BGM0_Default
    BGM0_Default,

    Count //Count체크용 enum, 삭제 금지
}

// 각 효과음
public enum SFX
{
    //규칙 : SFX0_Default
    SFX0_Default,

    Count //Count체크용 enum, 삭제 금지
}

//사운드 추가 시 위 Enum에 추가 할 것
/// <summary>
/// <para>사용 예시 :</para>
/// <para>받아온 사운드를 Resource/Sound/BGM 혹은 SFX에 enum과 동일한 명으로 추가 후 enum에 추가</para>
/// <para>원하는 부분에 SoundManager.Instance.PlaySFX(SFX.원하는 enum); 혹은 SoundManager.Instance.PlayBGM(BGM.원하는 enum, loop여부);</para>
/// 브금 교체 시 굳이 기존 브금 중지하지 않아도 새로운 브금 재생 시 자동 중지됨
/// </summary>
public class SoundManager : SingletonObject<SoundManager>
{
    public AudioMixer Mixer { get; private set; }

    [Header("BGM")]
    List<AudioClip> bgmClips = new List<AudioClip>();
    float bgmVolume = 0.5f;
    AudioSource bgmPlayer;
    AudioSource bgmBuffer;
    bool isBGMLooping = false;

    [Header("SFX")]
    List<AudioClip> sfxClips = new List<AudioClip>();
    float sfxVolume = 0.5f;
    int channels = 20;
    AudioSource[] sfxPlayers;
    int channelIndex;

    // AudioMixer - Master 볼륨
    public float MasterSoundVolume
    {
        get
        {
            float temp;
            Mixer.GetFloat("MasterSound", out temp);
            return Mathf.Pow(10, temp * 0.05f);
        }

        set
        {
            Mixer.SetFloat("MasterSound", Mathf.Log10(value) * 20);
        }
    }
    // AudioMixer - BGM 볼륨
    public float BGMSoundVolume
    {
        get
        {
            float temp;
            Mixer.GetFloat("BGMSound", out temp);
            return Mathf.Pow(10, temp * 0.05f);
        }

        set
        {
            Mixer.SetFloat("BGMSound", Mathf.Log10(value) * 20);
        }
    }
    // AudioMixer - SFX 볼륨
    public float SFXSoundVolume
    {
        get
        {
            float temp;
            Mixer.GetFloat("SFXSound", out temp);
            return Mathf.Pow(10, temp * 0.05f);
        }

        set
        {
            Mixer.SetFloat("SFXSound", Mathf.Log10(value) * 20);
        }
    }
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    // 초기화 BGM은 메인과 버퍼 2개가 있으며 SFX는 채널수를 지정해서 그 갯수만큼 만듦
    public void Init()
    {
        Mixer = Resources.Load<AudioMixer>($"Sound/Mixer");

        GameObject bgmObject = new GameObject("BgmPlayer");

        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.outputAudioMixerGroup = Mixer.FindMatchingGroups("BGM")[0];
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.dopplerLevel = 0.0f;
        bgmPlayer.reverbZoneMix = 0.0f;

        GameObject bufferObject = new GameObject("BgmPlayer");
        bufferObject.transform.parent = transform;
        bgmBuffer = bufferObject.AddComponent<AudioSource>();
        bgmBuffer.outputAudioMixerGroup = Mixer.FindMatchingGroups("BGM")[0];
        bgmBuffer.playOnAwake = false;
        bgmBuffer.loop = true;
        bgmBuffer.volume = bgmVolume;
        bgmBuffer.dopplerLevel = 0.0f;
        bgmBuffer.reverbZoneMix = 0.0f;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;

        sfxPlayers = new AudioSource[channels];

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].outputAudioMixerGroup = Mixer.FindMatchingGroups("SFX")[0];
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].dopplerLevel = 0.0f;
            sfxPlayers[index].reverbZoneMix = 0.0f;
        }

        isBGMLooping = false;

        AddBGMClips();
        AddSFXClips();
    }

    void AddBGMClips()
    {
        for (int i = 0; i < (int)BGM.Count; i++)
        {
            bgmClips.Add(Resources.Load<AudioClip>($"Sound/BGM/{(BGM)i}"));
        }
    }
    void AddSFXClips()
    {
        for (int i = 0; i < (int)SFX.Count; i++)
        {
            sfxClips.Add(Resources.Load<AudioClip>($"Sound/SFX/{(SFX)i}"));
        }
    }

    // BGM을 실행
    public void PlayBgm(BGM bgm, bool isLoop)
    {
        if (bgmPlayer.isPlaying)
        {
            bgmBuffer.clip = bgmClips[(int)bgm];
            bgmBuffer.Play();
            bgmBuffer.volume = bgmVolume;
            StartCoroutine(SoundSmooth(bgmPlayer, true));
            StartCoroutine(SoundSmooth(bgmBuffer, false));

            var temp = bgmBuffer;
            bgmBuffer = bgmPlayer;
            bgmPlayer = temp;

            bgmPlayer.loop = isLoop;
            bgmBuffer.loop = isLoop;
        }
        else
        {
            bgmPlayer.clip = bgmClips[(int)bgm];
            bgmPlayer.volume = bgmVolume;
            bgmPlayer.Play();
            StartCoroutine(SoundSmooth(bgmPlayer, false));
            bgmPlayer.loop = isLoop;
        }
    }
    // Rnd으로 플레이
    public void PlayRndBgm(BGM[] bgmList, bool isLoop)
    {
        if (bgmList == null || bgmList.Length == 0)
        {
            Debug.LogWarning("RndPlayBgm: BGM 리스트가 비어 있습니다.");
            return;
        }

        // 랜덤으로 하나 선택
        BGM randomBgm = bgmList[UnityEngine.Random.Range(0, bgmList.Length)];

        // 기존 PlayBgm 호출
        PlayBgm(randomBgm, isLoop);
    }
    // BGM을 멈춤
    public void StopBgm()
    {
        StartCoroutine(SoundSmooth(bgmPlayer, true));
        StopCoroutine("BGMRandomLoop");
        isBGMLooping = false;
    }
    public void SetBGMPicth(float val)
    {
        bgmPlayer.pitch = val;
    }
    // SFX를 실행
    public void PlaySFX(SFX sfx, float Pitch = 1, bool isLoop = false)
    {
        if (Pitch < 0)
            Pitch = UnityEngine.Random.Range(0.9f, 1.1f);

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].loop = isLoop;
            sfxPlayers[loopIndex].pitch = Pitch;
            sfxPlayers[loopIndex].volume = sfxVolume;
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
    // SFX를 서서히 실행
    public void SmoothPlaySfx(SFX sfx, float Pitch = 1, bool isLoop = false)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].loop = isLoop;
            sfxPlayers[loopIndex].pitch = Pitch;
            sfxPlayers[loopIndex].Play();
            StartCoroutine(SoundSmooth(sfxPlayers[loopIndex], false));
            break;
        }
    }
    // SFX를 멈춤
    public void StopSfx(SFX sfx)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            if (sfxPlayers[index].clip == sfxClips[(int)sfx])
            {
                StartCoroutine(SoundSmooth(sfxPlayers[index], true));
            }
        }
    }

    // 특정 값까지 BGM을 서서히 줄임
    public void SetBGMSoundVolume(float val)
    {
        StartCoroutine(BGMSmoothVolum(val, 1));
    }

    // BGM을 실행하고 끝났을시 랜덤으로 다시 돌림
    public void StartBGMRandomLoop(int num)
    {
        if (isBGMLooping) return;
        StopCoroutine("BGMRandomLoop");
        StartCoroutine("BGMRandomLoop", num);
        isBGMLooping = true;
    }

    // 사운드 크기가 특정값까지 자연스럽게 바뀜
    IEnumerator BGMSmoothVolum(float endVolum, float time)
    {
        float DeltaVolum = (endVolum - bgmPlayer.volume) * 0.1f;
        float second = time * 0.1f;

        for (int i = 0; i < 10; i++)
        {
            bgmPlayer.volume += DeltaVolum;
            yield return new WaitForSeconds(second);
        }
    }

    // 사운드의 크기를 서서히 줄이거나 늘릴때 사용 
    IEnumerator SoundSmooth(AudioSource audio, bool isDown)
    {
        float startvolume = audio.volume;
        float start = 0;
        float num = 0;
        if (isDown)
        {
            start = audio.volume;
            num = -audio.volume / 10;
        }
        else
        {
            start = 0;
            num = audio.volume / 10;
        }

        audio.volume = start;

        for (int i = 0; i < 10; i++)
        {
            audio.volume += num;
            yield return new WaitForSeconds(0.095f);
        }

        if (isDown)
        {
            audio.Stop();
            audio.volume = startvolume;
        }
    }

    // 2초마다 한번씩 BGM이 끝났는지 검사 후 끝났으면 다음 BGM을 틀음
    IEnumerator BGMRandomLoop(int num)
    {
        int[] temp = new int[num];
        for (int i = 0; i < num; i++)
            temp[i] = i;

        for (int i = 0; i < temp.Length; i++)
        {
            int randNum1 = UnityEngine.Random.Range(0, temp.Length);
            int tempint1 = temp[i];
            temp[i] = temp[randNum1];
            temp[randNum1] = tempint1;
        }

        int index = 0;

        while (true)
        {
            if (!bgmPlayer.isPlaying)
            {
                PlayBgm((BGM)temp[index], false);
                index++;

                if (index >= temp.Length)
                {
                    index = 0;

                    for (int i = 0; i < temp.Length; i++)
                    {
                        int randNum2 = UnityEngine.Random.Range(0, temp.Length);
                        int tempint2 = temp[i];
                        temp[i] = temp[randNum2];
                        temp[randNum2] = tempint2;
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

}
