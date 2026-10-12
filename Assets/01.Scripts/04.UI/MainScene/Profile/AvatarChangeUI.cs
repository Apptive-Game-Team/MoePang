using _01.Scripts._00.Manager;
using _01.Scripts._12.Backend;
using UnityEngine;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    public class AvatarChangeUI : MonoBehaviour
    {
        [SerializeField] private GameObject avatarItemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private AvatarDatabase avatarDatabase;
        private ProfileUI _profileUI;
        private _12.Backend.Profile _profile;

        public int selectedAvatarId;

        private void Awake()
        {
            _profileUI = GetComponentInParent<ProfileUI>();

            if (GameManager.Instance.IsLoggedIn)
            {
                _profile = SupabaseLoginManager.Instance.profile;
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

            for (int i = 0; i < avatarDatabase.Count; i++)
            {
                int avatarId = i;
                GameObject itemObject = Instantiate(avatarItemPrefab, content);

                AvatarItem item = itemObject.GetComponent<AvatarItem>();
                item.Initialize(
                    avatarId,
                    avatarDatabase.GetAvatar(avatarId),
                    GameManager.Instance.IsLoggedIn ? 
                        _profile.AvatarId : 
                        GameManager.Instance.localProfileData.avatarId,
                    SelectAvatar
                );
            }
        }

        private void SelectAvatar(int avatarId)
        {
            selectedAvatarId = avatarId;
            _profileUI.SetImage(avatarDatabase.GetAvatar(selectedAvatarId));
            _profileUI.CloseAvatarChangeUI();
        }
    }
}