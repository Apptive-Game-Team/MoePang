using _01.Scripts._12.Backend;
using UnityEngine;
using UnityEngine.Serialization;

namespace _01.Scripts._04.UI.MainScene
{
    public class AvatarChangeUI : MonoBehaviour
    {
        [SerializeField] private GameObject avatarItemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private AvatarDatabase avatarDatabase;
        private ProfileUI _profileUI;
        private Profile _profile;

        public int selectedAvatarId;

        private async void Awake()
        {
            _profileUI = GetComponentInParent<ProfileUI>();
            _profile = SupabaseLoginManager.Instance.profile;
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
                    _profile.AvatarId,
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