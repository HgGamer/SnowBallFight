using System;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class PuppetController : EntityController
{
   private PlayerController Owner;

    public void Spawn(Puppet puppet, PlayerController owner)
    {
        base.Spawn(puppet.EntityId);
		

        this.Owner = owner;
        //GetComponentInChildren<TMPro.TextMeshProUGUI>().text = owner.Username;
    }

	public override void OnDelete(EventContext context)
	{
		base.OnDelete(context);
        Owner.OnPuppetDeleted(this);
	}


}
