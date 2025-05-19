using SpacetimeDB;

namespace SnowFight.Models
{
    // We're using this table as a singleton, so in this table
    // there will only be one element where the `id` is 0.
    [Table(Name = "config", Public = true)]
    public partial struct Config
    {
        [PrimaryKey]
        public uint id;
        public ulong world_size;
    }
} 