using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene
{
    public class ProfileUI : MonoBehaviour
    {
        [SerializeField] private Image profileUI;
        [SerializeField] private Image profileImage;
        [SerializeField] private AvatarChangeUI avatarChangeUI;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private NicknameChangeUI nicknameChangeUI;
        [SerializeField] private AvatarDatabase avatarDatabase;

        private void Awake()
        {
            if (GameManager.Instance.IsLoggedIn)
            {
                Profile profile = SupabaseLoginManager.Instance.profile;
                SetImage(avatarDatabase.GetAvatar(profile.AvatarId));
                SetNickname(profile.Nickname);
            }
            else
            {
                LocalProfileData data = GameManager.Instance.localProfileData;
                SetImage(avatarDatabase.GetAvatar(data.avatarId));
                SetNickname(data.nickname);
            }
        }

        public void SetImage(Sprite sprite)
        {
            profileImage.sprite = sprite;
        }

        public void SetNickname(string nickname)
        {
            nicknameText.text = nickname;
        }

        public void OpenProfile()
        {
            profileUI.gameObject.SetActive(true);
        }

        public void CloseProfile()
        {
            profileUI.gameObject.SetActive(false);
        }

        public void OpenAvatarChangeUI()
        {
            avatarChangeUI.gameObject.SetActive(true);
        }

        public void CloseAvatarChangeUI()
        {
            int id = avatarChangeUI.selectedAvatarId;
            avatarChangeUI.gameObject.SetActive(false);
            UpdateAvatarId(id);
        }

        public void OpenNicknameChangeUI()
        {
            nicknameChangeUI.gameObject.SetActive(true);
            nicknameChangeUI.Open();
        }

        public void CloseNicknameChangeUI()
        {
            nicknameChangeUI.gameObject.SetActive(false);
            UpdateNickname();
        }
        
        private async void UpdateAvatarId(int id)
        {
            if (GameManager.Instance.IsLoggedIn)
            {
                SupabaseLoginManager.Instance.profile.AvatarId = id;
                await SupabaseLoginManager.Instance.ProfileRepository.UpdateAvatar(id);   
            }
            else
            {
                GameManager.Instance.localProfileData.avatarId = id;
                GameManager.Instance.SaveLocalProfileData();
            }
        }

        private async void UpdateNickname()
        {
            if (GameManager.Instance.IsLoggedIn)
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