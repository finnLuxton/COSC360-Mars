using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour {
	
	void Start () {
		Text gameoverText = GameObject.FindWithTag("gameover_text").GetComponent<Text>();
		Text gameoverTitle = GameObject.FindWithTag("lose_title").GetComponent<Text>();
		gameoverText.text = GameMaster.GameOverText;
		gameoverTitle.text = GameMaster.GameOverTitle;
	}
	
}
