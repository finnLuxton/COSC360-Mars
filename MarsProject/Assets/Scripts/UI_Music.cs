using UnityEngine;
using System.Collections;

public class UI_Music : MonoBehaviour {

    [SerializeField]
    private AudioClip menuMusic, gameMusic, introMusic, loseMusic, winMusic;

    [SerializeField]
    public AudioSource source;
 
    public static UI_Music instance;

    protected virtual void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(this);
            return;
        }
    }
 
    protected virtual void Start() {
        PlayMenuMusic();
    }

    static public void StopMusic(){
        if( instance != null){
            if( instance.source != null){
                instance.source.Stop();
            }
        }
    }

    static public void PlayMenuMusic (){
        if (instance != null) {
            if (instance.source != null) {
                instance.source.Stop();
                instance.source.clip = instance.menuMusic;
                instance.source.Play();
            }
        } else {
            Debug.Log("Unavailable UI_Music component");
        }
	}
    static public void PlayIntroMusic (){
		if(instance != null) {
			if(instance.source != null) {
				instance.source.Stop();
				instance.source.clip = instance.introMusic;
				instance.source.Play();
			}
		} else {
			Debug.Log("Unavailable UI_Music component");
		}
	}
    static public void PlayGameMusic () {
        if (instance != null) {
            if (instance.source != null) {
                instance.source.Stop();
                instance.source.clip = instance.gameMusic;
                instance.source.Play();
            }
        } else {
            Debug.Log("Unavailable UI_Music component");
        }
    }    static public void PlayLoseMusic () {
        if (instance != null) {
            if (instance.source != null) {
                instance.source.Stop();
                instance.source.clip = instance.loseMusic;
                instance.source.Play();
            }
        } else {
            Debug.Log("Unavailable UI_Music component");
        }
    }
        static public void PlayWinMusic () {
        if (instance != null) {
            if (instance.source != null) {
                instance.source.Stop();
                instance.source.clip = instance.winMusic;
                instance.source.Play();
            }
        } else {
            Debug.Log("Unavailable UI_Music component");
        }
    }
}