using UnityEngine;
using System;
using System.Collections;

public class Invincible : Powerup
{
    public override string GetPowerupName() => "Invincible";

    public override PowerupType GetPowerupType() => PowerupType.INVINCIBILITY;

    public override IEnumerator Started(Character c)
    {
        yield return base.Started(c);
        StartCoroutine(c.SetInvincible(5f));
    }
}
