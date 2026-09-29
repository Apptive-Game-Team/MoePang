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

        private async void Awake()
        {
            Profile profile = SupabaseLoginManager.Instance.profile;
            
            SetProfile(profile);
        }
        
        public void SetProfile(Profile profile)
        {
            nicknameText.text = profile.Nickname;
            profileImage.sprite = avatarDatabase.GetAvatar(profile.AvatarId);
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
            await SupabaseLoginManager.Instance.ProfileRepository.UpdateAvatar(id);
        }

        private async void UpdateNickname()
        {
            await SupabaseLoginManager.Instance.ProfileRepository.UpdateNickname(nicknameText.text);
        }
    }
}