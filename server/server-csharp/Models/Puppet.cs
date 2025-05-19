using SpacetimeDB;
using SnowFight.Types;
using System.Collections.Generic;

namespace SnowFight.Models
{
    [Table(Name = "puppet", Public = true)]
    public partial struct Puppet
    {
        [PrimaryKey]
        public uint entity_id;
        [SpacetimeDB.Index.BTree]
        public uint player_id;
        public DbVector2 direction;
        public float speed;
        public SpacetimeDB.Timestamp lastHitTime;
        public List<PlayerActions> current_states;
        public bool has_snowball;
        public SpacetimeDB.Timestamp locked_until;
    }
} 