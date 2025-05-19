using SpacetimeDB;
using SnowFight.Types;

namespace SnowFight.Models
{
    [Table(Name = "entity", Public = true)]
    public partial struct Entity
    {
        [PrimaryKey, AutoInc]
        public uint entity_id;
        public DbVector2 position;
        public float rotation;
    }
} 