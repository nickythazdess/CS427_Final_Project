using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;

public class TrackSegment : MonoBehaviour
{
    public Transform pathParent;
    public TrackManager manager;

	public Transform objectRoot;
	public Transform collectibleTransform;

    public AssetReference[] possibleObstacles; 

    [HideInInspector]
    public float[] obstaclePositions;

    public float worldLength { get { return m_WorldLength; } }

    protected float m_WorldLength;

    public void GetPointAt(float t, out Vector3 pos, out Quaternion rot)
    {
        float clampedT = Mathf.Clamp01(t);
        float scaledT = (pathParent.childCount - 1) * clampedT;
        int index = Mathf.FloorToInt(scaledT);
        float segmentT = scaledT - index;

        Transform orig = pathParent.GetChild(index);
        if (index == pathParent.childCount - 1)
        {
            pos = orig.position;
            rot = orig.rotation;
            return;
        }

        Transform target = pathParent.GetChild(index + 1);

        pos = Vector3.Lerp(orig.position, target.position, segmentT);
        rot = Quaternion.Lerp(orig.rotation, target.rotation, segmentT);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
