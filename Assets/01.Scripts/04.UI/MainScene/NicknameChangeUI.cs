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
            _profile = SupabaseLoginManager.Instance.profile;

            if (_profile != null)
            {
                nicknameInput.text = _profile.Nickname;
            }
        }

        public void Open()
        {
            if (_profile != null)
            {
                nicknameInput.text = _profile.Nickname;
            }

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

            if (_profile == null)
            {
                Debug.LogWarning("Profile is null.");
                return;
            }

            _profile.Nickname = nickname;

            _profileUI.SetNickname(nickname);
            _profileUI.CloseNicknameChangeUI();
        }

        public void Cancel()
        {
            _profileUI.CloseNicknameChangeUI();
        }
    }
}