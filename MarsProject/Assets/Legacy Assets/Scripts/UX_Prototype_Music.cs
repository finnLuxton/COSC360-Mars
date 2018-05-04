using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UX_Prototype_Music : MonoBehaviour {

	private static UX_Prototype_Music instance = null;
	public static UX_Prototype_Music Instance {
		get { return instance; }
	}

	void Awake () {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
			return;
		} else {
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);
	}
}
