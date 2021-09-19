using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    public AudioClip buttonClick;

    public void StartGameButton()  {
        LevelLoader.LoadLevel("MainMenu");
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(buttonClick);
	}
}
