using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UX_Prototype_Buttons : MonoBehaviour {

	public AudioSource source;
	public AudioClip hover;
	public AudioClip click;

	public void OnHover() {
		source.PlayOneShot(hover);
	}	

	public void OnClick() {
		source.PlayOneShot(click);
	}	

}
