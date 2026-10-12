using Newtonsoft.Json;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankingEntry
    {
        [JsonProperty("ranking")]
        public long Ranking { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("max_stage")]
        public int MaxStage { get; set; }

        [JsonProperty("avatar_id")]
        public int AvatarId { get; set; }

        [JsonProperty("google_avatar_url")]
        public string GoogleAvatarUrl { get; set; }

        [JsonProperty("is_google_avatar")]
        public bool IsGoogleAvatar { get; set; }
    }
}