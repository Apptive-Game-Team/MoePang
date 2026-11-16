using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace _01.Scripts._12.Backend
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; }

        [Column("nickname")]
        public string Nickname { get; set; }
        
        [Column("avatar_id")]
        public int AvatarId { get; set; }
        
        [Column("is_google_avatar")]
        public bool IsGoogleAvatar { get; set; }
        
        [Column("google_avatar_url")]
        public string GoogleAvatarUrl { get; set; }
    }
}