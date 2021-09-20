using System;
using UnityEngine;

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
