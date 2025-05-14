using System;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class SnowBallController : EntityController
{
   private PlayerController Owner;

    public void Spawn(SnowBall SnowBall, PlayerController owner)
    {
        base.Spawn(SnowBall.EntityId);
		

        this.Owner = owner;
        //GetComponentInChildren<TMPro.TextMeshProUGUI>().text = owner.Username;
    }
   
	public override void OnDelete(EventContext context)
	{
		base.OnDelete(context);
       
	}

    public override void OnEntityUpdated(Entity newVal)
	{
//		Debug.Log($"OnEntityUpdated  {newVal.Position.X} , {newVal.Position.Y} , {transform.name}");
		LerpTime = 0.0f;
		LerpStartPosition = transform.position;
		
		var newvector = (Vector2)newVal.Position;
		LerpTargetPosition = new Vector3(newvector.x, 0, newvector.y);
		LerpStartRotation = transform.rotation.eulerAngles;
		LerpTargetRotation = new Vector3(0, newVal.Rotation, 0);
		
	}
}
