using SpacetimeDB;

namespace SnowFight.Schedulers
{
    [Table(Name = "move_all_players_timer", Scheduled = nameof(Reducers.MovementReducers.MoveAllPlayers), ScheduledAt = nameof(scheduled_at))]
    public partial struct MoveAllPlayersTimer
    {
        [PrimaryKey, AutoInc]
        public ulong scheduled_id;
        public ScheduleAt scheduled_at;
    }

    [Table(Name = "move_snowball_timer", Scheduled = nameof(Reducers.MovementReducers.MoveSnowBall), ScheduledAt = nameof(scheduled_at))]
    public partial struct MoveSnowBallTimer
    {
        [PrimaryKey, AutoInc]
        public ulong scheduled_id;
        public ScheduleAt scheduled_at;
    }
} 