using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;

/// <summary>
/// Defines powerups in game.
/// </summary>

public abstract class Powerup : MonoBehaviour
{
    public float duration;
    public Sprite icon;
	public AudioClip activatedSound;

    public enum PowerupType {
        MAGNET,
        INVINCIBILITY,
        EXTRALIFE
    }
    
    public AssetReference activeParticleRef;
    public bool canBeSpawned = true;

    public bool active {  get { return isActive; } }
    public float timeActive {  get { return sinceStart; } }

    protected bool isActive = true;
    protected float sinceStart;
    protected ParticleSystem spawnedParticle;

    public abstract PowerupType GetPowerupType();
    public abstract string GetPowerupName();

    public void ResetTime() => sinceStart = 0;

    protected void Release(GameObject obj, float time) {
        StartCoroutine(ReleaseTimer(obj, time));
    }
    //Timer to release particle systems
    IEnumerator ReleaseTimer(GameObject obj, float time) {
        yield return new WaitForSeconds(time);
        Addressables.ReleaseInstance(obj);
    }

    public virtual IEnumerator Started(PlayerController c) {
        sinceStart = 0;

		if (activatedSound != null) {
			c.powerupSource.clip = activatedSound;
			c.powerupSource.Play();
		}

        if(activeParticleRef != null) {
            var op = activeParticleRef.InstantiateAsync();
            yield return op;
            if (op.Result == null || !(op.Result is GameObject)) yield break;
            spawnedParticle = op.Result.GetComponent<ParticleSystem>();
            if (!spawnedParticle.main.loop)
                Release(spawnedParticle.gameObject, spawnedParticle.main.duration);
            spawnedParticle.transform.SetParent(c.playerCollider.transform);
            spawnedParticle.transform.localPosition = op.Result.transform.position;
        }
	}
    //This will be attached to GameScene.cs Update()
    public virtual void Tick(PlayerController c) {
        sinceStart += Time.deltaTime;
        if (sinceStart >= duration) {
            isActive = false;
            return;
        }
    }

    public virtual void Ended(PlayerController c) {
        if (spawnedParticle != null && spawnedParticle.main.loop)
            Addressables.ReleaseInstance(spawnedParticle.gameObject);

        if (activatedSound != null && c.powerupSource.clip == activatedSound)
            c.powerupSource.Stop();

        for (int i = 0; i < c.activePowerups.Count; ++i) {
            if (c.activePowerups[i].active && c.activePowerups[i].activatedSound != null) {
                c.powerupSource.clip = c.activePowerups[i].activatedSound;
                c.powerupSource.Play();
            }
        }
    }
}
