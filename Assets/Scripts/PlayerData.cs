using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Save data locally for the game.
/// </summary>
public class PlayerData
{
    static protected PlayerData m_Instance;
    static public PlayerData instance { get { return m_Instance; } }

    protected string saveFile = "";

    public int coins;
    public int premium;
    public int bestScore;
    public int bestDistance;
    public int usedCharacter;  // Currently selected character.
    public bool tutorialDone;

    // File management
    static public void Create()
    {
		if (m_Instance == null) m_Instance = new PlayerData();
        m_Instance.saveFile = Application.persistentDataPath + "/save.bin";

        if (File.Exists(m_Instance.saveFile)) m_Instance.Read();
        else NewSave();
    }

	static public void NewSave()
	{
		m_Instance.usedCharacter = 0;
        m_Instance.coins = 0;
        m_Instance.premium = 0;
        m_Instance.bestScore = 0;
        m_Instance.bestDistance = 0;
        m_Instance.tutorialDone = false;
		m_Instance.Save();
	}

    public void Read()
    {
        BinaryReader r = new BinaryReader(new FileStream(saveFile, FileMode.Open));

        usedCharacter = r.ReadInt32();
        coins = r.ReadInt32();
        premium = r.ReadInt32();
        tutorialDone = r.ReadBoolean();
        bestScore = r.ReadInt32();
        bestDistance = r.ReadInt32();
        r.Close();
    }

    public void Save()
    {
        BinaryWriter w = new BinaryWriter(new FileStream(saveFile, FileMode.OpenOrCreate));

        w.Write(coins);
        w.Write(usedCharacter);
        w.Write(premium);
		w.Write(bestScore);
        w.Write(bestDistance);
        w.Write(tutorialDone);
        w.Close();
    }
}