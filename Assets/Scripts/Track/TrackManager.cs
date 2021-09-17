using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    static public TrackManager instance { get { return s_Instance; } }
    static protected TrackManager s_Instance;

    [Header("Character & Movements")]
    public Character characterController;
    public int speedStep = 3;
    public float minSpeed = 5.0f;
    public float maxSpeed = 30.0f;
    public float laneOffset = 1.0f;
    public bool invincible = false;

    [Header("Dictionaries")]
    public PowerupDictionary powerupsDictionary;

    public int coins { get { return m_coinAccumulator; } set { m_coinAccumulator = value; } }
    public int premium { get { return m_premiumAccumulator; } set { m_premiumAccumulator = value; } }
    public float score { get { return m_scoreAccumulator; } set { m_scoreAccumulator = value; } }
    public float distance { get { return m_distanceAccumulator; } set { m_distanceAccumulator = value; } }
    public float speed { get { return m_speed; } }
    public float speedRatio { get { return (m_speed - minSpeed) / (maxSpeed - minSpeed); } }
    public float timeToStart { get { return m_timeToStart; } }

    protected int m_coinAccumulator;
    protected int m_premiumAccumulator;

    protected float m_scoreAccumulator;
    protected float m_distanceAccumulator;
    protected float m_currentSegDistance;
    protected float m_speed;
    protected float m_timeToStart;

    protected bool moving;


    void Start() {
        m_coinAccumulator = 0;
        m_premiumAccumulator = 0;
        m_scoreAccumulator = 0;
        m_distanceAccumulator = 0;
        s_Instance = this;
    }

    public void StartMoving() {
        characterController.StartRunning();
        moving = true;
        m_speed = minSpeed;
    }

    public void StopMoving() {
        moving = false;
    }

    IEnumerator WaitToStart() {
        characterController.characterAnimator.Play(Animator.StringToHash("Start"));
        m_timeToStart = 5f;

        while (m_timeToStart >= 0) {
            yield return null;
            m_timeToStart -= Time.deltaTime * 1.5f;
        }
        m_timeToStart = -1;
        StartMoving();
    }

    public void Begin() {
        CharacterInfo player = CharacterDictionary
            .GetCharacter(PlayerData.instance.usedCharacter)
            .GetComponent<CharacterInfo>();

        characterController.character = player;
        player.transform.SetParent(characterController.transform, false);
        Camera.main.transform.SetParent(characterController.transform, true);
        //Coin.coinPool = new Pooler(, 255);
        characterController.Begin();
        StartCoroutine(WaitToStart());
    }

    public void End() {
        characterController.End();
        Addressables.ReleaseInstance(characterController.character.gameObject);
        characterController.character = null;
    }

    void Update() {
        if (!moving) return;
        float scaledSpeed = m_speed * Time.deltaTime;
        m_scoreAccumulator += scaledSpeed * 0.1f;

        m_distanceAccumulator += scaledSpeed;
        m_currentSegDistance += scaledSpeed;
    }
}
