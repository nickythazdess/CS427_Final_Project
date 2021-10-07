using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class FullLane : Obstacle
{
    public override IEnumerator Spawn(Track track, float t) {
        Vector3 pos;
		Quaternion rot;
		track.GetPoint(t, out pos, out rot);
        AsyncOperationHandle asset = Addressables.InstantiateAsync(gameObject.name, pos, rot);
        yield return asset;
        if (asset.Result == null || !(asset.Result is GameObject)) yield break;

        GameObject obj = asset.Result as GameObject;
        obj.transform.SetParent(track.objectRoot, true);
    }
}
