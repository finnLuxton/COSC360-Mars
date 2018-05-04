using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Event : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Update the event screen
		GameObject nextButton = GameObject.FindWithTag("nextButton");
		SpriteRenderer eventArt = GameObject.FindWithTag("eventArt").GetComponent<SpriteRenderer>();
		Text eventText = GameObject.FindWithTag("eventText").GetComponent<Text>();
		Text eventHex = GameObject.FindWithTag("event_hex").GetComponent<Text>();

		Text[] supplyDifferenceText =  {
			GameObject.FindWithTag("alcohol_event").GetComponent<Text>(),
			GameObject.FindWithTag("fuel_event").GetComponent<Text>(),
			GameObject.FindWithTag("oxygen_event").GetComponent<Text>(),
			GameObject.FindWithTag("rations_event").GetComponent<Text>()
		};
		
		eventArt.sprite = (GameMaster.EventImage.Equals("NONE")) ? null : Resources.Load(GameMaster.EventImage, typeof(Sprite)) as Sprite;
		eventText.text = GameMaster.EventText;
		eventHex.text = GameMaster.EventID;

		float[] supplyDifference = History.GetResourceDifference().ToArray();
		for(int i = 0; i < 4; i++) {
			supplyDifferenceText[i].text = supplyDifference[i].ToString();
			if(supplyDifference[i] != 0f) supplyDifferenceText[i].color = (supplyDifference[i] > 0f) ? new Color32(30, 240, 110, 255) : new Color32(255, 73, 73, 255);
		}

		nextButton.SetActive(!GameMaster.HasActions());
	}

	public void LoadNextEvent(){
		GameMaster.SelectAction(0);
	}
}
