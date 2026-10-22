using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    public class ProfileUI : MonoBehaviour
    {
        [Header("Profile UI")]
        [SerializeField] private Image profileUI;
        [SerializeField] private Image profileImage;
        [SerializeField] private AvatarChangeUI avatarChangeUI;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private NicknameChangeUI nicknameChangeUI;
        [SerializeField] private AvatarDatabase avatarDatabase;
        [SerializeField] private TextMeshProUGUI maxStageText;
        [SerializeField] private TextMeshProUGUI clearedTimeText;
        [SerializeField] private TextMeshProUGUI[] habitatBonuses;


        [Header("Simple Profile UI")] 
        [SerializeField] private Image simpleProfileImage;
        [SerializeField] private TMP_Text simpleNicknameText;
        [SerializeField] private TMP_Text simpleMaxStageText;

        private _12.Backend.Profile _profile; 
        public Sprite googleAvatarSprite;

        private void Awake()
        {
            _profile = SupabaseLoginManager.Instance.profile;
            googleAvatarSprite = GameManager.Instance.googleAvatarSprite;
            
            SetInitialSetting(_profile);
        }

        private void OnEnable()
        {
            SetStageAndHabitatInfo();
        }

        private async void SetInitialSetting(_12.Backend.Profile profile)
        {
            if (GameManager.Instance.isLoggedIn)
            {
                if (profile.IsGoogleAvatar && !string.IsNullOrEmpty(profile.GoogleAvatarUrl))
                {
                    SetImage(googleAvatarSprite);
                }
                else
                {
                    SetImage(avatarDatabase.GetAvatar(profile.AvatarId));
                }
                
                SetNickname(profile.Nickname);
                SetMaxStage(await SupabaseLoginManager.Instance.ProfileRepository.GetMyMaxStage());
            }
            else
            {
                LocalProfileData data = GameManager.Instance.localProfileData;
                
                SetImage(avatarDatabase.GetAvatar(data.avatarId));
                SetNickname(data.nickname);
                SetMaxStage(GameManager.Instance.playData.MaxStages.Values.Max());
            }
        }

        private void SetStageAndHabitatInfo()
        {
            PlayData playData = GameManager.Instance.playData;
            
            int maxStage = playData.MaxStages.Values.Max();
            StageType maxStageType = playData.MaxStages
                .FirstOrDefault(x => x.Value == maxStage)
                .Key;
            float minUsedTime = maxStage == 0 ? 0.0f : playData.StageData[maxStageType][maxStage - 1].minUsedTime;
            
            maxStageText.text = "stage " + maxStage;
            clearedTimeText.text = $"{minUsedTime:F}".Replace(".", " : ");

            for (int i = 0; i < habitatBonuses.Length; i++)
            {
                habitatBonuses[i].text = "+" + GameManager.Instance.playData.MaxStages[(StageType)(i + 1)];
            }
        }

        public void SetImage(Sprite sprite)
        {
            profileImage.sprite = sprite;
            simpleProfileImage.sprite = sprite;
        }

        public void SetNickname(string nickname)
        {
            nicknameText.text = nickname;
            simpleNicknameText.text = nickname;
        }

        public void SetMaxStage(int stage)
        {
            simpleMaxStageText.text = "Stage " + stage;
        }

        public void OpenProfile()
        {
            if (GameManager.Instance.isLoggedIn)
            {
                profileUI.gameObject.SetActive(true);
            }
        }

        public void CloseProfile()
        {
            profileUI.gameObject.SetActive(false);
        }

        public void OpenAvatarChangeUI()
        {
            avatarChangeUI.gameObject.SetActive(true);
        }

        public void SaveCloseAvatarChangeUI()
        {
            int id = avatarChangeUI.selectedAvatarId;
            avatarChangeUI.gameObject.SetActive(false);
            UpdateAvatarId(id);
        }

        public void SaveCloseAvatarChangeUIGoogle()
        {
            avatarChangeUI.gameObject.SetActive(false);
            UpdateGoogleAvatar();
        }

        public void CloseAvatarChangeUI()
        {
            avatarChangeUI.gameObject.SetActive(false);
        }

        public void OpenNicknameChangeUI()
        {
            nicknameChangeUI.gameObject.SetActive(true);
            nicknameChangeUI.Open();
        }

        public void SaveCloseNicknameChangeUI()
        {
            nicknameChangeUI.gameObject.SetActive(false);
            UpdateNickname();
        }

        public void CloseNicknameChangeUI()
        {
            nicknameChangeUI.gameObject.SetActive(false);
        }
        
        private async void UpdateAvatarId(int id)
        {
            if (GameManager.Instance.isLoggedIn)
            {
                SupabaseLoginManager.Instance.profile.AvatarId = id;
                SupabaseLoginManager.Instance.profile.IsGoogleAvatar = false;
                await SupabaseLoginManager.Instance.ProfileRepository.UpdateAvatar(id);
                await SupabaseLoginManager.Instance.ProfileRepository.UpdateAvatarMode(false);
            }
            else
            {
                GameManager.Instance.localProfileData.avatarId = id;
                GameManager.Instance.SaveLocalProfileData();
            }
        }

        private async void UpdateGoogleAvatar()
        {
            SupabaseLoginManager.Instance.profile.IsGoogleAvatar = true;
            await SupabaseLoginManager.Instance.ProfileRepository.UpdateAvatarMode(true);
        }

        private async void UpdateNickname()
        {
            if (GameManager.Instance.isLoggedIn)
            {
                SupabaseLoginManager.Instance.profile.Nickname = nicknameText.text;
                await SupabaseLoginManager.Instance.ProfileRepository.UpdateNickname(nicknameText.text);    
            }
            else
            {
                GameManager.Instance.localProfileData.nickname = nicknameText.text;
                GameManager.Instance.SaveLocalProfileData();
            }
        }
    }
}