using UnityEngine;
using System;
using System.Collections;

public class ExtraLife : Powerup
{
    public override string GetPowerupName() => "ExtraLife";

    public override PowerupType GetPowerupType() => PowerupType.EXTRALIFE;

    // Can't be used when max health
    public override IEnumerator Started(PlayerController c)
    {
        yield return base.Started(c);
        if (c.currentLife < c.maxLife) c.currentLife += 1;
    }
}
