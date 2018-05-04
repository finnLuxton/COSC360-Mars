using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoType : MonoBehaviour {

	[System.Serializable]
	public class PauseData {
		public char character;
		public float time;

		public PauseData(char character, float time) {
			this.character = character;
			this.time = time;
		}
	}

    // Allows user to set pause amount between each letter appearing
	public float letterPause = 0.02f;
	public PauseData[] pauseCharacters = new PauseData[1];


	// Message displayed, set in Text gameObjects Textbox
	string message;
	// Holder for Message
	Text textComp;

	// Use this for initialization
	void Start() {
		// On initialization, get text and parse it to TypeText method
		textComp = GetComponent<Text>();
		message = textComp.text;
		textComp.text = "";
		StartCoroutine(TypeText());
	}

	IEnumerator TypeText () {
		// Repeat for each letter in the message
		foreach (char letter in message.ToCharArray()) {
			// Print letter of text
			textComp.text += letter;

			//If a specified character, wait the defined amount
			foreach(PauseData p in pauseCharacters) {
				if(letter == p.character) {
					yield return new WaitForSeconds(p.time);
				}
			}

			//Wait the defined amoutn per character
			yield return new WaitForSeconds(letterPause);
		}
	}
}