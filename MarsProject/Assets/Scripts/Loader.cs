using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	// Link Loader to a SoundManager gameObject
	public GameObject soundManager;

	// On scene initilization
	void Awake () {
		// If there is no SoundManager, instantiate a new one.
		if (soundManager && SoundManager.instance == null)
			Instantiate(soundManager);
	}
}