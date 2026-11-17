using _01.Scripts._12.Backend;
using _01.Scripts._04.UI.MainScene.Profile;
using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankerCommentDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text commentText;

        public void Show(RankingEntry entry)
        {
            commentText.richText = false;

            var profile = SupabaseLoginManager.Instance.profile;
            bool isMe = profile != null && profile.Id == entry.UserId;
            commentText.text = ProfileCommentUI.Display(isMe ? profile.Comment : entry.Comment,
                isMe ? profile.Nickname : entry.Nickname);
        }
    }
}
