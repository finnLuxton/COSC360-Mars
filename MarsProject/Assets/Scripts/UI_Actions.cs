using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_Actions : MonoBehaviour {

	private int actionNumber;
	private Text[] predictedCostText;
	private Button commitButton;

	// Use this for initialization
	void Start () {
		// Used as a checker for the commit button, using OnCommit() and SetActionNumber()
		actionNumber = -1;
		//Var for the commit button
		commitButton = GameObject.FindWithTag("commitBtn").GetComponent<Button>();
		
		// Creates vars from game objects
		// Vars for the 4 choice buttons
		Text[] actionText = new Text[] {
			GameObject.FindWithTag("actionBtn_1").GetComponent<Text>(),
			GameObject.FindWithTag("actionBtn_2").GetComponent<Text>(),
			GameObject.FindWithTag("actionBtn_3").GetComponent<Text>(),
			GameObject.FindWithTag("actionBtn_4").GetComponent<Text>()
		};
		// Vars for resource text counters
		Text[] resourceNumStrings = new Text[] {
			GameObject.FindWithTag("counter_alcohol").GetComponent<Text>(),
			GameObject.FindWithTag("counter_fuel").GetComponent<Text>(),
			GameObject.FindWithTag("counter_oxygen").GetComponent<Text>(),
			GameObject.FindWithTag("counter_rations").GetComponent<Text>()
		};
		predictedCostText = new Text[] {
			GameObject.FindWithTag("alcohol_cost").GetComponent<Text>(),
			GameObject.FindWithTag("fuel_cost").GetComponent<Text>(),
			GameObject.FindWithTag("oxygen_cost").GetComponent<Text>(),
			GameObject.FindWithTag("rations_cost").GetComponent<Text>()
		};
		// Vars for resource fill bars
		Slider[] resourceSliders = new Slider[] {
			GameObject.FindWithTag("bar_alcohol").GetComponent<Slider>(),
			GameObject.FindWithTag("bar_fuel").GetComponent<Slider>(),
			GameObject.FindWithTag("bar_oxygen").GetComponent<Slider>(),
			GameObject.FindWithTag("bar_rations").GetComponent<Slider>()
		};
		// Vars for resource fill colour property bars
		Image[] resourceColours = new Image[] {
			GameObject.FindWithTag("colour_alcohol").GetComponent<Image>(),
			GameObject.FindWithTag("colour_fuel").GetComponent<Image>(),
			GameObject.FindWithTag("colour_oxygen").GetComponent<Image>(),
			GameObject.FindWithTag("colour_rations").GetComponent<Image>()
		};

		//It works exactly the same way as before, but using arrays, b/c seriously?!?
		for(int i = 0; i < 4; i++) {
			//Sets the text for the action buttons
			actionText[i].text = GameMaster.ActionText[i];

			// Sets text for the resource number counters
			resourceNumStrings[i].text = GameMaster.Resources[i].ToString();

			// Fills the resource bars with the percantage of resources currently in possesion
			resourceSliders[i].value = GameMaster.Resources[i] / 100f;

			//Sets the colours for each resource based on the percentage (or disables it)
			if(GameMaster.Resources[i] <= 0f) resourceColours[i].gameObject.SetActive(false);
			else resourceColours[i].color = UI_Bio.GetColor(resourceSliders[i].value);

			//Disables choices you can't afford
			if(!GameMaster.IsSelectable(i)) {
				if(actionText[i].text.Length > 0) actionText[i].text += "\t <color=#ff0000ff><b>INSUFFICENT RESOURCES</b></color>";
				actionText[i].transform.parent.gameObject.GetComponent<Button>().interactable = false;
			}
		}


		// Vars for timeline objects
		// Displays days until storm hits timer
		Text daysRemainingText = GameObject.FindWithTag("eta_num").GetComponent<Text>();
		daysRemainingText.text = (GameMaster.RoverPosition - GameMaster.StormPosition).ToString();

		Slider stormSlider = GameObject.FindWithTag("slider_storm").GetComponent<Slider>();
		Slider roverSlider = GameObject.FindWithTag("slider_rover").GetComponent<Slider>();
		//GameObject closureStorm = GameObject.FindWithTag("fill_stormHit");
		//GameObject handleRover = GameObject.FindWithTag("handle_rover");
		//GameObject handleStorm = GameObject.FindWithTag("handle_storm");
		
		// Moves the storm counter on timeline
		stormSlider.value = GameMaster.StormPosition / GameMaster.ETA;
		// Moves the rover counter on timeline
		roverSlider.value = GameMaster.RoverPosition / GameMaster.ETA;


		//Disable commit button if there are actions available (until an action is clicked)
		commitButton.interactable = !GameMaster.HasActions();
		commitButton.gameObject.GetComponent<Image>().sprite = Resources.Load(((GameMaster.HasActions()) ? "UI_Commit_Button" : "UI_NextEventLrgButton"), typeof (Sprite)) as Sprite;
	}
	

	public void OnCommit(){
		if(!GameMaster.HasActions()) GameMaster.SelectAction(0);
		else if(actionNumber > -1) GameMaster.SelectAction(actionNumber);
	}

	public void SetActionNumber(int input){
		actionNumber = input;
		commitButton.interactable = true;

		float[] predictedCost = GameMaster.ProjectCost(input).ToArray();
		for(int i = 0; i < predictedCost.Length; i++) {
			predictedCostText[i].text = predictedCost[i].ToString();
		}
	}
}
