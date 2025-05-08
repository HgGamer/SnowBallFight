using SpacetimeDB;

public static partial class Module
{

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
            if(new_pos.Magnitude > 100f)
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
        });
        return entity;
    }
     [Reducer]
    public static void ThrowSnowBall(ReducerContext ctx,uint player_id, DbVector2 position)
    {
        SpawnSnowBall(ctx, player_id, position, ctx.Timestamp);
    }

    public static Entity SpawnSnowBall(ReducerContext ctx, uint player_id, DbVector2 position, SpacetimeDB.Timestamp timestamp)
    {
        var entity = ctx.Db.entity.Insert(new Entity
        {
            position = position,
        });
         
        // Find the puppet to get facing direction based on rotation
        var puppet = ctx.Db.puppet.player_id.Filter(player_id).FirstOrDefault();
     
        // Get the puppet's entity to access rotation
        var puppetEntity = ctx.Db.entity.entity_id.Find(puppet.entity_id) ?? throw new Exception("Puppet entity not found");
        
        // Calculate direction vector from rotation (assuming rotation is in degrees around Y axis)
        float rotationRadians = puppetEntity.rotation * (float)Math.PI / 180f;
        DbVector2 direction = new DbVector2(
            (float)Math.Sin(rotationRadians), 
            (float)Math.Cos(rotationRadians)
        );
        
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
    }
}