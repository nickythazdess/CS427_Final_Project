using UnityEngine;

public class CountDown : MonoBehaviour
{
    protected AudioSource m_audio;
	protected float period;
	
	void OnEnable() {
		m_audio = GetComponent<AudioSource>();
		period = m_audio.clip.length;
        m_audio.PlayDelayed(0.7f);
	}

	void Update() {
		period -= Time.deltaTime;
        if (period < 0) gameObject.SetActive(false);
	}
}
