using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_NavMarkers : MonoBehaviour {

	private GameObject nm_arrow_left;
	private GameObject nm_arrow_right;
	private GameObject nm_stopper_left;
	private GameObject nm_stopper_right;

	public string sceneName;

	// Use this for initialization
	void Start () {
		nm_arrow_left = GameObject.FindWithTag("navmarker_arrow_left");
		nm_arrow_right = GameObject.FindWithTag("navmarker_arrow_right");
		nm_stopper_left = GameObject.FindWithTag("navmarker_stopper_left");
		nm_stopper_right = GameObject.FindWithTag("navmarker_stopper_right");
	}

	void Update() {
		if(sceneName == "UI_Screen1") {
			nm_arrow_left.GetComponent<Renderer>().enabled = false;
			nm_arrow_right.GetComponent<Renderer>().enabled = true;
			nm_stopper_left.GetComponent<Renderer>().enabled = true;
			nm_stopper_right.GetComponent<Renderer>().enabled = false;
		} else if(sceneName == "UI_Screen2") {
			nm_arrow_left.GetComponent<Renderer>().enabled = true;
			nm_arrow_right.GetComponent<Renderer>().enabled = true;
			nm_stopper_left.GetComponent<Renderer>().enabled = false;
			nm_stopper_right.GetComponent<Renderer>().enabled = false;
		} else if(sceneName == "UI_Screen3") {
			nm_arrow_left.GetComponent<Renderer>().enabled = true;
			nm_arrow_right.GetComponent<Renderer>().enabled = false;
			nm_stopper_left.GetComponent<Renderer>().enabled = false;
			nm_stopper_right.GetComponent<Renderer>().enabled = true;
		}
	}

}
