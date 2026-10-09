using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    public class ProfileUI : MonoBehaviour
    {
        [SerializeField] private Image profileUI;
        [SerializeField] private Image profileImage;
        [SerializeField] private AvatarChangeUI avatarChangeUI;
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private NicknameChangeUI nicknameChangeUI;
        [SerializeField] private AvatarDatabase avatarDatabase;

        private _12.Backend.Profile _profile;

        private void Awake()
        {
            _profile = SupabaseLoginManager.Instance.profile;
            
            SetInitialSetting(_profile);
        }

        private void SetInitialSetting(_12.Backend.Profile profile)
        {
            if (GameManager.Instance.IsLoggedIn)
            {
                if (profile.IsGoogleAvatar && !string.IsNullOrEmpty(profile.GoogleAvatarUrl))
                {
                    LoadGoogleAvatar(profile.GoogleAvatarUrl, profileImage);
                }
                else
                {
                    SetImage(avatarDatabase.GetAvatar(profile.AvatarId));
                }
                
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
            if (_profile.IsGoogleAvatar)
            {
                return;
            }
            
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

        public void ChangeAvatarMode(Button button)
        {
            if (!GameManager.Instance.IsLoggedIn)
            {
                return;
            }
            
            _profile.IsGoogleAvatar = !_profile.IsGoogleAvatar;
            
            ColorBlock colors = button.colors;
            colors.normalColor = _profile.IsGoogleAvatar ? Color.gray : Color.white;
            colors.highlightedColor = _profile.IsGoogleAvatar ? Color.gray : Color.white;
            colors.pressedColor = _profile.IsGoogleAvatar ? Color.gray : Color.white;
            colors.selectedColor = _profile.IsGoogleAvatar ? Color.gray : Color.white;
            button.colors = colors;
            
            if (_profile.IsGoogleAvatar && !string.IsNullOrEmpty(_profile.GoogleAvatarUrl))
            {
                LoadGoogleAvatar(_profile.GoogleAvatarUrl, profileImage);
            }
            else
            {
                SetImage(avatarDatabase.GetAvatar(_profile.AvatarId));
            }

            UpdateAvatarMode(_profile.IsGoogleAvatar);
        }

        private void UpdateAvatarMode(bool flag)
        {
            UpdateAvatarModeTask(flag);
        }

        private async void UpdateAvatarModeTask(bool flag)
        {
            await SupabaseLoginManager.Instance.ProfileRepository.UpdateAvatarMode(flag);
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

        private void LoadGoogleAvatar(string imageUrl, Image target)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                Debug.LogWarning("Google avatar URL is empty.");
                return;
            }

            StartCoroutine(LoadGoogleAvatarImage(imageUrl, target));
        }
        
        private IEnumerator LoadGoogleAvatarImage(string imageUrl, Image target)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Google avatar load failed.\n{request.error}"
                );

                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (texture == null)
            {
                Debug.LogError("Google avatar texture is null.");
                yield break;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    texture.width,
                    texture.height
                ),
                new Vector2(0.5f, 0.5f)
            );

            target.sprite = sprite;
        }
    }
}