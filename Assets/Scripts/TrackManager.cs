using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TrackManager : MonoBehaviour
{
    static public TrackManager instance { get { return s_Instance; } }
    static protected TrackManager s_Instance;

    [Header("Character & Movements")]
    public PlayerController playerController;
    public float acceleration = 0.2f;
    public float minSpeed = 5.0f;
    public float maxSpeed = 30.0f;
    public float laneOffset = 1.0f;

    [Header("Map")]
    public Map map;

    [Header("Dictionaries")]
    public PowerupDictionary powerupsDictionary;
    public CharacterDictionary characterDictionary;

    [Header("Assets")]
    public GameObject coinPrefab;
    public AssetReference premiumRef;

    public bool loaded { get; set; }
    public bool isMoving { get { return moving; } }

    public int coins { get { return m_coinAccumulator; } set { m_coinAccumulator = value; } }
    public int premium { get { return m_premiumAccumulator; } set { m_premiumAccumulator = value; } }
    public int currentZone { get { return m_currentZone; } }

    public float score { get { return m_scoreAccumulator; } set { m_scoreAccumulator = value; } }
    public float distance { get { return m_distanceAccumulator; } set { m_distanceAccumulator = value; } }
    public float speed { get { return m_speed; } }
    public float speedRatio { get { return (m_speed - minSpeed) / (maxSpeed - minSpeed); } }
    public float timeToStart { get { return m_timeToStart; } set { m_timeToStart = value; } }
    
    public Track currentTrack { get { return m_tracks[0]; } }
    public List<Track> tracks { get { return m_tracks; } }

    protected bool moving;

    protected int m_coinAccumulator;
    protected int m_premiumAccumulator;
    protected int m_currentZone;
    protected int m_beginSafeTracks;

    protected float m_scoreAccumulator;
    protected float m_distanceAccumulator;
    protected float m_currentTrackDistance;
    protected float m_currentZoneDistance;
    protected float m_speed;
    protected float m_timeToStart;
    protected float m_timeSinceLastPowerup;
    protected float m_timeSinceLastPremium;

    protected List<Track> m_tracks = new List<Track>();
    protected List<Track> m_pastTracks = new List<Track>();
    protected AudioSource m_audio;

    private int currentSpawnedTrack = 0;

    void Start() {
        s_Instance = this;
        m_audio = GetComponent<AudioSource>();
    }
    //Make the track and the player start moving
    public void StartMoving(bool start = true) {
        playerController.StartRunning();
        moving = true;
        if (start) m_speed = minSpeed;
    }

    public void StopMoving() {
        moving = false;
    }
    //public version of WaitToStart
    public void Wait(float period, bool start = true) {
        StartCoroutine(WaitToStart(period, start));
    }

    IEnumerator WaitToStart(float timer = 5f, bool start = true) {
        playerController.NoRunning();
        playerController.characterAnimator.Play("Start");
        m_timeToStart = timer;

        while (m_timeToStart >= 0) {
            yield return null;
            m_timeToStart -= Time.deltaTime * 1.5f;
        }
        m_timeToStart = -1;
        StartMoving(start);
    }
    //Spawn the player, setup variables
    public IEnumerator Begin() {
        m_coinAccumulator = 0;
        m_premiumAccumulator = 0;
        m_scoreAccumulator = 0;
        m_distanceAccumulator = 0;
        m_currentZone = 0;
        m_currentZoneDistance = 0;
        m_beginSafeTracks = 1;
        
        string charName = characterDictionary.characters[PlayerData.instance.usedCharacter].characterName;
        var characterInfo = Addressables.InstantiateAsync(charName, Vector3.zero, Quaternion.identity);
        yield return characterInfo;
        if (characterInfo.Result == null || !(characterInfo.Result is GameObject)) yield break;
        CharacterInfo character = characterInfo.Result.GetComponent<CharacterInfo>();
        playerController.character = character;

        character.transform.SetParent(playerController.playerCollider.transform, false);
        Camera.main.transform.SetParent(playerController.transform, true);
        
        RenderSettings.fog = true;
        RenderSettings.fogColor = map.fogColor;
        Coin.coinPool = new Pooler(coinPrefab, 255);
        playerController.Begin();
        StartCoroutine(WaitToStart());
        m_audio.Play();
        loaded = true;
    }
    //Clean up when game ends
    public void End() {
        foreach (Track track in m_tracks) Addressables.ReleaseInstance(track.gameObject);
        foreach (Track track in m_pastTracks) Addressables.ReleaseInstance(track.gameObject);
        m_tracks.Clear();
        m_pastTracks.Clear();

        playerController.End();
        Addressables.ReleaseInstance(playerController.character.gameObject);
        playerController.character = null;

    }
    //Update track's speed and distance, continuously swpaning and removing tracks  
    void Update() {
        while (currentSpawnedTrack < 10) {
            StartCoroutine(SpawnNewTrack());
            currentSpawnedTrack++;
        }
        if (!moving) return;

        float scaledSpeed = m_speed * Time.deltaTime;
        m_scoreAccumulator += scaledSpeed * 0.1f;
        m_distanceAccumulator += scaledSpeed;
        m_currentZoneDistance += scaledSpeed;
        m_currentTrackDistance += scaledSpeed;

        if (m_currentTrackDistance > m_tracks[0].worldLength) {
            m_currentTrackDistance -= m_tracks[0].worldLength;

            m_pastTracks.Add(m_tracks[0]);
            m_tracks.RemoveAt(0);
            currentSpawnedTrack--;
        }

        Vector3 currentPos;
        Quaternion currentRot;
        m_tracks[0].GetWorldPoint(m_currentTrackDistance, out currentPos, out currentRot);

        if (currentPos.magnitude > 100f) {
            foreach (Track track in m_tracks) track.transform.position -= currentPos;
            foreach (Track pastTrack in m_pastTracks) pastTrack.transform.position -= currentPos;
            m_tracks[0].GetWorldPoint(m_currentTrackDistance, out currentPos, out currentRot);
        }

        playerController.transform.position = currentPos;
        playerController.transform.rotation = currentRot;

        for (int i = 0; i < m_pastTracks.Count; ++i)
            if ((m_pastTracks[i].transform.position - currentPos).z < -30f) {
                m_pastTracks[i].Cleanup();
                m_pastTracks.RemoveAt(i);
                i--;
            }

        if (m_speed < maxSpeed) m_speed += acceleration * Time.deltaTime;
        else m_speed = maxSpeed;

        m_timeSinceLastPowerup += Time.deltaTime;
        m_timeSinceLastPremium += Time.deltaTime;
    }

    public IEnumerator SpawnNewTrack() {
        if (map.zones[m_currentZone].length < m_currentZoneDistance) {
            m_currentZone += 1;
            if (m_currentZone >= map.zones.Length) m_currentZone = 0;
            m_currentZoneDistance = 0;
        }

        int numOfTracks = map.zones[m_currentZone].prefabList.Length;
        int trackUse = Random.Range(0, numOfTracks);

        AsyncOperationHandle trackToUse = map.zones[m_currentZone].prefabList[trackUse]
            .InstantiateAsync(new Vector3(0, 0, -100f), Quaternion.identity);
        yield return trackToUse;
        if (trackToUse.Result == null || !(trackToUse.Result is GameObject)) yield break;
        Track newTrack = (trackToUse.Result as GameObject).GetComponent<Track>();
        newTrack.manager = this;

        Vector3 latestExitPos;
        Quaternion latestExitRot;
        if (m_tracks.Count > 0) m_tracks[m_tracks.Count - 1].GetPoint(1.0f, out latestExitPos, out latestExitRot);
        else {
            latestExitPos = transform.position;
            latestExitRot = transform.rotation;
        }

        newTrack.transform.rotation = latestExitRot;

        Vector3 entryPos;
        Quaternion entryRot;
        newTrack.GetPoint(0.0f, out entryPos, out entryRot);

        newTrack.transform.position = latestExitPos + (newTrack.transform.position - entryPos);
        newTrack.transform.localScale = new Vector3((Random.value > 0.5f ? -1 : 1), 1, 1);
        newTrack.objectRoot.localScale = new Vector3(1.0f / newTrack.transform.localScale.x, 1, 1);

        if (m_beginSafeTracks <= 0) SpawnObstacle(newTrack);
        else m_beginSafeTracks -= 1;
        m_tracks.Add(newTrack);
    }
    //Spawning obstacles on the tracks
    public void SpawnObstacle(Track track) {
        int obsCount = track.possibleObstacles.Length;
        if (obsCount > 0) foreach(float pos in track.obstaclePositions)
            StartCoroutine(track.possibleObstacles[Random.Range(0, obsCount)].Spawn(track, pos));
        StartCoroutine(SpawnCoinAndPowerup(track));
    }
    
    //Spawning coins and powerup on the tracks
    public IEnumerator SpawnCoinAndPowerup(Track track) {
        const float increment = 1.5f;
        float currentWorldPos = 0.0f;
        int currentLane = Random.Range(0, 3);

        float powerupChance = Mathf.Clamp01(Mathf.Floor(m_timeSinceLastPowerup) * 0.0005f);
        float premiumChance = Mathf.Clamp01(Mathf.Floor(m_timeSinceLastPremium) * 0.00005f);

        while (currentWorldPos < track.worldLength) {
            Vector3 pos;
            Quaternion rot;
            track.GetWorldPoint(currentWorldPos, out pos, out rot);

            bool hasValidLane = true;
            int checkingLane = currentLane;
            while (Physics.CheckSphere(pos + ((checkingLane - 1) * laneOffset * (rot * Vector3.right)), 0.4f, 1 << 7)) {
                checkingLane = (checkingLane + 1) % 3;
                if (currentLane == checkingLane) {
                    hasValidLane = false;
                    break;
                }
            }

            currentLane = checkingLane;

            if (hasValidLane) {
                pos = pos + ((currentLane - 1) * laneOffset * (rot * Vector3.right));

                GameObject toUse = null;
                if (Random.value < powerupChance) {
                    int picked = Random.Range(0, powerupsDictionary.powerups.Length);

                    if (powerupsDictionary.powerups[picked].canBeSpawned) {
                        m_timeSinceLastPowerup = 0f;

                        AsyncOperationHandle asset = Addressables.InstantiateAsync(powerupsDictionary.powerups[picked].gameObject.name, pos, rot);
                        yield return asset;
                        if (asset.Result == null || !(asset.Result is GameObject)) yield break;
                        
                        toUse = asset.Result as GameObject;
                        toUse.transform.SetParent(track.transform, true);
                    }
                } else if (Random.value < premiumChance) {
                    m_timeSinceLastPremium = 0.0f;

                    AsyncOperationHandle asset = Addressables.InstantiateAsync(premiumRef, pos, rot);
                    yield return asset;
                    if (asset.Result == null || !(asset.Result is GameObject)) yield break;
                    
                    toUse = asset.Result as GameObject;
                    toUse.transform.SetParent(track.transform, true);
                } else {
                    toUse = Coin.coinPool.Get(pos, rot);
                    toUse.transform.SetParent(track.coinTransform, true);
                }
            }

            currentWorldPos += increment;
        }
    }
}
