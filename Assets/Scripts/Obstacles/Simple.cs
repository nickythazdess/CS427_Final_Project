using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

public class Simple : Obstacle
{
    public override IEnumerator Spawn(Track track, float t)
    {
        int count = Random.Range(1, 3);
        int startLane = Random.Range(-1, 2);

        Vector3 pos;
        Quaternion rot;
        track.GetPoint(t, out pos, out rot);

        for(int i = 0; i < count; i++)
        {
            int lane = startLane + i;
            lane = lane > 1 ? -1 : lane;

            var asset = Addressables.InstantiateAsync(obstacleName, pos, rot);
            yield return asset;
            if (asset.Result == null || !(asset.Result is GameObject)) yield break;

            GameObject obj = asset.Result as GameObject;

            obj.transform.position += obj.transform.right * lane * track.manager.laneOffset;
            obj.transform.SetParent(track.objectRoot, true);
        }
    }
}
