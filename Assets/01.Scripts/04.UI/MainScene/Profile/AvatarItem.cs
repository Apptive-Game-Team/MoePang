using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    public class AvatarItem : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private GameObject selectedImage;

        private int _avatarId;
        private System.Action<int> _onClick;

        public void Initialize(int avatarId, Sprite avatarSprite, int currentAvatarId, System.Action<int> onClick)
        {
            _avatarId = avatarId;
            _onClick = onClick;

            avatarImage.sprite = avatarSprite;
            SetSelected(avatarId == currentAvatarId);
        }

        public void SetSelected(bool selected)
        {
            selectedImage.SetActive(selected);
        }

        public void OnClick()
        {
            _onClick?.Invoke(_avatarId);
        }
    }
}