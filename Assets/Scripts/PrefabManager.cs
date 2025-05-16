using SpacetimeDB.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Obstacle Prefab", menuName = "SnowFight/Obstacle Prefab", order = 1)]
public class ObstaclePrefabData : ScriptableObject
{
    public string obstacleId;
    public GameObject prefab;
}

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager Instance;

	public PuppetController PuppetPrefab;
	
	public PlayerController PlayerPrefab;
	public SnowBallController SnowBallPrefab;

	[SerializeField]
	private List<ObstaclePrefabData> obstaclePrefabs = new List<ObstaclePrefabData>();
	

	
    private void Awake()
	{
		Instance = this;
	}

    public static PuppetController SpawnPuppet(Puppet puppet, PlayerController owner)
	{
		Debug.Log($"SpawnPuppet: {puppet.EntityId}");
		var entityController = Instantiate(Instance.PuppetPrefab);
		entityController.name = $"Puppet - {puppet.EntityId}";
		entityController.Spawn(puppet, owner);
		owner.OnPuppetSpawned(entityController);
		return entityController;
	}

    public static PlayerController SpawnPlayer(Player player)
	{
		Debug.Log($"SpawnPlayer: {player.Identity}");
		var playerController = Instantiate(Instance.PlayerPrefab);
		playerController.name = $"PlayerController - {player.Name}";
		playerController.Initialize(player);
		return playerController;
	}
	 public static SnowBallController SpawnSnowball(SnowBall snowball, PlayerController owner)
	{
		var sbc = Instantiate(Instance.SnowBallPrefab);
		sbc.name = $"SnowballController ";
		sbc.Spawn(snowball, owner);
		return sbc;
	}

	public static void SpawnObstacle(Obstacle obstacle, Entity entity){
		
		var prefabData = Instance.obstaclePrefabs.FirstOrDefault(p => p != null && p.obstacleId == obstacle.ObstacleId);
		
		if (prefabData != null && prefabData.prefab != null)
		{
		    var obstacleObject = Instantiate(prefabData.prefab);
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