using UnityEngine;
using UnityEngine.UI;

public class PowerupIcon : MonoBehaviour
{
    [HideInInspector] public Powerup linkedPowerup;
    public Image icon;
    public Slider slider;

	void Start () { 
        icon.sprite = linkedPowerup.icon;
	}

    void Update() {
        slider.value = 1.0f - linkedPowerup.timeActive / linkedPowerup.duration;
    }
}

