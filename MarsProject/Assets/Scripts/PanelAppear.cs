using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelAppear : MonoBehaviour {

	public GameObject panel;

	void OnMouseOver(){
		panel.SetActive(true);
	}


	void OnMouseExit(){
		panel.SetActive(false);
	}

}
