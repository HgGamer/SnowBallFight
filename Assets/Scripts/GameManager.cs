using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    const string SERVER_URL = "http://127.0.0.1:3000";
    const string MODULE_NAME = "snowfight";

    public static event Action OnConnected;
    public static event Action OnSubscriptionApplied;


    public static GameManager Instance { get; private set; }
    public static Identity LocalIdentity { get; private set; }
    public static DbConnection Conn { get; private set; }
    public static Dictionary<uint, EntityController> Entities = new Dictionary<uint, EntityController>();
    public static Dictionary<uint, PlayerController> Players = new Dictionary<uint, PlayerController>();
    public static Dictionary<uint, SnowBallController> Snowballs = new Dictionary<uint, SnowBallController>();
    private void Start()
    {
        Instance = this;
        Application.targetFrameRate = 60;

        // In order to build a connection to SpacetimeDB we need to register
        // our callbacks and specify a SpacetimeDB server URI and module name.
        var builder = DbConnection.Builder()
            .OnConnect(HandleConnect)
            .OnConnectError(HandleConnectError)
            .OnDisconnect(HandleDisconnect)
            .WithUri(SERVER_URL)
            .WithModuleName(MODULE_NAME);

        // If the user has a SpacetimeDB auth token stored in the Unity PlayerPrefs,
        // we can use it to authenticate the connection.
        if (AuthToken.Token != "")
        {
            builder = builder.WithToken(null); //AuthToken.Token
        }

        // Building the connection will establish a connection to the SpacetimeDB
        // server.
        Conn = builder.Build();
    }

    // Called when we connect to SpacetimeDB and receive our client identity
    void HandleConnect(DbConnection conn, Identity identity, string token)
    {
        Debug.Log("Connected.");
        AuthToken.SaveToken(token);
        LocalIdentity = identity;




        conn.Db.Puppet.OnInsert += PuppetOnInsert;
        conn.Db.Puppet.OnUpdate += PuppetOnUpdate;
        conn.Db.Entity.OnUpdate += EntityOnUpdate;

        conn.Db.Entity.OnDelete += EntityOnDelete;
        conn.Db.Player.OnInsert += PlayerOnInsert;
        conn.Db.Player.OnDelete += PlayerOnDelete;

        conn.Db.Snowball.OnInsert += SnowballOnInsert;
        conn.Db.Snowball.OnDelete += SnowballOnDelete;

        OnConnected?.Invoke();

        // Request all tables
        Conn.SubscriptionBuilder()
            .OnApplied(HandleSubscriptionApplied)
            .SubscribeToAllTables();

    }

    void HandleConnectError(Exception ex)
    {
        Debug.LogError($"Connection error: {ex}");
    }

    void HandleDisconnect(DbConnection _conn, Exception ex)
    {
        Debug.Log("Disconnected.");
        if (ex != null)
        {
            Debug.LogException(ex);
        }
    }

    private void HandleSubscriptionApplied(SubscriptionEventContext ctx)
    {
        Debug.Log("Subscription applied!");
        OnSubscriptionApplied?.Invoke();
        ctx.Reducers.EnterGame("Username");
      
    }

    public static bool IsConnected()
    {
        return Conn != null && Conn.IsActive;
    }

    public void Disconnect()
    {
        Conn.Disconnect();
        Conn = null;
    }
    public static void GetObstacles()
    {
        Debug.Log("Getting obstacles");
        foreach (var obstacle in Conn.Db.Obstacle.Iter())
        {
            Debug.Log($"Obstacle: {obstacle.EntityId}");
            var entity = Conn.Db.Entity.EntityId.Find(obstacle.EntityId);
            PrefabManager.SpawnObstacle(obstacle, entity);
        }

    }


    private static void SnowballOnDelete(EventContext context, SnowBall deletedValue)
    {
        if (Snowballs.Remove(deletedValue.EntityId, out var snowballController))
        {
            GameObject.Destroy(snowballController.gameObject);
        }
        Entities.Remove(deletedValue.EntityId);
    }
    private static void SnowballOnInsert(EventContext context, SnowBall insertedValue)
    {
        var player = GetOrCreatePlayer(insertedValue.PlayerId);
        var snowballController = PrefabManager.SpawnSnowball(insertedValue, player);
        Snowballs.Add(insertedValue.EntityId, snowballController);
        Entities.Add(insertedValue.EntityId, snowballController);
    }

    private static void PuppetOnInsert(EventContext context, Puppet insertedValue)
    {
        var player = GetOrCreatePlayer(insertedValue.PlayerId);
        var entityController = PrefabManager.SpawnPuppet(insertedValue, player);
        Entities.Add(insertedValue.EntityId, entityController);
    }
    private static void PuppetOnUpdate(EventContext context, Puppet oldValue, Puppet newValue)
    {
        //find the puppet in the entities dictionary
        if (Entities.TryGetValue(newValue.EntityId, out var entityController))
        {
            ((PuppetController)entityController).OnEntityUpdated(newValue);
        }


    }
    private static void EntityOnUpdate(EventContext context, Entity oldEntity, Entity newEntity)
    {
        if (!Entities.TryGetValue(newEntity.EntityId, out var entityController))
        {
            return;
        }
        entityController.OnEntityUpdated(newEntity);
    }
    private static void EntityOnDelete(EventContext context, Entity oldEntity)
    {
        if (Entities.Remove(oldEntity.EntityId, out var entityController))
        {
            entityController.OnDelete(context);
        }
    }
    private static void PlayerOnInsert(EventContext context, Player insertedPlayer)
    {
        GetOrCreatePlayer(insertedPlayer.PlayerId);
    }
    private static void PlayerOnDelete(EventContext context, Player deletedvalue)
    {
        if (Players.Remove(deletedvalue.PlayerId, out var playerController))
        {
            GameObject.Destroy(playerController.gameObject);
        }
    }
    private static PlayerController GetOrCreatePlayer(uint playerId)
    {
        if (!Players.TryGetValue(playerId, out var playerController))
        {
            var player = Conn.Db.Player.PlayerId.Find(playerId);
            playerController = PrefabManager.SpawnPlayer(player);
            Players.Add(playerId, playerController);
        }

        return playerController;
    }
    public static void SpawnSnowBall(uint playerId, DbVector2 position)
    {
        Debug.Log($"SpawnSnowBall: {playerId}, {position}");
        Conn.Reducers.ThrowSnowBall(playerId, position);
    }

    public static void CraftSnowBall(uint playerId)
    {
        Conn.Reducers.CraftSnowBall(playerId);
    }
}
