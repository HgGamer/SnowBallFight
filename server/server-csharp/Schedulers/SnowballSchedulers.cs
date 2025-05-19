using SpacetimeDB;
using SnowFight.Types;

namespace SnowFight.Schedulers
{
    [Table(Name = "delayed_snowball", Scheduled = nameof(Reducers.SnowballReducers.SpawnDelayedSnowball), ScheduledAt = nameof(scheduled_at))]
    public partial struct DelayedSnowball
    {
        [PrimaryKey, AutoInc]
        public ulong id;
        public ScheduleAt scheduled_at;
        public uint player_id;
        public DbVector2 position;
    }
} 