using UnityEngine;
using System;

public class Magnet : Powerup
{
    public override string GetPowerupName() => "Magnet";

    public override PowerupType GetPowerupType() => PowerupType.MAGNET;

	protected Collider[] returnColls = new Collider[20];

	public override void Tick(PlayerController c)
    {
        base.Tick(c);
        PlayerCollider coll = c.playerCollider;
        int totalCollider = Physics.OverlapBoxNonAlloc(coll.transform.position, 
        new Vector3(5.0f, 1.0f, 1.0f), returnColls, coll.transform.rotation, 1 << 6);

        for(int i = 0; i < totalCollider; i++)
        {
			if (returnColls[i] != null && !coll.magnetCoins.Contains(returnColls[i].gameObject))
			{
				returnColls[i].transform.SetParent(coll.transform);
				coll.magnetCoins.Add(returnColls[i].gameObject);
			}
		}
    }
}
