using SpacetimeDB.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager Instance;

	public PuppetController PuppetPrefab;
	
	public PlayerController PlayerPrefab;
	public SnowBallController SnowBallPrefab;
    private void Awake()
	{
		Instance = this;
	}

    public static PuppetController SpawnPuppet(Puppet puppet, PlayerController owner)
	{
		var entityController = Instantiate(Instance.PuppetPrefab);
		entityController.name = $"Puppet - {puppet.EntityId}";
		entityController.Spawn(puppet, owner);
		owner.OnPuppetSpawned(entityController);
		return entityController;
	}

    public static PlayerController SpawnPlayer(Player player)
	{
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
}