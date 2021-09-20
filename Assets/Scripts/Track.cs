using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;

public class Track : MonoBehaviour
{
    public Transform pathParent;
    public TrackManager manager;

	public Transform objectRoot;
	public Transform coinTransform;

    public Obstacle[] possibleObstacles;

    public float[] obstaclePositions;

    public float worldLength { get { return m_WorldLength; } }

    protected float m_WorldLength;

    //Setup
    void OnEnable() {
        m_WorldLength = 0;

        for (int i = 1; i < pathParent.childCount; ++i) {
            Vector3 origin = pathParent.GetChild(i - 1).position;
            Vector3 end = pathParent.GetChild(i).position;
            m_WorldLength += (end - origin).magnitude;
        }
		GameObject obj = new GameObject("ObjectRoot");
		obj.transform.SetParent(transform);
		objectRoot = obj.transform;

		obj = new GameObject("Collect");
		obj.transform.SetParent(objectRoot);
		coinTransform = obj.transform;
    }

    //A track length will be marked from 0..1. Get the position and the rotation
    //at a certain point of a track (0: beginning of the track, 1: end of the track)
	public void GetPoint(float t, out Vector3 pos, out Quaternion rot) {
        float clampedT = Mathf.Clamp01(t);
        float scaledT = (pathParent.childCount - 1) * clampedT;
        int index = Mathf.FloorToInt(scaledT);
        float segmentT = scaledT - index;

        Transform orig = pathParent.GetChild(index);
        if (index == pathParent.childCount - 1) {
            pos = orig.position;
            rot = orig.rotation;
            return;
        }

        Transform target = pathParent.GetChild(index + 1);

        pos = Vector3.Lerp(orig.position, target.position, segmentT);
        rot = Quaternion.Lerp(orig.rotation, target.rotation, segmentT);
    }

    //Similar to get point but in world unit
    public void GetWorldPoint(float wt, out Vector3 pos, out Quaternion rot) {
        float t = wt / m_WorldLength;
        GetPoint(t, out pos, out rot);
    }

    //Clean up when the game ends
	public void Cleanup() {
		while(coinTransform.childCount > 0) {
			Transform t = coinTransform.GetChild(0);
			t.SetParent(null);
            Coin.coinPool.Free(t.gameObject);
		}
	    Addressables.ReleaseInstance(gameObject);
	}
}
