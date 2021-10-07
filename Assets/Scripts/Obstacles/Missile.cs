using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using UnityEngine;

public class Missile : Obstacle
{
	protected Track spawnedTrack;
	protected AudioSource m_audio;
	protected Animator animator;
    protected bool isReady;
	protected bool isMoving;
	
	public override IEnumerator Spawn(Track track, float t) {
        int lane = Random.Range(-1, 2);

		Vector3 pos;
		Quaternion rot;
		track.GetPoint(t, out pos, out rot);

	    AsyncOperationHandle asset = Addressables.InstantiateAsync(gameObject.name, pos, rot);
		yield return asset;
		if (asset.Result == null || !(asset.Result is GameObject)) yield break;

		GameObject obj = asset.Result as GameObject;
        obj.transform.SetParent(track.objectRoot, true);
        obj.transform.position += obj.transform.right * lane * track.manager.laneOffset;
        obj.transform.forward = -obj.transform.forward;

	    Missile missile = obj.GetComponent<Missile>();
		missile.m_audio = obj.GetComponent<AudioSource>();
		missile.animator = obj.GetComponent<Animator>();
	    missile.spawnedTrack = track;
		missile.isReady = true;
    }

    public override void Impact() {
		base.Impact();
		if (animator != null) animator.SetTrigger("Death");
	}

	public void Update() {
		TrackManager manager = TrackManager.instance;
		if (isReady && manager.isMoving) {
			if (isMoving) transform.position += transform.forward * (1f - manager.speedRatio) * 7 * Time.deltaTime;
			else {
				if (manager.tracks[1] == spawnedTrack) {
					if (animator != null) animator.SetTrigger("Run");
					if (m_audio != null) m_audio.Play();
					isMoving = true;
				}
			}
		}
	}
}
