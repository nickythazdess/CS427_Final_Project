using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerCollider : MonoBehaviour
{
    [Header("Config")]
    public TrackManager trackManager;
    public PlayerController controller;
    public CharacterInfo character;

    [Header("Particle")]
    public ParticleSystem hitParticle;
	public ParticleSystem koParticle;
    public ParticleSystem collectParticle;

    [Header("Sounds")]
    public AudioClip coinSound;
	public AudioClip premiumSound;

    [HideInInspector]
	public List<GameObject> magnetCoins = new List<GameObject>();

    public CapsuleCollider col { get { return m_collider; } set { m_collider = value; } }

    protected CapsuleCollider m_collider;
    protected bool invincible;
    protected AudioSource m_audio;

    static int s_HitHash = Animator.StringToHash("Hit");
    static int s_DeadHash = Animator.StringToHash("Dead");

    protected void Start()
    {
		m_collider = GetComponent<CapsuleCollider>();
        m_audio = GetComponent<AudioSource>();
        invincible = false;
	}

    void Update() {
        foreach(GameObject coin in magnetCoins)
            coin.transform.position = Vector3.MoveTowards(coin.transform.position, transform.position, 10f * Time.deltaTime);
    }

    public void Jump(bool jump) {
        if (jump) m_collider.center = new Vector3(0f, 1.1f, 0f);
        else m_collider.center = new Vector3(0f, 0.7f, 0f);
    }

    public void Slide(bool slide) {
        if (slide) {
            m_collider.direction = 2; // Z-axis
            m_collider.center = new Vector3(0f, 0.3f, 0f);
        }
        else {
            m_collider.direction = 1; // Y-axis
            m_collider.center = new Vector3(0f, 0.7f, 0f);
        }
    }

    protected void ObtainCoin(Collider c) {
        GameObject obj = c.gameObject;
        if (magnetCoins.Contains(obj)) magnetCoins.Remove(obj);
        Coin coin = obj.GetComponent<Coin>();
        if (coin != null) {
            collectParticle.Play();
            if (coin.isPremium) {
                Addressables.ReleaseInstance(c.gameObject);
                trackManager.premium += 1;
                trackManager.score += 10;
                m_audio.PlayOneShot(premiumSound);
            } else {
                Coin.coinPool.Free(c.gameObject);
                trackManager.coins += 1;
                trackManager.score += 1;
                m_audio.PlayOneShot(coinSound);
            }
        }
    }

    protected void HitObstacle(Collider c) { 
        if (invincible) return;
        controller.StopRunning();

        c.enabled = false;
        Obstacle obstacle = c.gameObject.GetComponent<Obstacle>();

        if (obstacle != null) obstacle.Impact();
        else Addressables.ReleaseInstance(c.gameObject);

        controller.characterAnimator.SetTrigger(s_HitHash);
        controller.currentLife -= 1;
        hitParticle.Play();
        if (controller.currentLife > 0) {
            m_audio.PlayOneShot(character.hitSound);
            SetInvincible();
            trackManager.Wait(1.5f);
        } else {
            m_audio.PlayOneShot(character.deathSound);
            character.animator.SetBool(s_DeadHash, true);
            koParticle.gameObject.SetActive(true);
        }
    }

    public IEnumerator Delay(float period) {
        yield return new WaitForSeconds(period);
    }

    protected void ObtainPowerup(Collider c) {
        Powerup powerup = c.GetComponent<Powerup>();
        if (powerup != null) {
            collectParticle.Play();
            controller.UsePowerup(powerup);
            trackManager.score += 50;
        }
    }

    void OnTriggerEnter(Collider c) {
        switch (c.gameObject.layer) {
            case 6: ObtainCoin(c); break;
            case 7: HitObstacle(c); break;
            case 8: ObtainPowerup(c); break;
            default: break;
        }
    }

    public void SetInvincible(float timer = 2f) {
        invincible = true;
		StartCoroutine(InvincibleTimer(timer));
	}

    public IEnumerator InvincibleTimer(float period)
    {
		float time = 0;
		float blink = 1f;
		float previousBlink = 0f;

		while(time < period && invincible) {
			Shader.SetGlobalFloat("_BlinkingValue", blink);
            yield return null;
			time += Time.deltaTime;
			previousBlink += Time.deltaTime;
			if (previousBlink > 0.2f) {
				previousBlink = 0;
				blink = 1f - blink;
			}
        }

		Shader.SetGlobalFloat("_BlinkingValue", 0f);
		invincible = false;
    }
}
