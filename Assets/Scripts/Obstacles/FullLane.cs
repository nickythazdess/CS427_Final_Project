using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using System.Collections;
using UnityEngine;

public class FullLane : Obstacle
{
    public override IEnumerator Spawn(Track track, float t) {
        Vector3 pos;
		Quaternion rot;
		track.GetPoint(t, out pos, out rot);
        var asset = Addressables.InstantiateAsync(obstacleName, pos, rot);
        yield return asset;
        if (asset.Result == null || !(asset.Result is GameObject)) yield break;

        GameObject obj = asset.Result as GameObject;
        obj.transform.SetParent(track.objectRoot, true);
        obj.transform.localPosition = Vector3.zero;
    }
}
