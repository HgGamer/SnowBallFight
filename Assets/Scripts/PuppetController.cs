using System;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class PuppetController : EntityController
{
    private Animator animator;
    private PlayerController Owner;
    public Puppet Puppet;
    void Awake(){
        animator = GetComponent<Animator>();
    }
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
        animator.SetFloat("Speed", newVal.Speed);
        foreach(var state in states){
            OnStateChanged(state);
        }
       // OnStateChanged();
    }
    public virtual void OnStateChanged(PlayerActions state){
        switch(state){
            case PlayerActions.Throw:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Throw");
                break;
            case PlayerActions.Hit:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Hit");
                break;
            case PlayerActions.Craft:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Craft");
                break;
            case PlayerActions.Standup:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Standup");
                break;
        }
    }
	public override void OnDelete(EventContext context)
	{
        Debug.Log($"PuppetController: OnDelete: {EntityId}");
		base.OnDelete(context);
        Owner.OnPuppetDeleted(this);
	}


}
