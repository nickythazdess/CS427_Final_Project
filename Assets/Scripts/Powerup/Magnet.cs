using UnityEngine;
using System;

public class Magnet : Powerup
{
    public override string GetPowerupName() => "Magnet";

    public override PowerupType GetPowerupType() => PowerupType.MAGNET;

	protected Collider[] returnColls = new Collider[20];

	public override void Tick(Character c)
    {
        base.Tick(c);

        int totalCollider = Physics.OverlapBoxNonAlloc(c.characterCollider.transform.position, 
        new Vector3(20.0f, 1.0f, 1.0f), returnColls, c.characterCollider.transform.rotation, (1 << 6) | (1 << 8));

        for(int i = 0; i < totalCollider; ++i)
        {
			if (returnColls[i] != null && !c.magnetCoins.Contains(returnColls[i].gameObject))
			{
				returnColls[i].transform.SetParent(c.transform);
				c.magnetCoins.Add(returnColls[i].gameObject);
			}
		}
    }
}
