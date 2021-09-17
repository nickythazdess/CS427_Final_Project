using UnityEngine;
using System;
using System.Collections;

public class ExtraLife : Powerup
{
    public override string GetPowerupName() => "ExtraLife";

    public override PowerupType GetPowerupType() => PowerupType.EXTRALIFE;

    public override bool CanBeUsed(Character c) 
        => c.currentLife == c.maxLife ? false : true;

    // If collected when max health, award coins instead
    public override IEnumerator Started(Character c)
    {
        yield return base.Started(c);
        if (c.currentLife < c.maxLife) c.currentLife += 1;
    }
}
