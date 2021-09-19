using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    public TrackManager trackManager;
	public CharacterInfo character;
    public PlayerCollider playerCollider;
    public GameObject shadow;
    public int maxLife = 3;
    
    [Header("Controls")]
	public float jumpHeight = 1.2f;
    public float jumpLength = 6.0f;
	public float slideLength = 7.0f;
    public float laneChangeSpeed = 14.0f;

    [Header("Sounds")]
	public AudioClip powerUpUseSound;

    public int currentLife { get { return m_CurrentLife; } set { m_CurrentLife = value; } }
    public AudioSource powerupSource { get { return m_audio; } }
    public Animator characterAnimator { get { return m_characterAnimator; } }
	public List<Powerup> activePowerups { get { return m_activePowerup; } }

    protected int m_CurrentLife;
    protected AudioSource m_audio;
    protected List<Powerup> m_activePowerup = new List<Powerup>();
    
	protected bool running;
    protected bool jumping;
    protected bool sliding;
	
    protected float jumpPoint;
	protected float slidePoint;

    protected int currentLane;

    protected Vector3 targetPosition;
    protected Animator m_characterAnimator;

	static int s_RunStartHash = Animator.StringToHash("runStart");
	static int s_MovingHash = Animator.StringToHash("Moving");
	static int s_JumpingHash = Animator.StringToHash("Jumping");
	static int s_JumpingSpeedHash = Animator.StringToHash("JumpSpeed");
	static int s_SlidingHash = Animator.StringToHash("Sliding");

    public void Begin() {
        playerCollider.character = character;
        m_characterAnimator = character.GetComponent<Animator>();
        m_audio = GetComponent<AudioSource>();
        m_activePowerup.Clear();
        
        running = false;
        jumping = false;
        sliding = false;
        jumpPoint = 0.0f;
        slidePoint = 0.0f;
        currentLane = 1;
        m_CurrentLife = maxLife;
        targetPosition = Vector3.zero;
    }

    public void End() {
        for (int i = 0; i < m_activePowerup.Count; ++i)
        {
            m_activePowerup[i].Ended(this);
            Addressables.ReleaseInstance(m_activePowerup[i].gameObject);
        }
        m_activePowerup.Clear();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) StartJumping();
		else if (Input.GetKeyDown(KeyCode.DownArrow)) StartSliding();
        
        Vector3 verticalTargetPosition = targetPosition;

        if (sliding) {
			float correctSlideLength = slideLength * (1.0f + trackManager.speedRatio);
			float ratio = (trackManager.distance - slidePoint) / correctSlideLength;
			if (ratio >= 1.0f) StopSliding();
		}

        if (jumping) {
			if (trackManager.isMoving) {
				float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
				float ratio = (trackManager.distance - jumpPoint) / correctJumpLength;
				if (ratio >= 1.0f) StopJumping();
				else verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
			} else if(!AudioListener.pause) {
			    verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, 80f * Time.deltaTime);
				if (Mathf.Approximately(verticalTargetPosition.y, 0f)) StopJumping();
			}
        }
        playerCollider.transform.localPosition = Vector3.MoveTowards(playerCollider.transform.localPosition,
            verticalTargetPosition, laneChangeSpeed * Time.deltaTime);

        RaycastHit hit;
        if(Physics.Raycast(playerCollider.transform.position + Vector3.up, Vector3.down, out hit, 100f, 1<<7))
            shadow.transform.position = hit.point + Vector3.up * 0.1f;
        else {
            Vector3 shadowPosition = playerCollider.transform.position;
            shadowPosition.y = 0.1f;
            shadow.transform.position = shadowPosition;
        }
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
            playerCollider.Jump(true);
			float correctJumpLength = jumpLength * (1f + trackManager.speedRatio);
			jumpPoint = trackManager.distance;
            float animSpeed = 0.5f * (trackManager.speed / correctJumpLength);

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
            playerCollider.Jump(false);
        }
    }

	public void StartSliding() {
		if (!running) return;
		if (!sliding) {
		    if (jumping) StopJumping();
            playerCollider.Slide(true);

            float correctSlideLength = slideLength * (1f + trackManager.speedRatio); 
			slidePoint = trackManager.distance;
            float animSpeed = 0.5f * (trackManager.speed / correctSlideLength);

			m_characterAnimator.SetFloat(s_JumpingSpeedHash, animSpeed);
			m_characterAnimator.SetBool(s_SlidingHash, true);
			m_audio.PlayOneShot(character.slideSound);
			sliding = true;
		}
	}

	public void StopSliding() {
		if (sliding) {
			m_characterAnimator.SetBool(s_SlidingHash, false);
			sliding = false;
            playerCollider.Slide(false);
		}
	}

    public void UsePowerup(Powerup p) {
		m_audio.PlayOneShot(powerUpUseSound);
        for(int i = 0; i < m_activePowerup.Count; ++i) {
            if(m_activePowerup[i].GetPowerupType() == p.GetPowerupType()) {
                m_activePowerup[i].ResetTime();
                Addressables.ReleaseInstance(p.gameObject);
                return;
            }
        }

        p.transform.SetParent(transform, false);
        p.gameObject.SetActive(false);
        m_activePowerup.Add(p);
        StartCoroutine(p.Started(this));
    }
}
