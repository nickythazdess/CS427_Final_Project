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

    public enum PowerupType
    {
        MAGNET,
        INVINCIBILITY,
        EXTRALIFE
    }
    
    //public ParticleSystem activeParticle;
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

    //override this to make a consumable not usable (e.g. avoid using ExtraLife when at full health)
    public virtual bool CanBeUsed(Character c) { return true; }

    IEnumerator ReleaseTimer(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Addressables.ReleaseInstance(obj);
    }

    public virtual IEnumerator Started(Character c)
    {
        sinceStart = 0;

		if (activatedSound != null)
		{
			c.powerupSource.clip = activatedSound;
			c.powerupSource.Play();
		}

        if(activeParticleRef != null)
        {
            var op = activeParticleRef.InstantiateAsync();
            yield return op;
            spawnedParticle = op.Result.GetComponent<ParticleSystem>();
            if (!spawnedParticle.main.loop)
                StartCoroutine(ReleaseTimer(spawnedParticle.gameObject, spawnedParticle.main.duration));
            spawnedParticle.transform.SetParent(c.characterCollider.transform);
            spawnedParticle.transform.localPosition = op.Result.transform.position;
        }
	}

    public virtual void Tick(Character c)
    {
        sinceStart += Time.deltaTime;
        if (sinceStart >= duration)
        {
            isActive = false;
            return;
        }
    }

    public virtual void Ended(Character c)
    {
        if (spawnedParticle != null && spawnedParticle.main.loop)
            Addressables.ReleaseInstance(spawnedParticle.gameObject);

        if (activatedSound != null && c.powerupSource.clip == activatedSound)
            c.powerupSource.Stop();

        for (int i = 0; i < c.powerups.Count; ++i)
        {
            if (c.powerups[i].active && c.powerups[i].activatedSound != null)
            {
                c.powerupSource.clip = c.powerups[i].activatedSound;
                c.powerupSource.Play();
            }
        }
    }
}
