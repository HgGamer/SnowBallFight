using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class SnowBallController : EntityController
{
   private PlayerController Owner;
	public AudioSource hitSound;
    public void Spawn(SnowBall SnowBall, PlayerController owner)
    {
        base.Spawn(SnowBall.EntityId);
		transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        this.Owner = owner;
        //GetComponentInChildren<TMPro.TextMeshProUGUI>().text = owner.Username;
    }
    IEnumerator OnHitSomething(){
		hitSound.Play();
		
		transform.GetChild(0).gameObject.SetActive(false);
		GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds(hitSound.clip.length);
		
		base.OnDelete(null);
	}
	public override void OnDelete(EventContext context)
	{	
	
		StartCoroutine(OnHitSomething());
	}

    public override void OnEntityUpdated(Entity newVal)
	{
//		Debug.Log($"OnEntityUpdated  {newVal.Position.X} , {newVal.Position.Y} , {transform.name}");
		LerpTime = 0.0f;
		LerpStartPosition = transform.position;
		
		var newvector = (Vector2)newVal.Position;
		LerpTargetPosition = new Vector3(newvector.x, 0.849f, newvector.y);
		LerpStartRotation = transform.rotation.eulerAngles;
		LerpTargetRotation = new Vector3(0, newVal.Rotation, 0);
		
	}
}
