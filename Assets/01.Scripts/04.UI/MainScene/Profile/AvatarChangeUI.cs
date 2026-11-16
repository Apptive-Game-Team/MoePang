using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    public class AvatarChangeUI : MonoBehaviour
    {
        [SerializeField] private GameObject avatarItemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private AvatarDatabase avatarDatabase;
        private ProfileUI _profileUI;
        private _12.Backend.Profile _profile;
        private bool _isGoogleLogin;

        public int selectedAvatarId;

        private void Awake()
        {
            _profileUI = GetComponentInParent<ProfileUI>();

            if (GameManager.Instance.isLoggedIn)
            {
                _profile = SupabaseLoginManager.Instance.profile;
                _isGoogleLogin = PlayerPrefs.GetInt("LoginType", 0) == 1;
            }
        }

        private void OnEnable()
        {
            CreateAvatarList();
        }

        private void CreateAvatarList()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            if (_isGoogleLogin && !string.IsNullOrEmpty(_profile.GoogleAvatarUrl))
            {
                GameObject itemObject = Instantiate(avatarItemPrefab, content);
                AvatarItem item = itemObject.GetComponent<AvatarItem>();
                
                item.Initialize(_profileUI.googleAvatarSprite ,_profile.IsGoogleAvatar, SelectGoogleAvatar);
            }

            for (int i = 0; i < avatarDatabase.Count; i++)
            {
                int avatarId = i;
                GameObject itemObject = Instantiate(avatarItemPrefab, content);
                AvatarItem item = itemObject.GetComponent<AvatarItem>();
                
                item.Initialize(
                    avatarId,
                    avatarDatabase.GetAvatar(avatarId),
                    GameManager.Instance.isLoggedIn ? 
                        _profile.AvatarId : 
                        GameManager.Instance.localProfileData.avatarId,
                    _profile.IsGoogleAvatar,
                    SelectAvatar
                );
            }
        }

        private void SelectAvatar(int avatarId)
        {
            selectedAvatarId = avatarId;
            _profileUI.SetImage(avatarDatabase.GetAvatar(selectedAvatarId));
            _profileUI.SaveCloseAvatarChangeUI();
        }

        private void SelectGoogleAvatar(int _)
        {
            _profileUI.SetImage(_profileUI.googleAvatarSprite);
            _profileUI.SaveCloseAvatarChangeUIGoogle();
        }
    }
}