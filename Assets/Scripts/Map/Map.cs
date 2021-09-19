using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct Zone
{
	public int length;
    public AssetReference[] prefabList;
}

[CreateAssetMenu(fileName ="map", menuName ="MeoMeo/Map")]
public class Map : ScriptableObject
{
	[Header("Zones")]
	public Zone[] zones;

    [Header("Decoration")]
	public GameObject sky;
    public Color fogColor;
}
