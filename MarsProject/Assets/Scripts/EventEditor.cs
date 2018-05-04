using System;
using System.Collections;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

class EventEditor : MonoBehaviour {

    #if UNITY_EDITOR
    /* Readonly constants for defining limits */
    public static readonly int MAX_EVENT_TEXT_LENGTH = 800;
    public static readonly int MAX_CHOICE_TEXT_LENGTH = 400;
    public static readonly int MAX_CHOICES = GameMaster.MAX_ACTIONS;
    public static readonly int MIN_DAMAGE = -100;
    public static readonly int MAX_DAMAGE = 100;


    /* Declares variables for use in the event editor */
    private static ArrayList choices;
    private static bool allowLethalBlows, choiceScreen, editingEvent, eventHead, lockedEvent, repeatable;
    private static bool[] survivorDamage;
    private static int choiceNum, eventID, parentID, editingEventID;
    private static string alcohol, chanceOfDamage, chanceOfSuccess, choiceText, choiceTrait, days, errorMessage, eventImageSource, eventText, eventTrait, failureEventID, fuel, minDamage, maxDamage, oxygen, rations, searchEventID, successEventID, unlocksEventID;


    /*************************************************************/
    /***                    INSTANCE METHODS                   ***/
    /*************************************************************/

    /* If this class is loaded by a game object, reset all
    event screen variables to defaults before loading the GUI */
    private void Awake() {
        if(!GameMaster.IN_GAME && !GameMaster.SPECIAL_EDITOR) GameMaster.Init(false);

        ResetEventScreen();
        if(GameMaster.CurrentEvent != null) LoadEvent(GameMaster.CurrentEvent);

        StringBuilder s = new StringBuilder();
        s.Append("Previously added events:\n");
        for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
            s.Append(EventMaster.GetEventFromIndex(i).PrintEventSummary() + "\n");
        }
        Debug.Log(s.ToString());
    }

    /* Method calculates and formats the number of characters remaining for main text area */
    private void CharactersRemaining(int len, int max) {
        if(len > max) GUI.color = Color.red;
        else if(len > 3*max/4) GUI.color = Color.yellow;
        GUI.skin.label.alignment = TextAnchor.LowerRight;
        GUI.Label(new Rect(50, 80, Screen.width - 105, Screen.height - 250), (max - len).ToString() + "/" + max.ToString());
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.color = Color.white;
    }

    /* Method for loading previously written choices */
    private void LoadChoice(Choice c) {
        Assert.IsNotNull(c);

        //Loads in choice variables
        alcohol = c.cost.Alcohol.ToString();
        chanceOfSuccess = c.chanceOfSuccess.ToString();
        choiceText = c.text;
        choiceTrait = c.requiredTrait.ToString();
        days = c.time.ToString();
        failureEventID = (c.failureEventID == int.MaxValue) ? "" : XMLReadWriter.ConvertToHex(c.failureEventID);
        fuel = c.cost.Fuel.ToString();
        oxygen = c.cost.Oxygen.ToString();
        rations = c.cost.Rations.ToString();
        successEventID = (c.successEventID == int.MaxValue) ? "" : XMLReadWriter.ConvertToHex(c.successEventID);
        unlocksEventID = (c.unlocksEventID == int.MaxValue) ? "" : XMLReadWriter.ConvertToHex(c.unlocksEventID);

        //Sets relevant UI variables
        choiceScreen = true;
        errorMessage = "";
    }

    /* Method for loading previously written events */
    public void LoadEvent(Event e) {
        //Checks that e is actually an event
        if(e == null) {
            errorMessage = "The Event you searched for could not be found!";
        } else {
            //Loads the event
            allowLethalBlows = e.allowLethalBlows;
            eventHead = e.isEventHead;
            eventID = e.eventID;
            eventImageSource = e.imageSource;
            eventText = e.text;
            eventTrait = e.requiredTrait.ToString();
            chanceOfDamage = e.chanceOfDamage.ToString();
            choices = new ArrayList();
            choices.AddRange(e.choices);
            lockedEvent = e.lockedEvent;
            maxDamage = e.maxDamage.ToString();
            minDamage = e.minDamage.ToString();
            parentID = e.parentID;
            repeatable = e.repeatableEvent;

            for(int i = 0; i < survivorDamage.Length; i++) {
                if(e.damageProduct % Survivor.DefineSurvivors()[i].uniquePrime == 0) survivorDamage[i] = true;
            }

            //Resets anything that needs resetting
            editingEvent = true;
            errorMessage = "";
            choiceNum = 0;
            choiceScreen = false;
            editingEventID = eventID;
            searchEventID = "";
        }
    }

    /* Method called to display a GUI by unity game objects every iteration */
    private void OnGUI() {
        //Writes the current event ID to the top right corner
        GUI.Label(new Rect(15, 10, 200, 20), "Event ID: " + XMLReadWriter.ConvertToHex(eventID));

        //Load parent event button (if it has one)
        GUI.Label(new Rect(Screen.width - 420, 10, 200, 20), "ParentID: " + ((parentID == int.MaxValue) ? "NONE" : XMLReadWriter.ConvertToHex(parentID)));
        if(editingEvent && parentID != int.MaxValue && GUI.Button(new Rect(Screen.width - 250, 10, 90, 20), "Load Parent")) {
            if(EditorUtility.DisplayDialog("Go to linked event?", "Are you sure you want to load this event (all unsaved changes will me lost)?", "Load", "Wait, I need to save!")) {
                //Attempt to load the event with the parent id
                LoadEvent(EventMaster.GetEvent(parentID));
            }
        }

        //Return to game button (if editing from within the game)
        if(GameMaster.IN_GAME && GUI.Button(new Rect((Screen.width / 2) - 75, 10, 150, 20), "Return to Game")) {
            if(EditorUtility.DisplayDialog("Go back to the game?", "Are you sure you want to return to the game (all unsaved changes will me lost)?", "Return", "Wait, I need to save!")) {
                EventMaster.GetEvent(editingEventID).Load();
            }
        }

        //Search for an existing event ID
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        if(searchEventID.Equals("")) GUI.Label(new Rect(Screen.width - 150, 10, 75, 20), "EventID");
        searchEventID = GUI.TextField(new Rect(Screen.width - 150, 10, 75, 20), searchEventID).ToUpper();
        if(GUI.Button(new Rect(Screen.width - 70, 10, 60, 20), "Search")) {
            try {
                //Attempts to convert the search id into an int
                int searchTarget = int.Parse(searchEventID, System.Globalization.NumberStyles.HexNumber);

                //If valid, then double check the user wants to discard any changes
                if(EditorUtility.DisplayDialog("Discard and search?", "Are you sure you want to load this new event (all unsaved changes will me lost)?", "Search", "Wait, I need to save!")) {
                    //Attempt to load the event with the search id
                    LoadEvent(EventMaster.GetEvent(searchTarget));
                }
            } catch (FormatException) {
                errorMessage = "FormatException: Search event ID is not in hexadecimal format!";
            } catch (OverflowException) {
                errorMessage = "OverflowException: Search event ID is out of range for an int!";
            }
        }
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

        //Formats and shows any error messages
        if(errorMessage.Length > 0) {
            GUI.color = Color.red;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(10, Screen.height - 30, (choiceScreen) ? Screen.width - 10 : Screen.width - 185, 20), errorMessage);
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.color = Color.white;
        }

        //Displays the event editor screen
        if(!choiceScreen) {

            /* Input fields & labels above the eventText box */
            GUI.Label(new Rect(60, 55, 100, 20), "Event Text: ");
            GUI.Label(new Rect(Screen.width - 300, 55, 100, 20), "Requied Trait: ");

            //Changes the colour to of the eventTrait input to red if it is NOT a valid Trait within the system
            try  {
                Enum.Parse(typeof(Traits), eventTrait);
            } catch (ArgumentException) {
                GUI.color = Color.red;
            }
            eventTrait = GUI.TextField(new Rect(Screen.width - 200, 55, 150, 20), eventTrait).ToUpper();
            GUI.color = Color.white;

            eventHead = GUI.Toggle(new Rect(190, 55, 100, 20), eventHead, " Event Head");
            lockedEvent = GUI.Toggle(new Rect(300, 55, 100, 20), lockedEvent, " Lock Event");
            repeatable = GUI.Toggle(new Rect(400, 55, 100, 20), repeatable, " Repeatable");


            /* Event Text Area related */
            //Adds an indicator to show the number of characters typed compared to the maximum.
            CharactersRemaining(eventText.Length, MAX_EVENT_TEXT_LENGTH);
            eventText = GUI.TextArea(new Rect(50, 80, Screen.width - 100, Screen.height - 250), eventText);



            /* Input fields, labels & toggle buttons etc below the eventText box */
            int y = Screen.height - 165;
            int dy = 25;

            //First row
            GUI.Label(new Rect(80, y, 140, 25), "Damage Options:");
            GUI.Label(new Rect(Screen.width - 400, y, 100, 20), "Image Source: ");
            eventImageSource = GUI.TextField(new Rect(Screen.width - 300, y, 250, 20), eventImageSource);
            GUI.Label(new Rect((Screen.width / 2) - 280, y, 140, 25), "Survivors to damage: ");

            y += dy;
            
            //Second row
            GUI.Label(new Rect(80, y, 140, 25), "Chance of damage: ");
            chanceOfDamage = GUI.TextField(new Rect(205, y, 40, 20), chanceOfDamage);

            //Displays toggle buttons for damaging each survivor
            for(int i = 0; i < Survivor.DefineSurvivors().Length; i++) {
                survivorDamage[i] = GUI.Toggle(new Rect((Screen.width / 2) - 280 + (150*(i%3)), y + ((i % 2)*20), 140, 20), survivorDamage[i], " " + Survivor.DefineSurvivors()[i].GivenName + " " + Survivor.DefineSurvivors()[i].Surname);
            }

            //Button for adding a new choice - restricted to a max of 15 choices b/c only so many edit buttons will fit (though could fit a max of 30)
            if(choices.Count < MAX_CHOICES) {
                if(GUI.Button(new Rect(Screen.width - 360, y + (dy / 2), 100, 20), "New Choice")) {
                    ResetChoiceScreen();
                    choiceNum++;
                    choiceScreen = true;
                }
            } else {
                errorMessage = "Maximum number of choices reached for a pretty UI";
            }

            //Places buttons to edit your old choices
            for(int i = 0; i < choices.Count; i++) {
                if(GUI.Button(new Rect(Screen.width - 360 + (105 * ((i + 1) % 3)), y + (dy / 2) + (dy * ((i + 1) / 3)) , 100, 20), "Edit Choice " + i.ToString())) {
                    choiceNum = i;
                    LoadChoice((Choice)choices[i]);
                }
            }

            y += dy;

            //Third & fouth rows
            GUI.Label(new Rect(80, y, 30, 20), "Min:");
            minDamage = GUI.TextField(new Rect(110, y, 40, 20), minDamage);
            GUI.Label(new Rect(170, y, 35, 20), "Max:");
            maxDamage = GUI.TextField(new Rect(205, y, 40, 20), maxDamage);

            y += dy;

            //Fourth row
            allowLethalBlows = GUI.Toggle(new Rect(80, y, 160, 20), allowLethalBlows, " Allow Lethal Blows");

            //If editing an event
            if(editingEvent) {
                //Button to delete the event currently being edited
                if(GUI.Button(new Rect(Screen.width - 230, Screen.height - 30, 70, 20), "Delete")) {
                    if(EditorUtility.DisplayDialog("Permanently delete?", "Are you sure you want to permanently delete this event file?", "Kill it with fire!", "Whoops! Didn't mean to click this.")) {
                        EventMaster.PermanentlyDeleteEvent(eventID, XMLReadWriter.path + XMLReadWriter.prefix + XMLReadWriter.ConvertToHex(eventID) + XMLReadWriter.type);
                        ResetEventScreen();
                    }
                }
                //Button to reload event being edited
                if(GUI.Button(new Rect(Screen.width - 155, Screen.height - 30, 70, 20), "Reload")) {
                    if(EditorUtility.DisplayDialog("Reload event?", "Are you sure you want to reload this event (all unsaved changes will be lost)?", "My changes were terrible.", "I like it better this way.")) {
                        LoadEvent(EventMaster.GetEvent(eventID));
                    }
                }
            } else {
                //Button to clear all fields
                if(GUI.Button(new Rect(Screen.width - 155, Screen.height - 30, 70, 20), "Reset")) {
                    if(EditorUtility.DisplayDialog("Reset editor?", "Are you sure you want to discard all changes and start again?", "Yeah", "Nah")) {
                        ResetEventScreen();
                    }
                }
            }

            //Button to confirm the event currently being written
            if(GUI.Button(new Rect(Screen.width - 80, Screen.height - 30, 70, 20), "Commit")) {
                //Checks the validity of event inputs and converts them into an event object
                Event e = ValidateEvent();

                //If inputs were valid, e will not equal null
                if(e != null) {
                    //Writes that event to a new/over an existing XML file
                    XMLReadWriter.WriteEvent(e);

                    //Updates an event or adds the new event has been created (and is to be written to the file of possilbe events)
                    if(editingEvent) {
                        EventMaster.ReloadEvent(e);
                    } else {
                        EventMaster.AddEvent(e);
                    }

                    if(EditorUtility.DisplayDialog("Create New Event", "The last event has been committed. Create a new event?", "Yep, I'm done with this shit.", "Nope, I want to change more.")) {
                        //Reset screen for new event
                        ResetEventScreen();
                    }
                    
                }
            }

            //Button to load the next event serially
            int eventIndex = EventMaster.GetEventIndex(eventID);
            if(eventIndex != -1) {
                if(GUI.Button(new Rect(Screen.width - 300, Screen.height - 60, 110, 20), "Previous Event")) {
                    int prev = eventIndex - 1;
                    if(prev < 0) prev += EventMaster.GetEventListLength();
                    LoadEvent(EventMaster.GetEventFromIndex(prev));
                }
                GUI.Label(new Rect(Screen.width - 170, Screen.height - 60, 90, 20), (eventIndex + 1).ToString("D3") + "/" + EventMaster.GetEventListLength());
                if(GUI.Button(new Rect(Screen.width - 100, Screen.height - 60, 90, 20), "Next Event")) {
                    LoadEvent(EventMaster.GetEventFromIndex((eventIndex + 1) % EventMaster.GetEventListLength()));
                }
            }

        //Displays the choice editor screen
        } else {
            //Text box labels
            GUI.Label(new Rect(Screen.width - 300, 55, 100, 20), "Requied Trait: ");
            GUI.Label(new Rect(55, 55, 140, 20), "Choice Text: ");
            GUI.Label(new Rect(55, Screen.height - 160, 70, 20), "Alcohol: ");
            GUI.Label(new Rect(180, Screen.height - 160, 70, 20), "Fuel: ");
            GUI.Label(new Rect(55, Screen.height - 138, 70, 25), "Oxygen: ");
            GUI.Label(new Rect(180, Screen.height - 135, 70, 20), "Rations: ");
            GUI.Label(new Rect(155, Screen.height - 88, 100, 25), "Time (days): ");

            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(Screen.width - 340, Screen.height - 160, 140, 20), "Chance of Success: ");
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            if(GUI.Button(new Rect(Screen.width - 340, Screen.height - 135, 140, 20), "Success Event ID: ")) {
                try {
                    //Attempts to convert the success event id into an int
                    int sEventID = int.Parse(successEventID, System.Globalization.NumberStyles.HexNumber);

                    //If valid, then double check the user wants to discard any changes
                    if(EditorUtility.DisplayDialog("Go to linked event?", "Are you sure you want to load this event (all unsaved changes will me lost)?", "Load", "Wait, I need to save!")) {
                        //Attempt to load the event with the search id
                        LoadEvent(EventMaster.GetEvent(sEventID));
                    }
                } catch (FormatException) {
                    errorMessage = "FormatException: Success event ID is not in hexadecimal format!";
                } catch (OverflowException) {
                    errorMessage = "OverflowException: Success event ID is out of range for an int!";
                }
            }
            if(GUI.Button(new Rect(Screen.width - 340, Screen.height - 110, 140, 20), "Failure Event ID: ")) {
                try {
                    //Attempts to convert the success event id into an int
                    int fEventID = int.Parse(failureEventID, System.Globalization.NumberStyles.HexNumber);

                    //If valid, then double check the user wants to discard any changes
                    if(EditorUtility.DisplayDialog("Go to linked event?", "Are you sure you want to load this event (all unsaved changes will me lost)?", "Load", "Wait, I need to save!")) {
                        //Attempt to load the event with the search id
                        LoadEvent(EventMaster.GetEvent(fEventID));
                    }
                } catch (FormatException) {
                    errorMessage = "FormatException: Failure event ID is not in hexadecimal format!";
                } catch (OverflowException) {
                    errorMessage = "OverflowException: Failure event ID is out of range for an int!";
                }
            }
            if(GUI.Button(new Rect(Screen.width - 340, Screen.height - 85, 140, 20), "Unlocks Event ID: ")) {
                try {
                    //Attempts to convert the success event id into an int
                    int lEventID = int.Parse(unlocksEventID, System.Globalization.NumberStyles.HexNumber);

                    //If valid, then double check the user wants to discard any changes
                    if(EditorUtility.DisplayDialog("Go to linked event?", "Are you sure you want to load this event (all unsaved changes will me lost)?", "Load", "Wait, I need to save!")) {
                        //Attempt to load the event with the search id
                        LoadEvent(EventMaster.GetEvent(lEventID));
                    }
                } catch (FormatException) {
                    errorMessage = "FormatException: Unlocks event ID is not in hexadecimal format!";
                } catch (OverflowException) {
                    errorMessage = "OverflowException: Unlocks event ID is out of range for an int!";
                }
            }

            //Adds an indicator to show the number of characters typed compared to the maximum.
            CharactersRemaining(choiceText.Length, MAX_CHOICE_TEXT_LENGTH);

            //Changes the colour to of the choiceTrait input to red if it is NOT a valid Trait within the system
            try  {
                Enum.Parse(typeof(Traits), choiceTrait);
            } catch (ArgumentException) {
                GUI.color = Color.red;
            }
            choiceTrait = GUI.TextField(new Rect(Screen.width - 200, 55, 150, 20), choiceTrait).ToUpper();
            GUI.color = Color.white;

            //Input fields for resources
            alcohol = GUI.TextField(new Rect(112, Screen.height - 160, 40, 20), alcohol);
            fuel = GUI.TextField(new Rect(237, Screen.height - 160, 40, 20), fuel);
            oxygen = GUI.TextField(new Rect(112, Screen.height - 135, 40, 20), oxygen);
            rations = GUI.TextField(new Rect(237, Screen.height - 135, 40, 20), rations);
            days = GUI.TextField(new Rect(237, Screen.height - 85, 40, 20), days);

            //Input fields for chance, and resulting chained events
            chanceOfSuccess = GUI.TextField(new Rect(Screen.width - 200, Screen.height - 160, 150, 20), chanceOfSuccess);
            successEventID = GUI.TextField(new Rect(Screen.width - 200, Screen.height - 135, 150, 20), successEventID).ToUpper();
            failureEventID = GUI.TextField(new Rect(Screen.width - 200, Screen.height - 110, 150, 20), failureEventID).ToUpper();
            unlocksEventID = GUI.TextField(new Rect(Screen.width - 200, Screen.height - 85, 150, 20), unlocksEventID).ToUpper();

            //Input field for optional text describing the choice
            choiceText = GUI.TextArea(new Rect(50, 80, Screen.width - 100, Screen.height - 250), choiceText);

            //Button removes the current choice (only shows if this is not a new choice)
            if(choiceNum < choices.Count) {
                if(GUI.Button(new Rect(Screen.width - 270, Screen.height - 60, 70, 20), "Remove")) {
                    if(EditorUtility.DisplayDialog("Remove choice?", "Are you sure that you want to remove this choice (can still be retrieved by reloading event)?", "Remove", "Cancel")) {
                        choices.RemoveAt(choiceNum--);
                        choiceScreen = false;
                    }
                }
            }

            //Button confirms the current choice creation
            if(GUI.Button(new Rect(Screen.width - 195, Screen.height - 60, 70, 20), "Confirm")) {
                //If all inputs are valid, c is assigned to a new Choice object
                Choice c = ValidateChoice();
                //If not, c will be null
                if(c != null) {
                    if(choiceNum < choices.Count) {
                        //If editing a choice, replaces the original reference in the array list with this new object
                        choices[choiceNum] = c;
                    } else {
                        //Adds the new choice to the choices array list until event creation/update
                        choices.Add(c);
                    }

                    //Go back to event editor scene
                    choiceScreen = false;
                }
            }

            //Button cancels the current choice creation
            if(GUI.Button(new Rect(Screen.width - 120, Screen.height - 60, 70, 20), "Cancel")) {
                choiceNum--;
                choiceScreen = false;
            }
        }
    }


    /* Resets all event screen variables to their defaults */
    private void ResetEventScreen() {
        allowLethalBlows = true;
        chanceOfDamage = "0.0";
        choiceNum = 0;
        choices = new ArrayList();
        editingEvent = false;
        errorMessage = "";
        eventHead = false;
        eventID = GenerateUniqueID();
        eventImageSource = "";
        lockedEvent = false;
        eventText = "";
        eventTrait = "NONE";
        maxDamage = "0";
        minDamage = MIN_DAMAGE.ToString();
        parentID = int.MaxValue;
        repeatable = false;
        searchEventID = "";
        survivorDamage = new bool[Survivor.DefineSurvivors().Length];
    }

    /* Resets all choice screen variables to their defaults */
    private void ResetChoiceScreen() {
        alcohol = "0";
        chanceOfSuccess = "0.5";
        choiceText = "";
        choiceTrait = "NONE";
        days = "0";
        errorMessage = "";
        failureEventID = "";
        fuel = "0";
        oxygen = "0";
        rations = "0";
        successEventID = "";
        unlocksEventID = "";
    }

    /* Checks the choice inputs to see if they are valid */
    private Choice ValidateChoice() {
        float chance;
        float[] cost;
        int time;
        Traits t;
        int[] chainedID = new int[3];

        if(choiceText.Length > MAX_CHOICE_TEXT_LENGTH) {
            errorMessage = "Choice text is too long!";
            return null;
        }

        try {
            chance = float.Parse(chanceOfSuccess);
            if(chance < 0f) chance = 0f;
            else if(chance > 1f) chance = 1f;
        } catch (FormatException) {
            errorMessage = "FormatException: Chance of success could not be converted into a float!";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: Chance of success is out of range for a float!";
            return null;
        }

        try {
            cost = new float[4] {float.Parse(alcohol), float.Parse(fuel), float.Parse(oxygen), float.Parse(rations)};
        } catch (FormatException) {
            errorMessage = "FormatException: A resource could not be converted into a float!";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: A resource is out of range for a float!";
            return null;
        }

        try {
            time = int.Parse(days);
        } catch (FormatException) {
            errorMessage = "FormatException: Time could not be converted into an int!";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: Time is out of range for an int!";
            return null;
        }

        try {
            t = (Traits)Enum.Parse(typeof(Traits), choiceTrait);
        } catch (ArgumentException) {
            errorMessage = "ArgumentException: " + choiceTrait + " is not a known Traits definition! (Whitespace?)";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: The required trait is outside of the range of the underlying Traits enum!";
            return null;
        }

        try {
            if(successEventID.Equals("")) chainedID[0] = int.MaxValue;
            else chainedID[0] = int.Parse(successEventID, System.Globalization.NumberStyles.HexNumber);
            if(failureEventID.Equals("")) chainedID[1] = int.MaxValue;
            else chainedID[1] = int.Parse(failureEventID, System.Globalization.NumberStyles.HexNumber);
            if(unlocksEventID.Equals("")) chainedID[2] = int.MaxValue;
            else chainedID[2] = int.Parse(unlocksEventID, System.Globalization.NumberStyles.HexNumber);
        } catch (FormatException) {
            errorMessage = "FormatException: A chained event ID is not in hexadecimal format!";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: A chained event ID is out of range for an int!";
            return null;
        }

        return new Choice(choiceText, chance, time, new Supplies(cost[0], cost[1], cost[2], cost[3]), t, chainedID[0], chainedID[1], chainedID[2]);
    }

    /* Checks the event inputs to see if they are valid */
    private Event ValidateEvent() {
        float chance;
        int min, max, damageProduct = 1;
        Traits t;

        //Checks that there is actually something to add
        if(eventText.Equals("") && choices.Count == 0) {
            errorMessage = "Blank event! Must have text OR at least one choice.";
            return null;
        }
        
        if(eventText.Length > MAX_EVENT_TEXT_LENGTH) {
            errorMessage = "Event text is too long!";
            return null;
        }

        try {
            t = (Traits)Enum.Parse(typeof(Traits), eventTrait);
        } catch (ArgumentException) {
            errorMessage = "ArgumentException: " + eventTrait + " is not a known Traits definition! (Whitespace?)";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: The required trait is outside of the range of the underlying Traits enum!";
            return null;
        }

        try {
            chance = float.Parse(chanceOfDamage);
            if(chance < 0f) chance = 0f;
            else if(chance > 1f) chance = 1f;
        } catch (FormatException) {
            errorMessage = "FormatException: Chance of damage could not be converted into a float!";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: Chance of damage is out of range for a float!";
            return null;
        }

        try {
            if(minDamage.Equals("")) min = MIN_DAMAGE;
            else min = int.Parse(minDamage);
            if(min < MIN_DAMAGE) min = MIN_DAMAGE;
            if(min > MAX_DAMAGE) min = MAX_DAMAGE;

            if(maxDamage.Equals("")) max = 0;
            else max = int.Parse(maxDamage);
            if(max < MIN_DAMAGE) max = MIN_DAMAGE;
            if(max > MAX_DAMAGE) max = MAX_DAMAGE;
        } catch (FormatException) {
            errorMessage = "FormatException: A chained event ID is not in hexadecimal format!";
            return null;
        } catch (OverflowException) {
            errorMessage = "OverflowException: A chained event ID is out of range for an int!";
            return null;
        }

        for(int i = 0; i < survivorDamage.Length; i++) {
            if(survivorDamage[i]) damageProduct *= Survivor.DefineSurvivors()[i].uniquePrime;
        }

        Event e = new Event(eventID, parentID, eventHead, lockedEvent, repeatable, eventText, t, eventImageSource, chance, min, max, damageProduct, allowLethalBlows, (Choice[])choices.ToArray(typeof(Choice)));

        if(e.isEventHead) { //Looks down
            EventMaster.RecursiveParent(null, e);
        } else if(e.parentID == int.MaxValue) { //Looks up
            Event o;

            for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
                o = EventMaster.GetEventFromIndex(i);

                if(o.LinkedTo(e.eventID)) {
                    e.parentID = o.eventID;
                    XMLReadWriter.WriteEvent(e);
                    break;
                }
            }

            //Debug.Log("New parent of " + XMLReadWriter.ConvertToHex(e.eventID) + ": " + XMLReadWriter.ConvertToHex(e.parentID));
        }

        return e;
    }

    /* Generates a unique ID for the event */
    private int GenerateUniqueID() {
        int i;

        //Loops until a unique id is found
        do {
            i = UnityEngine.Random.Range(int.MinValue, int.MaxValue - 1);
            /* MaxValue used as signifier of no event ID
            MaxValue -1 used for unit testing */
        } while (EventMaster.GetEvent(i) != null);

        return i;
    }

    #endif
}