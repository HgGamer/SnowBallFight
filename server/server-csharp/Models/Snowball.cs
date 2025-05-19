using SpacetimeDB;
using SnowFight.Types;

namespace SnowFight.Models
{
    [Table(Name = "snowball", Public = true)]
    public partial struct SnowBall
    {
        [PrimaryKey]
        public uint entity_id;
        [SpacetimeDB.Index.BTree]
        public uint player_id;
        public DbVector2 direction;
        public float speed;
    }
} 