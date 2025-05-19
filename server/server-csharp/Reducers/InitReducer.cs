using SpacetimeDB;
using SnowFight.Models;
using SnowFight.Types;
using SnowFight.Schedulers;
using System;
using SnowFight.Utils;

namespace SnowFight.Reducers
{
    public static partial class InitReducer
    {
        [Reducer(ReducerKind.Init)]
        public static void Init(ReducerContext ctx)
        {
            Log.Info($"Initializing...");
            //ctx.Db.config.Insert(new Config { world_size = 1000 });
            //ctx.Db.spawn_food_timer.Insert(new SpawnFoodTimer
            //{
            //    scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(500))
            //});
            ctx.Db.move_all_players_timer.Insert(new MoveAllPlayersTimer
            {
                scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(50))
            });
            ctx.Db.move_snowball_timer.Insert(new MoveSnowBallTimer
            {
                scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(50))
            });
            
            // Create obstacles
            CreateObstacles(ctx);
        }

        static void CreateSnowMan(ReducerContext ctx, DbVector2 position, float rotation)
        {
            // Create a sphere obstacle (rock)
            var snowmanEntity = ctx.Db.entity.Insert(new Entity
            {
                position = position,
                rotation = rotation
            });
            
            ctx.Db.obstacle.Insert(new Obstacle
            {
                entity_id = snowmanEntity.entity_id,
                obstacle_id = "snowman",
                collider_type = ColliderType.Sphere,
                box_size = new DbVector2(0, 0), // Not used for sphere collider
                sphere_radius = 0.55f
            });
        }

        private static void CreateObstacles(ReducerContext ctx)
        {
            // Create multiple snowmen at random positions with random rotations
            Random rng = ctx.Rng;
            
            // Create 10 snowmen with random positions and rotations
            for (int i = 0; i < 10; i++)
            {
                // Generate random angle (0-360 degrees)
                float angle = rng.Range(0, 360);
                
                // Convert angle to radians
                float angleRad = angle * (float)Math.PI / 180.0f;
                
                // Generate random direction vector based on angle
                DbVector2 direction = new DbVector2(
                    (float)Math.Cos(angleRad),
                    (float)Math.Sin(angleRad)
                );
                
                // Generate random distance between 10 and 40 units
                float distance = rng.Range(10f, 40f);
                
                // Calculate final position
                DbVector2 position = direction * distance;
                
                // Generate random rotation (0-360 degrees)
                float rotation = rng.Range(0f, 360f);
                
                // Create the snowman at the random position with random rotation
                CreateSnowMan(ctx, position, rotation);
                
                Log.Info($"Created snowman at position ({position.x}, {position.y}) with rotation {rotation}");
            }
            
            // Uncomment to create specific obstacles if needed
            //var boxEntity = ctx.Db.entity.Insert(new Entity
            //{
            //    position = new DbVector2(0, 15),
            //    rotation = 45 // Rotated 45 degrees
            //});
            //
            //ctx.Db.obstacle.Insert(new Obstacle
            //{
            //    entity_id = boxEntity.entity_id,
            //    obstacle_id = "barricade_1",
            //    collider_type = ColliderType.Box,
            //    box_size = new DbVector2(3, 3),
            //    sphere_radius = 0 // Not used for box collider
            //});
        }
    }
} 