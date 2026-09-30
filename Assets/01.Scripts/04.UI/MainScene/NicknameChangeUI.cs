using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.MainScene
{
    public class NicknameChangeUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;

        private ProfileUI _profileUI;
        private Profile _profile;

        private void Awake()
        {
            _profileUI = GetComponentInParent<ProfileUI>();

            if (GameManager.Instance.IsLoggedIn)
            {
                _profile = SupabaseLoginManager.Instance.profile;
                
                if (_profile != null)
                {
                    nicknameInput.text = _profile.Nickname;
                }
            }
            else
            {
                nicknameInput.text = GameManager.Instance.localProfileData.nickname;
            }
        }

        public void Open()
        {
            nicknameInput.Select();
            nicknameInput.ActivateInputField();
        }

        public void Confirm()
        {
            string nickname = nicknameInput.text.Trim();

            if (string.IsNullOrWhiteSpace(nickname))
            {
                Debug.LogWarning("Nickname is empty.");
                return;
            }

            if (GameManager.Instance.IsLoggedIn && _profile == null)
            {
                Debug.LogWarning("Profile is null.");
                return;
            }

            _profileUI.SetNickname(nickname);
            _profileUI.CloseNicknameChangeUI();
        }

        public void Cancel()
        {
            _profileUI.CloseNicknameChangeUI();
        }
    }
}