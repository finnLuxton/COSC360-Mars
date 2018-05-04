using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugEditor : MonoBehaviour {

	private int errorCounter, COUNTER_MAX = 500;
	private string errorMessage = "", searchEventID = "";
	private string[] resourceNames = { "Alcohol", "Fuel", "Oxygen", "Rations" };
	private string[] resourceValues;
	private string[] hpValues;

	// Use this for initialization
	void Start () {
		LoadCurrent();
	}

	void LoadCurrent() {
		resourceValues = new string[GameMaster.Resources.Length];
		for(int i = 0; i < resourceValues.Length; i++) {
			resourceValues[i] = GameMaster.Resources[i].ToString("F0");
		}

		hpValues = new string[GameMaster.Survivors.Length];
		for(int i = 0; i < hpValues.Length; i++) {
			hpValues[i] = GameMaster.Survivors[i].HP.ToString();
		}
	}

	void ApplyCurrent() {
		float[] r = new float[4];
		int[] hp = new int[6];
		bool invalid = false;

		try {
            r = new float[4] { float.Parse(resourceValues[0]), float.Parse(resourceValues[1]), float.Parse(resourceValues[2]), float.Parse(resourceValues[3]) };
        } catch (FormatException) {
            SetMsg("FormatException: A resource could not be converted into a float!");
            invalid = true;
        } catch (OverflowException) {
            SetMsg("OverflowException: A resource is out of range for a float!");
            invalid = true;
        }

		try {
			hp = new int[6] { int.Parse(hpValues[0]), int.Parse(hpValues[1]), int.Parse(hpValues[2]), int.Parse(hpValues[3]), int.Parse(hpValues[4]), int.Parse(hpValues[5]) };
        } catch (FormatException) {
            SetMsg("FormatException: An HP is not in hexadecimal format!");
            invalid = true;
        } catch (OverflowException) {
            SetMsg("OverflowException: An HP value is out of range for an int!");
            invalid = true;
        }

		if(!invalid) {
			GameMaster.resources = +(new Supplies(r[0], r[1], r[2], r[3]));

			for(int i = 0; i < GameMaster.Survivors.Length; i++) {
				GameMaster.Survivors[i].ModifyHP(hp[i] - GameMaster.Survivors[i].HP, true);
			}

			LoadCurrent();
		}
	}
	
	// Update is called once per frame
	void OnGUI () {
		//Search for an existing event ID
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        if(searchEventID.Equals("")) GUI.Label(new Rect(Screen.width - 150, 10, 75, 20), "EventID");
        searchEventID = GUI.TextField(new Rect(Screen.width - 150, 10, 75, 20), searchEventID).ToUpper();
        if(GUI.Button(new Rect(Screen.width - 70, 10, 60, 20), "Load")) {
            try {
                //Attempts to convert the search id into an int
                int searchTarget = int.Parse(searchEventID, System.Globalization.NumberStyles.HexNumber);

				//Attempt to load the event with the search id
				Event e = EventMaster.GetEvent(searchTarget);

				//Checks that e is actually an event
        		if(e == null) SetMsg("The Event you searched for could not be found!");
				else e.Load();

            } catch (FormatException) {
                SetMsg("FormatException: Search event ID is not in hexadecimal format!");
            } catch (OverflowException) {
                SetMsg("OverflowException: Search event ID is out of range for an int!");
            }
        }
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

		//Formats and shows any error messages
        if(errorMessage.Length > 0 && errorCounter > 0) {
            GUI.color = Color.red;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(10, 10, Screen.width - 160, 20), errorMessage);
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.color = Color.white;
			errorCounter--;
        }

		for (int i = 0; i < 4; i++) {
			GUI.Label(new Rect(100, 100 + (i*25), 100, 20), resourceNames[i]);
			resourceValues[i] = GUI.TextField(new Rect(200, 100 + (i*25), 100, 20), resourceValues[i]);
		}

		for(int i = 0; i < hpValues.Length; i++) {
			GUI.Label(new Rect(400, 100 + (i*25), 100, 20), GameMaster.Survivors[i].GivenName);
			hpValues[i] = GUI.TextField(new Rect(500, 100 + (i*25), 100, 20), hpValues[i]);
		}

		if(GUI.Button(new Rect(Screen.width - 200, Screen.height - 60, 190, 20), "Edit Current Event")) {
			SceneManager.LoadScene("EventEditor");
		}

		if(GUI.Button(new Rect(Screen.width - 200, Screen.height - 30, 90, 20), "Apply")) {
			ApplyCurrent();
			SetMsg("Changes applied.");
		}

		if(GUI.Button(new Rect(Screen.width - 100, Screen.height - 30, 90, 20), "Return")) {
			SceneManager.LoadScene("UI_Screen1");
		}
	}

	private void SetMsg(string msg) {
		errorMessage = msg;
		errorCounter = COUNTER_MAX;
	}
}
