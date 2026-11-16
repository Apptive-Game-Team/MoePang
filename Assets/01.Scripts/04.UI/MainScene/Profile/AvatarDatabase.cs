using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts._04.UI.MainScene.Profile
{
    [CreateAssetMenu(fileName = "AvatarDatabase", menuName = "Game/Avatar Database")]
    public class AvatarDatabase : ScriptableObject
    {
        [SerializeField] private List<Sprite> avatars;

        public int Count => avatars.Count;

        public Sprite GetAvatar(int avatarId)
        {
            if (avatarId < 0 || avatarId >= avatars.Count)
            {
                return avatars[0];
            }

            return avatars[avatarId];
        }
    }
}