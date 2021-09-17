using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Config")]
    public TrackManager trackManager;
	public CharacterInfo character;
    public int maxLife = 3;

    [Header("Controls")]
	public float jumpHeight = 1.2f;
    public float jumpLength = 2.0f;
	public float slideLength = 2.0f;
    public float laneChangeSpeed = 1.0f;

    [Header("Sounds")]
	public AudioClip powerUpUseSound;
    public AudioClip coinSound;
	public AudioClip premiumSound;

    [HideInInspector]
	public List<GameObject> magnetCoins = new List<GameObject>();

    public int currentLife { get { return m_CurrentLife; } set { m_CurrentLife = value; } }
    public AudioSource powerupSource { get { return m_audio; } }
    public CapsuleCollider characterCollider { get { return m_characterCollider; } }
    public Animator characterAnimator { get { return m_characterAnimator; } }
	public List<Powerup> powerups { get { return activePowerup; } }

    protected int m_CurrentLife;
    protected AudioSource m_audio;
    protected List<Powerup> activePowerup = new List<Powerup>();
    
    protected bool invincible;
	protected bool running;
    protected bool jumping;
    protected bool sliding;
	
    protected float jumpPoint;
	protected float slidePoint;

    protected int currentLane;
    static int s_BlinkingHash;

    protected Vector3 targetPosition;
    protected CapsuleCollider m_characterCollider;
    protected Animator m_characterAnimator;

    static int s_HitHash = Animator.StringToHash("Hit");
    static int s_DeadHash = Animator.StringToHash("Dead");
	static int s_RunStartHash = Animator.StringToHash("runStart");
	static int s_MovingHash = Animator.StringToHash("Moving");
	static int s_JumpingHash = Animator.StringToHash("Jumping");
	static int s_JumpingSpeedHash = Animator.StringToHash("JumpSpeed");
	static int s_SlidingHash = Animator.StringToHash("Sliding");

    public void Begin() {
        m_characterCollider = GetComponent<CapsuleCollider>();
        m_characterAnimator = character.GetComponent<Animator>();
        m_audio = GetComponent<AudioSource>();
        s_BlinkingHash = Shader.PropertyToID("_BlinkingValue");

        jumping = false;
        sliding = false;
        invincible = false;
        jumpPoint = 0.0f;
        slidePoint = 0.0f;
        currentLane = 1;
        m_CurrentLife = maxLife;
        targetPosition = Vector3.zero;
    }

    public void End() {
        for (int i = 0; i < activePowerup.Count; ++i)
        {
            activePowerup[i].Ended(this);
            Addressables.ReleaseInstance(activePowerup[i].gameObject);
        }
        activePowerup.Clear();
    }

    public void Init() {
        running = false;
        m_characterAnimator.SetBool(s_DeadHash, false);
        activePowerup.Clear();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) StartJumping();
		else if (Input.GetKeyDown(KeyCode.DownArrow)) StartSliding();
        
        Vector3 verticalTargetPosition = targetPosition;

        for(int i = 0; i < magnetCoins.Count; ++i)
            magnetCoins[i].transform.position = Vector3.MoveTowards(magnetCoins[i].transform.position, 
            transform.position, 10f * Time.deltaTime);
    }

    public void ChangeLane(int direction) {
		if (!running) return;
        int targetLane = currentLane + direction;
        currentLane = (targetLane < 0 || targetLane > 2) ? currentLane : targetLane;
        targetPosition = new Vector3((currentLane - 1) * trackManager.laneOffset, 0f, 0f);
    }

    public void StartRunning() {
        running = true;
        m_characterAnimator.Play(s_RunStartHash);
        m_characterAnimator.SetBool(s_MovingHash, true);
    }

    public void StopRunning() {
        running = false;
        m_characterAnimator.SetBool(s_MovingHash, false);
        trackManager.StopMoving();
    }

    public void StartJumping() {
	    if (!running) return;
        if (!jumping) {
			if (sliding) StopSliding();

			float correctJumpLength = jumpLength * (1f + trackManager.speedRatio);
			jumpPoint = trackManager.distance;
            float animSpeed = 0.6f * (trackManager.speed / correctJumpLength);

            m_characterAnimator.SetFloat(s_JumpingSpeedHash, animSpeed);
            m_characterAnimator.SetBool(s_JumpingHash, true);
			m_audio.PlayOneShot(character.jumpSound);
			jumping = true;
        }
    }

    public void StopJumping() {
        if (jumping) {
            m_characterAnimator.SetBool(s_JumpingHash, false);
            jumping = false;
        }
    }

	public void StartSliding() {
		if (!running) return;
		if (!sliding) {
		    if (jumping) StopJumping();

            float correctSlideLength = slideLength * (1f + trackManager.speedRatio); 
			slidePoint = trackManager.distance;
            float animSpeed = 0.6f * (trackManager.speed / correctSlideLength);

			m_characterAnimator.SetFloat(s_JumpingSpeedHash, animSpeed);
			m_characterAnimator.SetBool(s_SlidingHash, true);
			m_audio.PlayOneShot(character.slideSound);
			sliding = true;

			characterCollider.direction = 2; // Z-axis
		}
	}

	public void StopSliding() {
		if (sliding) {
			m_characterAnimator.SetBool(s_SlidingHash, false);
			sliding = false;

			characterCollider.direction = 1; // Y-axis
		}
	}

    protected void ObtainCoin(Collider c) {
        if (magnetCoins.Contains(c.gameObject)) magnetCoins.Remove(c.gameObject);
        Coin coin = c.GetComponent<Coin>();
        if (coin != null) {
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
        StopRunning();

        c.enabled = false;
        Obstacle obstacle = c.gameObject.GetComponent<Obstacle>();

        if (obstacle != null) obstacle.Impacted();
        else Addressables.ReleaseInstance(c.gameObject);

        m_characterAnimator.SetTrigger(s_HitHash);
        currentLife -= 1;

        if (currentLife > 0) {
            m_audio.PlayOneShot(character.hitSound);
            StartCoroutine(SetInvincible());
        } else {
            m_audio.PlayOneShot(character.deathSound);
            m_characterAnimator.SetBool(s_DeadHash, true);
        }
    }

    protected void ObtainPowerup(Collider c) {
        if (magnetCoins.Contains(c.gameObject)) magnetCoins.Remove(c.gameObject);
        Powerup powerup = c.GetComponent<Powerup>();
        if (powerup != null) {
            UsePowerup(powerup);
            trackManager.score += 50;
        }
    }

    protected void OnTriggerEnter(Collider c) {
        switch (c.gameObject.layer) {
            case 6: ObtainCoin(c); break;
            case 7: HitObstacle(c); break;
            case 8: ObtainPowerup(c); break;
            default: break;
        }
    }

    public void UsePowerup(Powerup p) {
		m_audio.PlayOneShot(powerUpUseSound);
        for(int i = 0; i < activePowerup.Count; ++i) {
            if(activePowerup[i].GetPowerupType() == p.GetPowerupType()) {
                activePowerup[i].ResetTime();
                Addressables.ReleaseInstance(p.gameObject);
                return;
            }
        }

        p.transform.SetParent(transform, false);
        p.gameObject.SetActive(false);
        activePowerup.Add(p);
        StartCoroutine(p.Started(this));
    }

    public IEnumerator SetInvincible(float period = 2f)
    {
        invincible = true;

		float time = 0;
		float blink = 1f;
		float previousBlink = 0f;

		while(time < period && invincible)
		{
			Shader.SetGlobalFloat(s_BlinkingHash, blink);
            yield return null;

			time += Time.deltaTime;
			previousBlink += Time.deltaTime;
			if (previousBlink > 0.2f)
			{
				previousBlink = 0;
				blink = 1f - blink;
			}
        }

		Shader.SetGlobalFloat(s_BlinkingHash, 0f);

		invincible = false;
    }
}
