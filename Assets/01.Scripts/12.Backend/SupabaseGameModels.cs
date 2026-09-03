using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace _01.Scripts._12.Backend
{
    [Table("player_data")]
    public class PlayerDataModel : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public string UserId { get; set; }

        [Column("gold")]
        public int Gold { get; set; }

        [Column("dia")]
        public int Dia { get; set; }

        [Column("castle_level")]
        public int CastleLevel { get; set; }
    }


    [Table("player_progress")]
    public class PlayerProgressModel : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public string UserId { get; set; }

        [Column("stage_type")]
        public int StageType { get; set; }

        [Column("max_stage")]
        public int MaxStage { get; set; }

        [Column("selected_stage")]
        public int SelectedStage { get; set; }
    }


    [Table("player_stages")]
    public class PlayerStageModel : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public string UserId { get; set; }

        [Column("stage_type")]
        public int StageType { get; set; }

        [Column("stage_num")]
        public int StageNum { get; set; }

        [Column("min_used_time")]
        public float MinUsedTime { get; set; }

        [Column("min_used_tile")]
        public int MinUsedTile { get; set; }

        [Column("max_used_tile")]
        public int MaxUsedTile { get; set; }
    }


    [Table("player_units")]
    public class PlayerUnitModel : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public string UserId { get; set; }

        [Column("unit_id")]
        public int UnitId { get; set; }

        [Column("unlocked")]
        public bool Unlocked { get; set; }

        [Column("level")]
        public int Level { get; set; }
    }


    [Table("player_items")]
    public class PlayerItemModel : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public string UserId { get; set; }

        [Column("item_type")]
        public int ItemType { get; set; }

        [Column("amount")]
        public int Amount { get; set; }
    }


    [Table("player_combos")]
    public class PlayerComboModel : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public string UserId { get; set; }

        [Column("habitat")]
        public int Habitat { get; set; }

        [Column("level")]
        public int Level { get; set; }
    }
}