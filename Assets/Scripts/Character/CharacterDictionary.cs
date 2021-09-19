using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Character", menuName = "MeoMeo/Character Dictionary")]
public class CharacterDictionary : ScriptableObject
{
    public CharacterInfo[] characters;

    static protected Dictionary<string, CharacterInfo> dict;

    public void Load() {
        if (dict == null) {
            dict = new Dictionary<string, CharacterInfo>();

            for (int i = 0; i < characters.Length; ++i)
                dict.Add(characters[i].characterName, characters[i]);
        }
    }

    static public CharacterInfo GetCharacterByName(string name) {
        CharacterInfo c;
        return dict.TryGetValue(name, out c) ? c : null;
    }
}