using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

public class Patrol : Obstacle
{
    public float minTime = 2f;
    public float maxTime = 4f;

    protected Animator animator;
    protected Track spawnedTrack;
    protected Vector3 originalPos = Vector3.zero;
    protected bool moving = false;
	protected float m_speed;
	protected float currentPos;

    public override void Setup() {
		originalPos = transform.localPosition - transform.forward * spawnedTrack.manager.laneOffset;
		transform.localPosition = originalPos;

		float actualTime = Random.Range(minTime, maxTime);
        m_speed = (spawnedTrack.manager.laneOffset * 4f) / actualTime;

        animator = GetComponent<Animator>();
		if (animator != null)
            animator.SetFloat(Animator.StringToHash("SpeedRatio"),
                animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / actualTime);

	    moving = true;
	}

    public override IEnumerator Spawn(Track track, float t) {
		Vector3 pos;
		Quaternion rot;
		track.GetPoint(t, out pos, out rot);
        AsyncOperationHandle asset = Addressables.InstantiateAsync(gameObject.name, pos, rot);
        yield return asset;
        if (asset.Result == null || !(asset.Result is GameObject)) yield break;

        GameObject obj = asset.Result as GameObject;
        obj.transform.SetParent(track.objectRoot, true);
        obj.transform.rotation = new Quaternion(0f, 1f, 0f, 1f);

        Patrol patrol = obj.GetComponent<Patrol>();
        patrol.spawnedTrack = track;
        
        patrol.Setup();
    }

	void Update() {
		if (!moving) return;

		currentPos += m_speed * Time.deltaTime;
        transform.localPosition = originalPos + transform.forward * Mathf.PingPong(currentPos, spawnedTrack.manager.laneOffset * 2f);
	}

    public override void Impact() {
	    moving = false;
		base.Impact();
		if (animator != null) animator.SetTrigger("Dead");
	}
}
