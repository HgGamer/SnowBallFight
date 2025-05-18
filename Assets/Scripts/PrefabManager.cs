using SpacetimeDB.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance;

	public PuppetController PuppetPrefab;
	
	public PlayerController PlayerPrefab;
	public SnowBallController SnowBallPrefab;

	
	public List<GameObject> obstaclePrefabs = new List<GameObject>();
	
    private void Start()
	{
		Instance = this;
		Debug.Log($"ObstaclePrefabs: {obstaclePrefabs.Count}");
		foreach (var prefab in obstaclePrefabs)
		{
			Debug.Log($"ObstaclePrefab: {prefab.transform.name}");
		}
	}

    public  PuppetController SpawnPuppet(Puppet puppet, PlayerController owner)
	{
		Debug.Log($"SpawnPuppet: {puppet.EntityId}");
		var entityController = Instantiate(PuppetPrefab);
		entityController.name = $"Puppet - {puppet.EntityId}";
		entityController.Spawn(puppet, owner);
		owner.OnPuppetSpawned(entityController);
		return entityController;
	}

    public  PlayerController SpawnPlayer(Player player)
	{
		Debug.Log($"SpawnPlayer: {player.Identity}");
		var playerController = Instantiate(PlayerPrefab);
		playerController.name = $"PlayerController - {player.Name}";
		playerController.Initialize(player);
		return playerController;
	}
	 public  SnowBallController SpawnSnowball(SnowBall snowball, PlayerController owner)
	{
		var sbc = Instantiate(SnowBallPrefab);
		sbc.name = $"SnowballController ";
		sbc.Spawn(snowball, owner);
		return sbc;
	}

	public  void SpawnObstacle(Obstacle obstacle, Entity entity){
		
		var prefabData = obstaclePrefabs.FirstOrDefault(p => p != null && p.transform.name == obstacle.ObstacleId);
		
		if (prefabData != null && prefabData != null)
		{
		    var obstacleObject = Instantiate(prefabData);
		    obstacleObject.name = $"Obstacle - {obstacle.EntityId}";
		    obstacleObject.transform.position = new Vector3(entity.Position.X,0 ,entity.Position.Y);
		    obstacleObject.transform.localEulerAngles = new Vector3(0, entity.Rotation, 0);
		}
		else
		{
		    Debug.LogWarning($"No prefab found for obstacle id: {obstacle.ObstacleId}");
		}
	}
}