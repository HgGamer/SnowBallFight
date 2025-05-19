using SpacetimeDB;

namespace SnowFight.Models
{
    [Table(Name = "logged_out_player")]
    [Table(Name = "player", Public = true)]
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity;
        [Unique, AutoInc]
        public uint player_id;
        public string name;
    }
} 