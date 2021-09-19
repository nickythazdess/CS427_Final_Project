using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameScene : MonoBehaviour
{
    public Canvas canvas;
    public TrackManager trackManager;
	public AudioClip gameTheme;

    [Header("UI")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI premiumText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI newScoreRecord;
	public TextMeshProUGUI distanceText;
    public TextMeshProUGUI newDistanceRecord;
    public RectTransform powerupZone;
	public RectTransform lifeZone;

    public Button pauseButton;
	public RectTransform pausePanel;
	public RectTransform HUD;
    public GameObject gameOverPopup;

    [Header("Prefabs")]
    public GameObject PowerupIconPrefab;

    protected List<Image> hearts;
    protected List<PowerupIcon> activePowerupIcons = new List<PowerupIcon>();
    protected bool finished;
    protected bool wasMoving;
    protected bool newBestScore;
    protected bool newBestDistance;


    void Start() {
        PlayerData.Create();
        hearts = new List<Image>();
        for (int i = 0; i < trackManager.playerController.maxLife; i++)
            hearts.Add(lifeZone.GetChild(i).GetComponent<Image>());
        finished = false;
        newBestScore = false;
        newBestDistance = false;
        wasMoving = false;
        activePowerupIcons.Clear();
        countdownText.gameObject.SetActive(true);
        StartCoroutine(trackManager.Begin());
    }

    void Update() {
        if (finished) return;

        if (trackManager.loaded) {
            PlayerController playerController = trackManager.playerController;

            if (playerController.currentLife <= 0) {
                pauseButton.gameObject.SetActive(false);
                playerController.End();
                StartCoroutine(WaitForGameOver());
            }

            List<Powerup> expiredPowerups = new List<Powerup>();
            List<PowerupIcon> expiredPowerupIcons = new List<PowerupIcon>();

            foreach (Powerup powerup in playerController.activePowerups) {
                PowerupIcon icon = null;
                foreach (PowerupIcon tempIcon in activePowerupIcons) {
                    if (tempIcon.linkedPowerup == powerup) {
                        icon = tempIcon;
                        break;
                    }
                }

                powerup.Tick(playerController);
                if (!powerup.active) {
                    expiredPowerups.Add(powerup);
                    expiredPowerupIcons.Add(icon);
                } else if (icon == null) {
                    GameObject obj = Instantiate(PowerupIconPrefab);
                    icon = obj.GetComponent<PowerupIcon>();

                    icon.linkedPowerup = powerup;
                    icon.transform.SetParent(powerupZone, false);
                    activePowerupIcons.Add(icon);
                }
            }

            foreach (Powerup powerup in expiredPowerups)
            {
                powerup.Ended(playerController);
                Addressables.ReleaseInstance(powerup.gameObject);
                playerController.activePowerups.Remove(powerup);
            }

            foreach (PowerupIcon icon in expiredPowerupIcons)
            {
                if (icon != null) Destroy(icon.gameObject);
                activePowerupIcons.Remove(icon);
            }

            coinText.text = trackManager.coins.ToString();
            premiumText.text = trackManager.premium.ToString();

            int score = Mathf.FloorToInt(trackManager.score);
            if (!newBestScore && score > PlayerData.instance.bestScore) {
                newScoreRecord.gameObject.SetActive(true);
                newBestScore = true;
            }
            scoreText.text = score.ToString();

            int distance = Mathf.FloorToInt(trackManager.distance);
            if (!newBestDistance && distance > PlayerData.instance.bestDistance) {
                newDistanceRecord.gameObject.SetActive(true);
                newBestDistance = true;
            }
            distanceText.text = distance.ToString() + "m";

            for (int i = 0; i < hearts.Count; i++)
                if (i < playerController.currentLife)
                    hearts[i].color = Color.white;
                else hearts[i].color = Color.black;

            if (trackManager.timeToStart >= 0) {
                countdownText.text = Mathf.Ceil(trackManager.timeToStart).ToString();
            } else countdownText.gameObject.SetActive(false);
        }
    }

    public void Pause() {
        if (finished) return;

		Time.timeScale = 0;
        AudioListener.pause = true;
		HUD.gameObject.SetActive(false);
        pausePanel.gameObject.SetActive(true);
		wasMoving = trackManager.isMoving;
		trackManager.StopMoving();
	}

	public void Resume() {
        Time.timeScale = 1.0f;
        AudioListener.pause = false;
        HUD.gameObject.SetActive(true);
		pausePanel.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        if (wasMoving) trackManager.Wait(5f, false);
	}

    public void CleanPowerupIcons() {
        for (int i = 0; i < activePowerupIcons.Count; ++i)
            if (activePowerupIcons[i] != null) Destroy(activePowerupIcons[i].gameObject);

        activePowerupIcons.Clear();
        trackManager.playerController.powerupSource.Stop();
    }

    public void Restart()  { //In game-over panel
		Time.timeScale = 1.0f;
        LevelLoader.LoadLevel("MainGame");
	}

    public void BackToMainMenu()  { //In game-over panel
		Time.timeScale = 1.0f;
		trackManager.End();
        LevelLoader.LoadLevel("MainMenu");
	}

    public void QuitToMainMenu() { //In pause panel
		Time.timeScale = 1.0f;
		AudioListener.pause = false;
		trackManager.End();
        CleanPowerupIcons();
        LevelLoader.LoadLevel("MainMenu");
	}

    IEnumerator WaitForGameOver() {
		finished = true;
		trackManager.StopMoving();
        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        PlayerData.instance.coins += trackManager.coins;
        PlayerData.instance.premium += trackManager.premium;
        if (newBestScore) PlayerData.instance.bestScore = Mathf.FloorToInt(trackManager.score);
        if (newBestDistance) PlayerData.instance.bestDistance = Mathf.FloorToInt(trackManager.distance);
        PlayerData.instance.Save();

        yield return new WaitForSeconds(2.0f);
        gameOverPopup.SetActive(true);
        CleanPowerupIcons();
	}

    void OnApplicationPause(bool pause) {
		if (pause) Pause();
	}

    void OnApplicationFocus(bool focus) {
        //if (!focus) Pause();
    }
}
