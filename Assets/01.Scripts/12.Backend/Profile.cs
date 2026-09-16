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
    }
}