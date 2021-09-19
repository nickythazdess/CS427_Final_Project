using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Powerup", menuName = "MeoMeo/Powerup Dictionary")]
public class PowerupDictionary : ScriptableObject
{
    public Powerup[] powerups;

    static protected Dictionary<Powerup.PowerupType, Powerup> dict;

    public void Load() {
        if (dict == null) {
            dict = new Dictionary<Powerup.PowerupType, Powerup>();

            for (int i = 0; i < powerups.Length; ++i)
                dict.Add(powerups[i].GetPowerupType(), powerups[i]);
        }
    }

    static public Powerup GetByType(Powerup.PowerupType type) {
        Powerup c;
        return dict.TryGetValue(type, out c) ? c : null;
    }
}
