using SpacetimeDB;

public static partial class Module
{ 
    
    [SpacetimeDB.Type]
    public enum PlayerActions{
        Throw,
        Hit,
        Craft,
        Standup,
    }
    
    [SpacetimeDB.Type]
    public enum ColliderType {
        Box,
        Sphere
    }
  

    // We're using this table as a singleton, so in this table
    // there will only be one element where the `id` is 0.
    [Table(Name = "config", Public = true)]
    public partial struct Config
    {
        [PrimaryKey]
        public uint id;
        public ulong world_size;
    }
    [Table(Name = "entity", Public = true)]
    public partial struct Entity
    {
        [PrimaryKey, AutoInc]
        public uint entity_id;
        public DbVector2 position;
        public float rotation;


    }
    
    [Table(Name = "obstacle", Public = true)]
    public partial struct Obstacle
    {
        [PrimaryKey]
        public uint entity_id;
        public string obstacle_id;
        public ColliderType collider_type;
        public DbVector2 box_size;    // Used when collider_type is Box
        public float sphere_radius;   // Used when collider_type is Sphere
    }
    
    [Table(Name = "logged_out_player")]
    [Table(Name = "player", Public = true)]
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity;
        [Unique, AutoInc]
        public uint player_id;
        public string name;
    }
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
    [Table(Name = "snowball", Public = true)]
    public partial struct SnowBall
    {
        [PrimaryKey]
        public uint entity_id;
        [SpacetimeDB.Index.BTree]
        public uint player_id;
        public DbVector2 direction;

        public float speed;
    }


    // This allows us to store 2D points in tables.
    [SpacetimeDB.Type]
    public partial struct DbVector2
    {
        public float x;
        public float y;

        public DbVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public float SqrMagnitude => x * x + y * y;
        public float Magnitude => MathF.Sqrt(SqrMagnitude);
        public DbVector2 Normalized => this / Magnitude;

        public static DbVector2 operator +(DbVector2 a, DbVector2 b) => new DbVector2(a.x + b.x, a.y + b.y);
        public static DbVector2 operator -(DbVector2 a, DbVector2 b) => new DbVector2(a.x - b.x, a.y - b.y);
        public static DbVector2 operator *(DbVector2 a, float b) => new DbVector2(a.x * b, a.y * b);
        public static DbVector2 operator /(DbVector2 a, float b) => new DbVector2(a.x / b, a.y / b);
    }

    [Reducer]
    public static void UpdatePlayerInput(ReducerContext ctx, DbVector2 direction, float rotation)
    {
        var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        foreach (var c in ctx.Db.puppet.player_id.Filter(player.player_id))
        {
            var puppet = c;
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

    [Table(Name = "move_all_players_timer", Scheduled = nameof(MoveAllPlayers), ScheduledAt = nameof(scheduled_at))]
    public partial struct MoveAllPlayersTimer
    {
        [PrimaryKey, AutoInc]
        public ulong scheduled_id;
        public ScheduleAt scheduled_at;
    }
    [Table(Name = "move_snowball_timer", Scheduled = nameof(MoveSnowBall), ScheduledAt = nameof(scheduled_at))]
    public partial struct MoveSnowBallTimer
    {
        [PrimaryKey, AutoInc]
        public ulong scheduled_id;
        public ScheduleAt scheduled_at;
    }
    const uint START_PLAYER_SPEED = 10;
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
            if(new_pos.Magnitude > 40f)
            {
                Log.Info($"Deleting snowball {snowball.entity_id} at distance {new_pos.Magnitude}");
                ctx.Db.snowball.entity_id.Delete(snowball.entity_id);
                ctx.Db.entity.entity_id.Delete(snowball.entity_id);
            }
        }

    }

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
            
            puppet_entity.position.x = new_pos.x;
            puppet_entity.position.y = new_pos.y;

            ctx.Db.entity.entity_id.Update(puppet_entity);
           
        }
        foreach (var puppet in ctx.Db.puppet.Iter())
        {
            ClearStates(ctx, puppet);
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
    public static float Range(this Random rng, float min, float max) => rng.NextSingle() * (max - min) + min;

    public static uint Range(this Random rng, uint min, uint max) => (uint)rng.NextInt64(min, max);

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
        var x = 0.5f;
        var y = 0.5f;
        return SpawnPuppetAt(
            ctx,
            player_id,
            new DbVector2(x, y),
            ctx.Timestamp
        );
    }

    public static Entity SpawnPuppetAt(ReducerContext ctx, uint player_id, DbVector2 position, SpacetimeDB.Timestamp timestamp)
    {
        var entity = ctx.Db.entity.Insert(new Entity
        {
            position = new DbVector2(0, 0),
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
    static void ClearStates(ReducerContext ctx, Puppet puppet){
        
        puppet.current_states.Clear();
        ctx.Db.puppet.entity_id.Update(puppet);
    }
    static void AddState(ReducerContext ctx,uint player_id, PlayerActions state){
        var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
        puppet.current_states.Add(state);
        ctx.Db.puppet.entity_id.Update(puppet);
    }

    [Table(Name = "delayed_snowball", Scheduled = nameof(SpawnDelayedSnowball), ScheduledAt = nameof(scheduled_at))]
    public partial struct DelayedSnowball
    {
        [PrimaryKey, AutoInc]
        public ulong id;
        public ScheduleAt scheduled_at;
        public uint player_id;
        public DbVector2 position;
    }

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
            AddState(ctx, player_id, PlayerActions.Throw);
        }
    }
    
    [Reducer]
    public static void CraftSnowBall(ReducerContext ctx,uint player_id)
    {
        var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
        puppet.has_snowball = true;
        puppet.locked_until = ctx.Timestamp + TimeSpan.FromMilliseconds(1000);
        ctx.Db.puppet.entity_id.Update(puppet);
        AddState(ctx, player_id, PlayerActions.Craft);
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
    
    static void CreateSnowMan(ReducerContext ctx, DbVector2 position, float rotation){
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
        // Create a box ob  stacle (wall)
        CreateSnowMan(ctx, new DbVector2(0, 0), 0);
        CreateSnowMan(ctx, new DbVector2(0, 10), 0);
        CreateSnowMan(ctx, new DbVector2(0, 20), 0);
        CreateSnowMan(ctx, new DbVector2(0, 30), 0);
 
        
        
        
      //  // Create another box obstacle
      //  var boxEntity = ctx.Db.entity.Insert(new Entity
      //  {
      //      position = new DbVector2(0, 15),
      //      rotation = 45 // Rotated 45 degrees
      //  });
      //  
      //  ctx.Db.obstacle.Insert(new Obstacle
      //  {
      //      entity_id = boxEntity.entity_id,
      //      obstacle_id = "barricade_1",
      //      collider_type = ColliderType.Box,
      //      box_size = new DbVector2(3, 3),
      //      sphere_radius = 0 // Not used for box collider
      //  });
    }
}