using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster {

	/* Readonly variables */
	public static readonly float UPKEEP_FUEL = 6f; //applied once for the entire party
	public static readonly float UPKEEP_OXYGEN = 1f; //applied once per living person
	public static readonly float UPKEEP_RATIONS = 1f; //applied once per living person in order of lowest HP
	public static readonly int MAX_ACTIONS = 4;
	public static readonly int ROVER_START_POSITION = 11;
	public static readonly int SHOP_FREQUENCY = 5; // will come across a shop every 5 days (WILL MISS IF TIME IS GAINED THROUGH AN EVENT)
	public static readonly int STORM_START_POSITION = 6;
	public static readonly string NO_FUEL_EVENT = "71FBD114";
	public static readonly string OXYGEN_TANK_EVENT = "A63177CA";
	public static readonly string SHOP_EVENT = "4721DF9F"; //For the cyberpunk shop event, check Survivor class variables.
	public static readonly string[] STARTING_EVENTS = {
		"8D7FA2D9",
		"B2356C08",
		"027D48CB",
		"E7C5C679",
		"1240AC90"
	};
	public static readonly Supplies STARTING_RESOURCES = new Supplies(60f, 48f, 48f, 60f);
	
	/* Public variables */
	public static bool IN_GAME = false;
	public static bool SPECIAL_EDITOR = false;
	public static Event CurrentEvent; //Currently loaded event
	public static float difficulty = 30;
	public static Supplies resources; //Remaining resources of the survivors
	public static string GameOverText;
	public static string GameOverTitle;

	/* Private variables */
	private static Choice[] actions;
	private static float eta; //number of days from the origin - defined in Init()
	private static int roverPosition;
	private static int tutorialIndex;
	private static int stormPosition; //number of days from the origin
	private static string currentImage = "";
	private static string[] actionText;
	private static Survivor[] survivors;	//List of all Survivors in this playthrough of the game


	/*************************************************************/
	/***                     STATIC METHODS                    ***/
	/*************************************************************/

	public static float ETA { get { return eta; } }
	public static float[] Resources { get { return resources.ToArray(); } }
	public static int RoverPosition { get { return roverPosition; } set { roverPosition = value; } }
	public static int StormPosition { get { return stormPosition; } }
	public static string EventID { get { return XMLReadWriter.ConvertToHex(CurrentEvent.eventID); } }
	public static string EventImage { get { return currentImage; } set { if(value.Length > 0) currentImage = value; } } // FINN YOU DIPSHIT, IF YOU ARE TRYING TO FIND AN ERROR HERE ITS BECAUSE YOUR NOT PLAYING FROM THE TITLE SCREEN
	public static string EventText { get { return CurrentEvent.text; } }
	public static string[] ActionText { get {return actionText; } }
	public static Survivor[] Survivors { get { return survivors; } }


	public static void AdvanceDays(int days, bool moveRover) {
		float alcoholPerDay;
		float fuelPerDay;
		int survivorsAlive;

		//Applies daily upkeep according to the number of days requested
		for(int i = 0; i < days; i++) {

			survivorsAlive = 0;
			foreach(Survivor s in Survivor.GetByHealth()) {
				if(resources.Rations - UPKEEP_RATIONS >= 0) {
					if(s.ModifyHP(+2 * ((int)UPKEEP_RATIONS), true)) History.AddManualEntry(s.GivenName + " ate " + UPKEEP_RATIONS.ToString("F0") + " rations and healed " + History.GetHPDifference(s).ToString() + "HP!", History.GREEN);
					else History.AddManualEntry(s.GivenName + " ate " + UPKEEP_RATIONS.ToString("F0") + " rations.", History.RED);
					resources += new Supplies(0f, 0f, 0f, -UPKEEP_RATIONS);
				} else {
					if(!s.ModifyHP(-2 * ((int)UPKEEP_RATIONS), true)) {
						History.AddManualEntry("<b>" + s.GivenName + " couldn't find any food and starved to death!</b>", History.BLOOD_RED);
						continue; //Skips the survivor's alive increment
					}
					else History.AddManualEntry(s.GivenName + " couldn't find any food and went hungry! (" + History.GetHPDifference(s).ToString() + " HP)", History.RED);
				}
				
				survivorsAlive++; //not applied if the survivor died
			}

			fuelPerDay = (Survivor.PartyHasTrait(Traits.CAR_JOCKEY)) ? UPKEEP_FUEL - Survivor.CAR_JOCKEY_FUEL_SAVINGS : UPKEEP_FUEL;
			if(moveRover && resources.Fuel > fuelPerDay) {
				roverPosition++;

				History.AddManualEntry("Our rover used " + fuelPerDay.ToString("F0") + " units of fuel.", History.RED);
				if(fuelPerDay != UPKEEP_FUEL) History.AddManualEntry(Survivor.GetByTrait(Traits.CAR_JOCKEY).GivenName + "'s skilled, sand-dune-traversing technique, saved us " + Survivor.CAR_JOCKEY_FUEL_SAVINGS.ToString("F0") + " units of fuel.", History.GREEN);

			} else {
				fuelPerDay = 0;
				if(moveRover) History.AddManualEntry("<b>Our rover doesn't have enough fuel remaining to make a full day's trip!</b>", History.YELLOW);
			}

			if(Survivor.PartyHasTrait(Traits.ALCOHOLIC)) {
				alcoholPerDay =  Survivor.ACOHOLIC_DAILY_CONSUMPTION;
				History.AddManualEntry(Survivor.GetByTrait(Traits.ALCOHOLIC).GivenName + " drank " + Survivor.ACOHOLIC_DAILY_CONSUMPTION.ToString("F0") + " units of our alcohol supply.", History.RED);
			} else alcoholPerDay = 0;

			//Decrements player resources according to consumers
			resources = +(resources + new Supplies(-alcoholPerDay, -fuelPerDay, -UPKEEP_OXYGEN * survivorsAlive, 0f));

			History.AddManualEntry("Our remaining oxygen supplies will last us " + (resources.Oxygen / (UPKEEP_OXYGEN * survivorsAlive)).ToString("F2") + " more days!", History.YELLOW);
			History.AddManualEntry("We breathed " + ((int)UPKEEP_OXYGEN * survivorsAlive).ToString() + " units of oxygen.", History.RED);

			//Advances the storm by one day
			stormPosition++;
			History.AddManualEntry("The storm moved a day closer!", History.YELLOW);
			
			History.NewDay();

			//Checks if the game has ended
			CheckEndGameConditions();
		}

		History.LogSurvivorData();
	}
	

	public static void CheckEndGameConditions() {
		if(IN_GAME) {
			//Win conditions
			if(roverPosition >= ETA) End(true, "Victory", "On day " + History.GetDay().ToString() + " we made it safely to the sister base.");
			
			//Lose conditions
			if(resources.Oxygen <= 0) End(false, "Out of Oxygen", "LAST KNOWN LOG ENTRY>\n\n\n\nThis is my last entry.\n\nWe're not going to make it. My breaths grow shorter by the minute and my vision has started to blur. There just simply isn't enough oxygen.\n\nI don't ... want ... to die.\nSomebody ..., please ... anybody!\nHe...lp ...me...");
			else if(stormPosition >= roverPosition) End(false, "Engulfed by the storm", "");
			else if(Survivor.PartyKilled(survivors)) End(false, "All survivors dead", "");
		}
	}


	public static void End(bool won, string header, string msg) {
		IN_GAME = false;
		GameOverTitle = header;
		GameOverText = msg;
		
		if(won){
			SceneManager.LoadScene("UI_SplashScreen_Win");
			UI_Music.PlayWinMusic();
		} else {
			SceneManager.LoadScene("UI_SplashScreen_Lose");
		 	UI_Music.PlayLoseMusic();
		}
	}


	public static bool HasActions() {
		return actions.Length > 0 && actionText[0].Length > 0;
	}


	public static void Init(bool start) {
		UnitTests();

		EventMaster.Init();
		History.Init();
		survivors = Survivor.DefineSurvivors();
		resources = new Supplies(0f, 0f, 0f, 0f); //0 here, then set to 100 to avoid errors then set to actual starting values after the first event in run()

		History.LogResources();

		stormPosition = STORM_START_POSITION;
		roverPosition = ROVER_START_POSITION;
		tutorialIndex = 0;
		eta = difficulty;

		XMLReadWriter.ReadEventList();
		EventMaster.SearchForEventHeads();

		if(start) EventMaster.GetEvent(XMLReadWriter.ConvertToDec(STARTING_EVENTS[tutorialIndex++])).Load();
	}


	public static bool IsSelectable(int index) {
		if(index < actions.Length && actionText[index].Length > 0) return actions[index].IsSelectable();
		return false;
	}


	public static Supplies ProjectCost(int index) {
		return (index < actions.Length) ? actions[index].cost : new Supplies(0f, 0f, 0f, 0f);
	}


	public static void Run(bool log) {
		//Cycles through the tutorial/starting events
		if(tutorialIndex < STARTING_EVENTS.Length) EventMaster.GetEvent(XMLReadWriter.ConvertToDec(STARTING_EVENTS[tutorialIndex++])).Load();

		//Oxygen tank punctured event --final day if full party
		else if(roverPosition == eta - 1 && Survivor.FullParty()) EventMaster.GetEvent(XMLReadWriter.ConvertToDec(OXYGEN_TANK_EVENT)).Load();
		
		//Encounters the item shop
		else if(roverPosition % SHOP_FREQUENCY == 0)
			EventMaster.GetEvent(XMLReadWriter.ConvertToDec(Survivor.PartyHasTrait(Traits.CYBERPUNK) ? Survivor.CYBERPUNK_SHOP_EVENT : SHOP_EVENT)).Load();

		//Event for if the player has run out of fuel
		else if(resources.Fuel < UPKEEP_FUEL || (Survivor.PartyHasTrait(Traits.CAR_JOCKEY) && resources.Fuel < (UPKEEP_FUEL - Survivor.CAR_JOCKEY_FUEL_SAVINGS)))
			EventMaster.GetEvent(XMLReadWriter.ConvertToDec(NO_FUEL_EVENT)).Load();

		//Secret dendrophile ending (one in one hundred million of a chance)
		else if(Random.Range(0f, 1f) < (1f / 100000000f)) EventMaster.GetEvent(XMLReadWriter.ConvertToDec("9F383972")).Load();

		//Gets a random event
		else EventMaster.GetRandomEventHead().Load();


		//If first event, sets resources to values that will not fail during upkeep (and oxygen to a value for a realistic days remaining estimate)
		if(roverPosition == ROVER_START_POSITION) resources = new Supplies(100f, 100f, STARTING_RESOURCES.Oxygen + (UPKEEP_OXYGEN * Survivors.Length), 100f); 

		if(log) History.LogResources();
		AdvanceDays(1, true);

		if(roverPosition - 1 == ROVER_START_POSITION) {
			resources = new Supplies(STARTING_RESOURCES);
			History.LogResources();
		}
	}


	public static void SelectAction(int index) {
		if(actions.Length > 0 && index < actions.Length) actions[index].ActivateChoice();
		else Run(true);
	}


	//Unit Tests
	private static void UnitTests() {
		if(Debug.isDebugBuild) {
			Debug.Log("This is a development build! Running unit tests...");

			XMLReadWriter.Test();
			Supplies.Test();

			Debug.Log("Unit tests completed!");
		}
	}


	public static void UpdateActions() {
		actions = CurrentEvent.GetChoices();
		actionText = new string[MAX_ACTIONS];

		for(int i = 0; i < MAX_ACTIONS; i++) {
			actionText[i] = (i < actions.Length) ? actions[i].text : "";
		}
	}

}