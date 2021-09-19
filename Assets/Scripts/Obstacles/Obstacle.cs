using System.Collections;
using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
	public AudioClip impactSound;

    public virtual void Setup() {}

    public abstract IEnumerator Spawn(Track track, float t);

	public virtual void Impact() {
		Animation impactAnimation = GetComponentInChildren<Animation>();
		AudioSource audioSource = GetComponent<AudioSource>();

		if (impactAnimation != null) impactAnimation.Play();

		if (audioSource != null && impactSound != null) {
			audioSource.Stop();
			audioSource.loop = false;
			audioSource.clip = impactSound;
			audioSource.Play();
		}
	}
}
