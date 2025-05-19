using SpacetimeDB;
using SnowFight.Types;

namespace SnowFight.Models
{
    [Table(Name = "obstacle", Public = true)]
    public partial struct Obstacle
    {
        [PrimaryKey]
        public uint entity_id;
        public string obstacle_id;
        public ColliderType collider_type;
        public DbVector2 box_size;    // Used when collider_type is Box
        public float sphere_radius;   // Used when collider_type is Sphere
    }
} 