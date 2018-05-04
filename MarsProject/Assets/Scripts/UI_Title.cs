using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UI_Title : MonoBehaviour {

	static private Text diffText;

	// Use this for initialization
	void Start () {
		diffText = GameObject.FindWithTag("difficulty_name").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
