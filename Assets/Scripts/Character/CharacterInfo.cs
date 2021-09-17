using System;
using UnityEngine;

/// <summary>
/// Data to define a character
/// </summary>

public class CharacterInfo : MonoBehaviour
{
    public string characterName;
    public Animator animator;
    public Sprite avatar;

    [Header("Sound")]
    public AudioClip slideSound;
	public AudioClip jumpSound;
	public AudioClip hitSound;
	public AudioClip deathSound;
}
