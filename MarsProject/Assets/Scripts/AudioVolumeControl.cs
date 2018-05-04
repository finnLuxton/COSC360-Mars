using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeControl : MonoBehaviour {
    AudioSource musicAudio, sfxAudio;
    Slider musicSlider, sfxSlider;
    Toggle musicToggler, sfxToggler;

    void Start() {
        
    }

    public void MusicValueChanged() {
        musicAudio.volume = musicSlider.value;
    }

    public void SFXValueChanged() {
        sfxAudio.volume = sfxSlider.value;
    }

    public void ToggleMusic() {
        musicAudio.mute = !musicToggler.isOn;
    }

    public void ToggleSFX() {
        sfxAudio.mute = !sfxToggler.isOn;
    }
}
