using SpacetimeDB;
using SnowFight.Types;
using SnowFight.Models;

namespace SnowFight.Utils
{
    public static class PlayerStateHelper
    {
        public static void ClearStates(ReducerContext ctx, Puppet puppet)
        {
            puppet.current_states.Clear();
            ctx.Db.puppet.entity_id.Update(puppet);
        }

        public static void AddState(ReducerContext ctx, uint player_id, PlayerActions state)
        {
            Log.Info($"Adding state {state} to player {player_id}");
            var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
            puppet.current_states.Add(state);
            ctx.Db.puppet.entity_id.Update(puppet);
        }
    }
} 