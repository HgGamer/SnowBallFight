using SpacetimeDB;
using SnowFight.Models;
using SnowFight.Types;
using SnowFight.Utils;
using SnowFight.Schedulers;
using System;
using System.Linq;

namespace SnowFight.Reducers
{
    public static partial class MovementReducers
    {
        [Reducer]
        public static void MoveAllPlayers(ReducerContext ctx, MoveAllPlayersTimer timer)
        {
            var movespeed = 0.5f;
            var puppet_directions = ctx.Db.puppet.Iter().Select(c => (c.entity_id, c.direction * c.speed)).ToDictionary();

            // Handle player input
            foreach (var puppet in ctx.Db.puppet.Iter())
            {
               
                if(puppet.locked_until > ctx.Timestamp){
                    var pup = puppet;
                    pup.speed = 0;
                    ctx.Db.puppet.entity_id.Update(pup);
                    continue;
                }else{
                    Log.Info($"Player {puppet.player_id} is not locked");
                    Log.Info($"Player states: {string.Join(", ", puppet.current_states)}");
                    if(puppet.current_states.Contains(PlayerActions.Hit)){
                        Log.Info($"Player {puppet.player_id} is standing up");
                       
                        var pup = puppet;
                        pup.current_states.Clear();
                        pup.speed = 0;
                        pup.has_snowball = false;
                      
                        pup.locked_until = ctx.Timestamp + TimeSpan.FromMilliseconds(200);
                        ctx.Db.puppet.entity_id.Update(pup);
                        PlayerStateHelper.AddState(ctx, puppet.player_id, PlayerActions.Standup);
                    }
                }
                var check_entity = ctx.Db.entity.entity_id.Find(puppet.entity_id);
                if (check_entity == null)
                {
                    // This can happen if the puppet has been eaten by another puppet.
                    continue;
                }
                var puppet_entity = check_entity.Value;

                var direction = puppet_directions[puppet.entity_id];
                if (float.IsNaN(direction.x) || float.IsNaN(direction.y))
                {
                    continue;
                }

                var new_pos = puppet_entity.position + direction * movespeed;
               
                // Check if new position magnitude exceeds 20, if so clamp it
                if (new_pos.Magnitude > 40)
                {
                    // Scale the position vector to have magnitude of 20
                    new_pos = new_pos.Normalized * 40;
                }
                
                // Check for obstacle collision
                if(CollisionHelper.CheckCollisionWithObstacle(ctx, new_pos, 0.5f)){
                   new_pos = puppet_entity.position;
                }
                
                // Check for player-to-player collision
                if(CollisionHelper.CheckPlayerPlayerCollision(ctx, new_pos, 0.5f, puppet.entity_id)){
                   new_pos = puppet_entity.position;
                }
                
                puppet_entity.position.x = new_pos.x;
                puppet_entity.position.y = new_pos.y;

                ctx.Db.entity.entity_id.Update(puppet_entity);
               
            }
            
            //this is not good here only remove throw and craft states
            
            foreach (var puppet in ctx.Db.puppet.Iter())
            {
                // Only clear Throw and Craft states, preserve other states
                if (puppet.current_states.Contains(PlayerActions.Throw) || puppet.current_states.Contains(PlayerActions.Craft))
                {
                    var updatedPuppet = puppet;
                    updatedPuppet.current_states.RemoveAll(state => state == PlayerActions.Throw || state == PlayerActions.Craft);
                    ctx.Db.puppet.entity_id.Update(updatedPuppet);
                    Log.Info($"Cleared Throw/Craft states for player {puppet.player_id}");
                }
            }
        }

        [Reducer]
        public static void MoveSnowBall(ReducerContext ctx, MoveSnowBallTimer timer)
        {
            var movespeed = 1f;
            var directions = ctx.Db.snowball.Iter().Select(c => (c.entity_id, c.direction * c.speed)).ToDictionary();

            // Handle player input
            foreach (var snowball in ctx.Db.snowball.Iter())
            {
               
                var check_entity = ctx.Db.entity.entity_id.Find(snowball.entity_id);
                if (check_entity == null)
                {
                   
                    continue; 
                }
                var snowball_entity = check_entity.Value;

                var direction = directions[snowball.entity_id];
                
                if (float.IsNaN(direction.x) || float.IsNaN(direction.y))
                {
                    Log.Info($"direction is null");
                    continue;
                }

                var new_pos = snowball_entity.position + direction * movespeed;
                Log.Info($"SB dir {direction.x} , {direction.y}");
                Log.Info($"SB new_pos {new_pos.x} , {new_pos.y}");
                snowball_entity.position.x = new_pos.x;
                snowball_entity.position.y = new_pos.y;
                ctx.Db.entity.entity_id.Update(snowball_entity);
                Log.Info($" {snowball.entity_id} at distance {new_pos.Magnitude}");
                
                // Check if snowball hit a player
                if(CollisionHelper.CheckSnowBallPlayerCollision(ctx, snowball_entity.position, 0.2f, snowball.entity_id, snowball.player_id))
                {
                    Log.Info($"Deleting snowball {snowball.entity_id} after hitting a player");
                    ctx.Db.snowball.entity_id.Delete(snowball.entity_id);
                    ctx.Db.entity.entity_id.Delete(snowball.entity_id);
                    continue;
                }
                
                // Check if snowball is too far away
                if(new_pos.Magnitude > 40f)
                {
                    Log.Info($"Deleting snowball {snowball.entity_id} at distance {new_pos.Magnitude}");
                    ctx.Db.snowball.entity_id.Delete(snowball.entity_id);
                    ctx.Db.entity.entity_id.Delete(snowball.entity_id);
                    continue;
                }
                
                // Check if snowball hit an obstacle
                if(CollisionHelper.CheckCollisionWithObstacle(ctx, snowball_entity.position, 0.2f)){
                    Log.Info($"Collision detected for snowball {snowball.entity_id}");
                    ctx.Db.snowball.entity_id.Delete(snowball.entity_id);
                    ctx.Db.entity.entity_id.Delete(snowball.entity_id);
                    continue;
                }
            }
        }
    }
} 