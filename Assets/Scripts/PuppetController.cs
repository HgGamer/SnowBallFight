using System;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class PuppetController : EntityController
{
    private PlayerController Owner;
    public Puppet Puppet;
    public void Spawn(Puppet puppet, PlayerController owner)
    {
        Debug.Log($"PuppetController: Spawn: {puppet.EntityId}");
        base.Spawn(puppet.EntityId);
        this.Owner = owner;
        this.Puppet = puppet;
        //GetComponentInChildren<TMPro.TextMeshProUGUI>().text = owner.Username;
    }

    public virtual void OnEntityUpdated(Puppet newVal)
	{
		Debug.Log($"PuppetController: OnEntityUpdated: {newVal.EntityId} , {newVal.PlayerId},  has snowball:  {newVal.HasSnowball}");
        this.Puppet = newVal;
        var states = newVal.CurrentStates;
        foreach(var state in states){
            OnStateChanged(state);
        }
       // OnStateChanged();
    }
    public virtual void OnStateChanged(PlayerActions state){
        Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
    }
	public override void OnDelete(EventContext context)
	{
        Debug.Log($"PuppetController: OnDelete: {EntityId}");
		base.OnDelete(context);
        Owner.OnPuppetDeleted(this);
	}


}
