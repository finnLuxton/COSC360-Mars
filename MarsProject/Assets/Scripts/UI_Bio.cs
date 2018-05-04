using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Bio : MonoBehaviour {

	private static int numberOfChars = 6;
	private Button charButton, bioButton;
	private Button[] charButtons = new Button[numberOfChars];
	private GameObject bioClose, bioPanel, historyPanel, bioPortrait;
	private Image bioFill;
	private Image[] charFill = new Image[numberOfChars];
	private Text bioName, bioAge, bioHistory, bioText, bioTitleHelp, bioTitleTraits, bioTraits, historyEvent, employeeInfo;
	private Text[] charNames = new Text[numberOfChars];
	private Slider bioHealth;
	private Slider[] charHealth = new Slider[numberOfChars];
	private SpriteRenderer[] charPortraits = new SpriteRenderer[numberOfChars];
 

	// Use this for initialization
	void Start () {
		bioAge = GameObject.FindWithTag("bio_age").GetComponent<Text>();
		bioFill = GameObject.FindGameObjectWithTag("bio_fill").GetComponent<Image>();
		bioHealth = GameObject.FindWithTag("bio_health").GetComponent<Slider>();
		bioName = GameObject.FindWithTag("bio_name").GetComponent<Text>();
		bioPortrait = GameObject.FindWithTag("bio_portrait");
		bioButton = GameObject.FindWithTag("bio_button").GetComponent<Button>();
		bioText = GameObject.FindWithTag("bio_text").GetComponent<Text>();
		bioTraits = GameObject.FindWithTag("bio_traits").GetComponent<Text>();
		historyEvent = GameObject.FindWithTag("history_text").GetComponent<Text>();
		bioTitleTraits = GameObject.FindWithTag("trait_title").GetComponent<Text>();
		bioTitleHelp = GameObject.FindWithTag("trait_helper").GetComponent<Text>();
		bioHistory = GameObject.FindWithTag("bio_history").GetComponent<Text>();
		employeeInfo = GameObject.FindWithTag("employee_info").GetComponent<Text>();
		bioClose = GameObject.FindWithTag("bio_close");
		bioPanel = GameObject.FindWithTag("bio_panel");
		historyPanel = GameObject.FindWithTag("history_panel");

		//On screen load, Enable the history objects, and disable the bio stuff
		historyPanel.SetActive(true);
		bioPanel.SetActive(false);
		bioClose.SetActive(false);

		//Stores references to objects in the GUI in their respective arrays
		for(int i = 0; i < numberOfChars; i++) {
			charNames[i] = GameObject.FindWithTag("char" + (i + 1).ToString() + "_name").GetComponent<Text>();
			charPortraits[i] = GameObject.FindWithTag("char" + (i + 1).ToString() + "_portrait").GetComponent<SpriteRenderer>();
			charButtons[i] = GameObject.FindWithTag("char" + (i + 1).ToString() + "_button").GetComponent<Button>();
			charHealth[i] = GameObject.FindWithTag("char" + (i + 1).ToString() + "_health").GetComponent<Slider>();
			charFill[i] = GameObject.FindWithTag("fill" + (i + 1).ToString()).GetComponent<Image>();
		}

		// Initialise each of the existing survivor names, portraits and health
		for(int i=0; i<GameMaster.Survivors.Length; i++){
			charNames[i].text = GameMaster.Survivors[i].GivenName;
			charHealth[i].value = GameMaster.Survivors[i].HP / 100f;
			charFill[i].color = GetColor(charHealth[i].value);

			if(GameMaster.Survivors[i].IsDead()){
				charButtons[i].interactable = false;
				charPortraits[i].sprite = Resources.Load("dead", typeof(Sprite)) as Sprite;
			}
		}
		string historyText = History.Get();
		if(historyText != "") {
			historyEvent.text = historyText;
		}
		// On first load of the scene, display the bio of the last selected survivor
		// (or the first survivor if first time loading)
		UpdateBio(UI_LoadScene.lastBioLoaded);
	}
	
	// If a survivor portrait button is clicked, update the bio display to match it
	public void UpdateBio(int index) {
		Survivor survivor = GameMaster.Survivors[index];

		// Update the bio section for that survivor
		bioName.text = survivor.GivenName + " " + survivor.Surname;
		bioPortrait.GetComponent<SpriteRenderer>().sprite = charPortraits[index].sprite;//Resources.Load(survivor.PortraitImage, typeof(Sprite)) as Sprite; //removing dependency on resources
		bioHealth.value = survivor.HP / 100f;
		bioFill.color = GetColor(bioHealth.value);
		bioAge.text = "Age: " + survivor.Age;
		bioText.text = survivor.Biography;
		bioTraits.text = "[" + survivor.CharacterTraits[0] + "]\n[" + survivor.CharacterTraits[1] + "]";

		// Remember the last bio that was loaded in the case of changing screens
		UI_LoadScene.lastBioLoaded = index;
	}

	public static Color32 GetColor(float percentage) {
		if(percentage > 0.65f) return new Color32(19, 255, 0, 255);
		else if(percentage > 0.35) return new Color32(255, 206, 0, 255);
		else return new Color32(255, 0, 0, 255);
	}

	// Swaps the bio scene into bio mode
	public void SwapToBio(){
		bioPanel.SetActive(true);
		historyPanel.SetActive(false);
		bioHistory.text = "Bio";
		bioClose.SetActive(true);
	}

	// Swap the bio scene into history mode
	public void SwapToHistory(){
		bioPanel.SetActive(false);
		historyPanel.SetActive(true);
		bioHistory.text = "History";
		bioClose.SetActive(false);
	}

	public void UnselectPortraitButton() {
		EventSystem.current.SetSelectedGameObject(null);
	}

}
