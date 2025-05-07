using SpacetimeDB.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class EntityController  : MonoBehaviour
{
    const float LERP_DURATION_SEC = 0.1f;

	private static readonly int ShaderColorProperty = Shader.PropertyToID("_Color");

	[DoNotSerialize] public uint EntityId;

	protected float LerpTime;
	protected Vector3 LerpStartPosition;
	protected Vector3 LerpTargetPosition;
	protected Vector3 LerpStartRotation;
	protected Vector3 LerpTargetRotation;
	protected Vector3 TargetScale;

	protected virtual void Spawn(uint entityId)
	{
		EntityId = entityId;

		var entity = GameManager.Conn.Db.Entity.EntityId.Find(entityId);
		LerpStartPosition = LerpTargetPosition = transform.position = new Vector3(entity.Position.X, 0, entity.Position.Y);
		LerpStartRotation = LerpTargetRotation = transform.rotation.eulerAngles;
		transform.localScale = Vector3.one;
		
	}

	

	public virtual void OnEntityUpdated(Entity newVal)
	{
		Debug.Log($"OnEntityUpdated  {newVal.Position.X} , {newVal.Position.Y} , {transform.name}");
		LerpTime = 0.0f;
		LerpStartPosition = transform.position;
		
		var newvector = (Vector2)newVal.Position;
		LerpTargetPosition = new Vector3(newvector.x, 0, newvector.y);
		LerpStartRotation = transform.rotation.eulerAngles;
		LerpTargetRotation = new Vector3(0, newVal.Rotation, 0);
		
	}

	public virtual void OnDelete(EventContext context)
	{
		Destroy(gameObject);
	}

	public virtual void Update()
	{
		// Interpolate position and scale
		LerpTime = Mathf.Min(LerpTime + Time.deltaTime, LERP_DURATION_SEC);
		Vector3 nextPosition = Vector3.Lerp(LerpStartPosition, LerpTargetPosition, LerpTime / LERP_DURATION_SEC);
		
		if (float.IsNaN(nextPosition.x) || float.IsNaN(nextPosition.y) || float.IsNaN(nextPosition.z) ||
			float.IsInfinity(nextPosition.x) || float.IsInfinity(nextPosition.y) || float.IsInfinity(nextPosition.z))
		{
			Debug.LogWarning($"Attempted to set invalid position for '{gameObject.name}': {nextPosition}");
			return;
		}
		transform.position = nextPosition;

		// Lerp rotation using shortest path for Y (yaw)
		float startY = LerpStartRotation.y;
		float endY = LerpTargetRotation.y;
		float deltaY = Mathf.DeltaAngle(startY, endY);
		float lerpedY = startY + deltaY * (LerpTime / LERP_DURATION_SEC);
		Vector3 lerpedEuler = new Vector3(0, lerpedY, 0);
		transform.rotation = Quaternion.Euler(lerpedEuler);
	}

}
