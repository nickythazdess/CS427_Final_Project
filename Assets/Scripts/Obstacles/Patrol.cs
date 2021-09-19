using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

public class Patrol : Obstacle
{
    public float minTime = 2f;
    public float maxTime = 4f;

    protected Animator animator;
    protected AudioSource m_audio;
    protected Track track;
    protected Vector3 originalPos = Vector3.zero;
    protected bool moving = false;
	protected float m_speed;
	protected float currentPos;

    public override void Setup() {
		originalPos = transform.localPosition + transform.right * track.manager.laneOffset;
		transform.localPosition = originalPos;

		float actualTime = Random.Range(minTime, maxTime);
        m_speed = (track.manager.laneOffset * 4) / actualTime;

        animator = GetComponent<Animator>();
		if (animator != null)
            animator.SetFloat("SpeedRatio", animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / actualTime);

	    moving = true;
	}

    public override IEnumerator Spawn(Track _track, float t) {
		Vector3 pos;
		Quaternion rot;
		_track.GetPoint(t, out pos, out rot);
        var asset = Addressables.InstantiateAsync(obstacleName, pos, rot);
        yield return asset;
        if (asset.Result == null || !(asset.Result is GameObject)) yield break;

        GameObject obj = asset.Result as GameObject;
        obj.transform.SetParent(_track.objectRoot, true);
        obj.transform.localPosition = Vector3.zero;

        Patrol po = obj.GetComponent<Patrol>();
        po.track = _track;
        po.Setup();
    }

	void Update() {
		if (!moving) return;

		currentPos += m_speed * Time.deltaTime;
        transform.localPosition = originalPos - transform.right * Mathf.PingPong(currentPos, track.manager.laneOffset * 2);
	}

    public override void Impact() {
	    moving = false;
		base.Impact();

		if (animator != null) animator.SetTrigger("Dead");
	}
}
