using SpacetimeDB;
using SnowFight.Models;
using SnowFight.Types;
using SnowFight.Utils;
using SnowFight.Schedulers;
using System;

namespace SnowFight.Reducers
{
    public static partial class SnowballReducers
    {
        [Reducer]
        public static void SpawnDelayedSnowball(ReducerContext ctx, DelayedSnowball delayed)
        {
            SpawnSnowBall(ctx, delayed.player_id, delayed.position, ctx.Timestamp);
            Log.Info($"Spawned delayed snowball for player {delayed.player_id}");
        }

        [Reducer]
        public static void ThrowSnowBall(ReducerContext ctx, uint player_id, DbVector2 position)
        {
            var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
            if(puppet.locked_until > ctx.Timestamp){
                return;
            }
            if(puppet.has_snowball){
                // Schedule snowball to spawn after 1 second
                ctx.Db.delayed_snowball.Insert(new DelayedSnowball
                {
                    player_id = player_id,
                    position = position,
                    scheduled_at = new ScheduleAt.Time(ctx.Timestamp + TimeSpan.FromMilliseconds(1000))
                });
                
                puppet.has_snowball = false;
                puppet.locked_until = ctx.Timestamp + TimeSpan.FromMilliseconds(1000);
                ctx.Db.puppet.entity_id.Update(puppet);
                PlayerStateHelper.AddState(ctx, player_id, PlayerActions.Throw);
            }
        }

        [Reducer]
        public static void CraftSnowBall(ReducerContext ctx, uint player_id)
        {
            var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
            if(puppet.locked_until > ctx.Timestamp){
                return;
            }
            puppet.has_snowball = true;
            puppet.locked_until = ctx.Timestamp + TimeSpan.FromMilliseconds(1000);
            ctx.Db.puppet.entity_id.Update(puppet);
            PlayerStateHelper.AddState(ctx, player_id, PlayerActions.Craft);
        }

        public static Entity SpawnSnowBall(ReducerContext ctx, uint player_id, DbVector2 position, SpacetimeDB.Timestamp timestamp)
        {
            // Find the puppet to get facing direction based on rotation
            var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
           
            puppet.has_snowball = false;
            ctx.Db.puppet.entity_id.Update(puppet);
            
            // Get the puppet's entity to access rotation
            var puppetEntity = ctx.Db.entity.entity_id.Find(puppet.entity_id) ?? throw new Exception("Puppet entity not found");
            
            // Calculate direction vector from rotation (assuming rotation is in degrees around Y axis)
            float rotationRadians = puppetEntity.rotation * (float)Math.PI / 180f;
            
            // Calculate the main direction vector
            DbVector2 direction = new DbVector2(
                (float)Math.Sin(rotationRadians), 
                (float)Math.Cos(rotationRadians)
            );
            
            // Calculate the right vector by rotating the direction vector 90 degrees clockwise
            DbVector2 rightVector = new DbVector2(
                direction.y,
                -direction.x
            );
            
            // Add the offset to the right (0.5f of the right vector)
            var offsetdirection = direction + rightVector * 0.5f;
            
            // Offset the spawn position slightly in front of the player
            DbVector2 offsetPosition = position + offsetdirection.Normalized * 2.0f;
            
            var entity = ctx.Db.entity.Insert(new Entity
            {
                position = offsetPosition,
            });
            
            ctx.Db.snowball.Insert(new SnowBall
            {
                entity_id = entity.entity_id,
                player_id = player_id,
                direction = direction.Normalized,
                speed = 1.0f,
            });
            return entity;
        }
    }
} 