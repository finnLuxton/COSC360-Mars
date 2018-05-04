using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	// Select an audio source to play too
	public AudioSource efxSource;
	public AudioSource musicScore;

	// Link it gameObject SoundManager
	public static SoundManager instance = null;

	// Have variable pitch for audio
	public float lowPitchRange = 0.95f;
	public float highPitchRange = 1.05f;

	// On scene initialization
	void Awake (){
		// Create an instance of SoundManager. If one already exists, replace with new one.
		if(instance == null){
			instance = this;
		}else if(instance != this){
			Destroy(gameObject);
		}
		DontDestroyOnLoad (gameObject);
	}

	// Plays a single audio clip parsed to it.
	public void PlaySingle (AudioClip clip) {
		efxSource.clip = clip;
		efxSource.Play();
	}

	// Play an array of audio clips at different pitch levels
	public void RandomizeSfx (params AudioClip [] clips){
		int randomIndex = Random.Range(0, clips.Length);
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);

		efxSource.pitch = randomPitch;
		efxSource.clip = clips[randomIndex];
		efxSource.Play();
	}
}
