using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace _01.Scripts._12.Backend
{
    [Table("test_data")]
    public class TestData : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("number_value")]
        public int NumberValue { get; set; }

        [Column("created_at")]
        public System.DateTime CreatedAt { get; set; }
    }
}