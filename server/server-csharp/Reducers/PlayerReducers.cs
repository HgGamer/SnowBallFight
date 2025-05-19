using SpacetimeDB;
using SnowFight.Models;
using SnowFight.Types;
using System;
using System.Collections.Generic;
using SnowFight.Utils;

namespace SnowFight.Reducers
{
    public static partial class PlayerReducers
    {
        // Constants
        private const uint START_PLAYER_SPEED = 10;
        private const float MIN_SPAWN_DISTANCE = 5f;
        private const float MAX_SPAWN_DISTANCE = 20f;
        
        [Reducer]
        public static void UpdatePlayerInput(ReducerContext ctx, DbVector2 direction, float rotation)
        {
            var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");

            foreach (var c in ctx.Db.puppet.player_id.Filter(player.player_id))
            {
                var puppet = c;
                if(puppet.locked_until > ctx.Timestamp){
                    continue;
                }
                puppet.direction = direction.Normalized;
                puppet.speed = Math.Clamp(direction.Magnitude, 0f, 1f);
                ctx.Db.puppet.entity_id.Update(puppet);
                var check_entity = ctx.Db.entity.entity_id.Find(puppet.entity_id);
                if (check_entity == null)
                {
                    continue;
                }
                var puppet_entity = check_entity.Value;
                puppet_entity.rotation = rotation;
                ctx.Db.entity.entity_id.Update(puppet_entity);
            }
        }

        [Reducer(ReducerKind.ClientConnected)]
        public static void Connect(ReducerContext ctx)
        {
            var player = ctx.Db.logged_out_player.identity.Find(ctx.Sender);
            if (player != null)
            {
                ctx.Db.player.Insert(player.Value);
                ctx.Db.logged_out_player.identity.Delete(player.Value.identity);
            }
            else
            {
                ctx.Db.player.Insert(new Player
                {
                    identity = ctx.Sender,
                    name = "",
                });
            }
        }

        [Reducer(ReducerKind.ClientDisconnected)]
        public static void Disconnect(ReducerContext ctx)
        {
            var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
            // Remove any puppets from the arena
            foreach (var puppet in ctx.Db.puppet.player_id.Filter(player.player_id))
            {
                var entity = ctx.Db.entity.entity_id.Find(puppet.entity_id) ?? throw new Exception("Could not find puppet");
                ctx.Db.entity.entity_id.Delete(entity.entity_id);
                ctx.Db.puppet.entity_id.Delete(entity.entity_id);
            }
            ctx.Db.logged_out_player.Insert(player);
            ctx.Db.player.identity.Delete(player.identity);
        }

        [Reducer]
        public static void EnterGame(ReducerContext ctx, string name)
        {
            Log.Info($"Creating player with name {name}");
            var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
            player.name = name;
            ctx.Db.player.identity.Update(player);
            SpawnPlayerInitialPuppet(ctx, player.player_id);
        }

        public static Entity SpawnPlayerInitialPuppet(ReducerContext ctx, uint player_id)
        {
            var rng = ctx.Rng;
            
            // Generate random spawn position
            // First generate a random angle (0-360 degrees)
            float angle = rng.Range(0f, 360f);
            
            // Convert angle to radians
            float angleRad = angle * (float)Math.PI / 180.0f;
            
            // Generate random direction vector based on angle
            DbVector2 direction = new DbVector2(
                (float)Math.Cos(angleRad),
                (float)Math.Sin(angleRad)
            );
            
            // Generate random distance between MIN_SPAWN_DISTANCE and MAX_SPAWN_DISTANCE
            float distance = rng.Range(MIN_SPAWN_DISTANCE, MAX_SPAWN_DISTANCE);
            
            // Calculate final position
            DbVector2 position = direction * distance;
            
            // Generate random initial rotation
            float rotation = rng.Range(0f, 360f);
            
            Log.Info($"Spawning player {player_id} at position ({position.x}, {position.y}) with rotation {rotation}");
            
            return SpawnPuppetAt(
                ctx,
                player_id,
                position,
                ctx.Timestamp
            );
        }

        public static Entity SpawnPuppetAt(ReducerContext ctx, uint player_id, DbVector2 position, SpacetimeDB.Timestamp timestamp)
        {
            var entity = ctx.Db.entity.Insert(new Entity
            {
                position = position,
                rotation = 0, // Initial rotation is set to 0, will be updated by player input
            });

            ctx.Db.puppet.Insert(new Puppet
            {
                entity_id = entity.entity_id,
                player_id = player_id,
                direction = new DbVector2(0, 1),
                speed = 0f,
                lastHitTime = timestamp,
                has_snowball = true,
                current_states = new List<PlayerActions>()
            });
            return entity;
        }
    }
} 