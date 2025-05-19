using SpacetimeDB;
using SnowFight.Types;
using System;

namespace SnowFight.Utils
{
    public static class CollisionHelper
    {
        public static bool CheckCollisionWithObstacle(ReducerContext ctx, DbVector2 position, float radius)
        {
            // Check collision with every obstacle
            foreach (var obstacle in ctx.Db.obstacle.Iter())
            {
                var obstacleEntity = ctx.Db.entity.entity_id.Find(obstacle.entity_id);
                if (obstacleEntity == null) continue;
                
                // Calculate distance between entity and obstacle
                var diff = position - obstacleEntity.Value.position;
                
                if (obstacle.collider_type == ColliderType.Sphere)
                {
                    // For sphere colliders, check if distance is less than the sum of radii
                    if (diff.Magnitude < (obstacle.sphere_radius + radius))
                    {
                        return true;
                    }
                }
                else if (obstacle.collider_type == ColliderType.Box)
                {
                    // For box colliders, we need to account for rotation
                    // Convert to obstacle's local space
                    float rotationRad = -obstacleEntity.Value.rotation * (float)Math.PI / 180.0f;
                    
                    // Rotate vector (apply inverse rotation to convert to local space)
                    float rotatedX = diff.x * (float)Math.Cos(rotationRad) - diff.y * (float)Math.Sin(rotationRad);
                    float rotatedY = diff.x * (float)Math.Sin(rotationRad) + diff.y * (float)Math.Cos(rotationRad);
                    
                    // Check AABB collision in local space
                    float halfWidth = obstacle.box_size.x / 2.0f;
                    float halfHeight = obstacle.box_size.y / 2.0f;
                    
                    if (Math.Abs(rotatedX) < halfWidth + radius && 
                        Math.Abs(rotatedY) < halfHeight + radius)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public static bool CheckSnowBallPlayerCollision(ReducerContext ctx, DbVector2 position, float radius, uint snowballId, uint throwerId)
        {
            foreach (var puppet in ctx.Db.puppet.Iter())
            {
                // Skip the player who threw the snowball
                if (puppet.player_id == throwerId) continue;
                
                var puppetEntity = ctx.Db.entity.entity_id.Find(puppet.entity_id);
                if (puppetEntity == null) continue;
                
                // Calculate distance between snowball and puppet
                var diff = position - puppetEntity.Value.position;
                
                // Use a fixed collision radius for players (0.5f)
                if (diff.Magnitude < (0.5f + radius))
                {
                    // Player was hit, log it
                    Log.Info($"Player {puppet.player_id} was hit by a snowball thrown by player {throwerId}!");
                    
                    // Here you could implement additional hit logic
                    // For example, temporarily freeze player movement
                    var hitPuppet = puppet;
                    hitPuppet.locked_until = ctx.Timestamp + TimeSpan.FromMilliseconds(1500);
                    ctx.Db.puppet.entity_id.Update(hitPuppet);
                    
                    // Add hit state
                    PlayerStateHelper.AddState(ctx, puppet.player_id, PlayerActions.Hit);
                    
                    // Return true to indicate a hit
                    return true;
                }
            }
            return false;
        }

        public static bool CheckPlayerPlayerCollision(ReducerContext ctx, DbVector2 position, float radius, uint currentPlayerId) 
        {
            // Check collision with every other player
            foreach (var otherPuppet in ctx.Db.puppet.Iter()) 
            {
                // Skip checking against self
                if (otherPuppet.entity_id == currentPlayerId) continue;
                
                var otherEntity = ctx.Db.entity.entity_id.Find(otherPuppet.entity_id);
                if (otherEntity == null) continue;
                
                // Calculate distance between players
                var diff = position - otherEntity.Value.position;
                
                // Use a fixed collision radius for players (0.5f + 0.5f = 1.0f total distance)
                if (diff.Magnitude < (radius + 0.5f)) 
                {
                    // Log collision
                    Log.Info($"Player collision detected between {currentPlayerId} and {otherPuppet.entity_id}");
                    return true;
                }
            }
            
            return false;
        }
    }
} 