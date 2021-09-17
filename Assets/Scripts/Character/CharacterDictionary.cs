using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Store a database of all characters, indexed by name.
/// </summary>
public class CharacterDictionary
{
    static protected List<string> m_characters = new List<string>(); 
    static protected Dictionary<string, CharacterInfo> m_dictionary;

    static public IEnumerator LoadDatabase()
    {
        if (m_dictionary == null)
        {
            m_dictionary = new Dictionary<string, CharacterInfo>();
            yield return Addressables.LoadAssetsAsync<GameObject>("characters", op =>
            {
                CharacterInfo c = op.GetComponent<CharacterInfo>();
                if (c != null)
                {
                    m_dictionary.Add(c.characterName, c);
                    m_characters.Add(c.characterName);
                }
            });
        }
    }

    static public CharacterInfo GetCharacter(int index)
    {
        CharacterInfo c;
        if (m_dictionary == null || !m_dictionary.TryGetValue(m_characters[index], out c)) return null;
        return c;
    }

    static public CharacterInfo GetCharacterByName(string name)
    {
        CharacterInfo c;
        if (m_dictionary == null || !m_dictionary.TryGetValue(name, out c)) return null;
        return c;
    }
}