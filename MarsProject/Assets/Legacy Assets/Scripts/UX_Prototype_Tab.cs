using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UX_Prototype_Tab : MonoBehaviour {

	private Color enabledColour = new Color32(0,160,170,255);
	private Color disabledColour = new Color32(0,110,120,255);

	private	GameObject eventTab;
	private	GameObject optionsTab;
	private	GameObject logTab;

	// Use this for initialization
	void Start () {
		eventTab = GameObject.Find("Event Tab");
	 	optionsTab = GameObject.Find("Options Tab");
		logTab = GameObject.Find("Log Tab");

		eventTab.GetComponent<Image>().color = enabledColour;
		optionsTab.GetComponent<Image>().color = disabledColour;
		logTab.GetComponent<Image>().color = disabledColour;
	}

	public void ToggleTabs(string name) {
		if (eventTab.name == name) {
			eventTab.GetComponent<Image>().color = enabledColour;
			optionsTab.GetComponent<Image>().color = disabledColour;
			logTab.GetComponent<Image>().color = disabledColour;
		} else if (optionsTab.name == name) {
			optionsTab.GetComponent<Image>().color = enabledColour;
			eventTab.GetComponent<Image>().color = disabledColour;
			logTab.GetComponent<Image>().color = disabledColour;
		} else if (logTab.name == name) {
			logTab.GetComponent<Image>().color = enabledColour;
			eventTab.GetComponent<Image>().color = disabledColour;
			optionsTab.GetComponent<Image>().color = disabledColour;
		}
	}
}
