using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class Pregame : MonoBehaviour
{
    [Header("Character")]
    public Text characterNameText;
    public Transform characterPosition;
    public CharacterDictionary characterDictionary;

    [Header("UI")]
    public Button runButton;
    public Text bestScoreText;
    public Text bestDistanceText;
    public Text fishboneText;
    public Text premiumText;

    [Header("SFX")]
    public AudioClip buttonClick;
    public AudioClip runButtonClick;

    protected bool loading;
    protected CharacterInfo curentCharacter;
    protected AudioSource audioSource;

    void Start() {
        PlayerData.Create();
        audioSource = GetComponent<AudioSource>();
        loading = false;
        ChangeCharacter(0);
        bestScoreText.text = "Best score: " +  PlayerData.instance.bestScore.ToString();
        bestDistanceText.text = "Best distance: " +  PlayerData.instance.bestDistance.ToString() + "m";
        fishboneText.text = PlayerData.instance.coins.ToString();
        premiumText.text = PlayerData.instance.premium.ToString();
    }

    public void ChangeCharacter(int value) {
        audioSource.PlayOneShot(buttonClick);
        PlayerData.instance.usedCharacter += value;
        if (PlayerData.instance.usedCharacter > 1) PlayerData.instance.usedCharacter = 0;
        else if (PlayerData.instance.usedCharacter < 0) PlayerData.instance.usedCharacter = 1;
        PlayerData.instance.Save();
        StartCoroutine(PopulateCharacters());
    }

    public void StartGame() {
        LevelLoader.LoadLevel("MainGame");
        audioSource.PlayOneShot(runButtonClick);
    }

    void Update() {
        if (loading) {
            runButton.interactable = false;
            runButton.GetComponentInChildren<Text>().text = "Loading...";
        } else {
            runButton.interactable = true;
            runButton.GetComponentInChildren<Text>().text = "RUN";
        }
        if(curentCharacter != null)
            curentCharacter.transform.Rotate(0, 4f * Time.deltaTime, 0, Space.Self);
    }

    public IEnumerator PopulateCharacters() {
        loading = true;
        string charName = characterDictionary.characters[PlayerData.instance.usedCharacter].characterName;

        var characterInfo = Addressables.InstantiateAsync(charName, Vector3.zero, Quaternion.identity);
        yield return characterInfo;
        if (characterInfo.Result == null || !(characterInfo.Result is GameObject)) yield break;
        CharacterInfo character = characterInfo.Result.GetComponent<CharacterInfo>();
        
        character.transform.SetParent(characterPosition, false);
        character.transform.rotation =  Quaternion.Euler (0f, 180f, 0f);

        if (curentCharacter != null) Addressables.ReleaseInstance(curentCharacter.gameObject);
        curentCharacter = character;
        characterNameText.text = charName;
        curentCharacter.transform.localPosition = new Vector3(0f, -100f, 0f);
        curentCharacter.transform.localScale = new Vector3(150f, 150f, 150f);
        loading = false;
	}


}
