using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class Choice {

	/* Chance that the choice will be a success between 0f (0%) and 1f (100%) */
	public readonly float chanceOfSuccess;

	/* Events to chain depending on results */
	public readonly int failureEventID, time, successEventID, unlocksEventID;

	/* Text shown to the player to represent the choice */
	public readonly string text;

	/* The associated cost of this action, regardless of result */
	public readonly Supplies cost;

	/* Required trait for a character to have in order to display this choice */
	public readonly Traits requiredTrait;


	/* Simple constructor for adding a choice with no required trait or event chaining */
	public Choice(string text, float chanceOfSuccess, Supplies cost)
	: this(text, chanceOfSuccess, 0, cost, Traits.NONE, int.MaxValue, int.MaxValue, int.MaxValue) {

	}

	/* More complex constructor for adding a choice with a
	specific success and failure event and a customised required trait */
	public Choice(string text, float chanceOfSuccess, int time, Supplies cost, Traits requiredTrait, int successEventID, int failureEventID, int unlocksEventID) {
		this.text = text;
		this.chanceOfSuccess = chanceOfSuccess;
		this.time = time;
		this.cost = cost;
		this.requiredTrait = requiredTrait;
		this.successEventID = successEventID;
		this.failureEventID = failureEventID;
		this.unlocksEventID = unlocksEventID;
	}

	/* Method to run if a choice is selected */
	public void ActivateChoice() {
		Assert.IsNotNull(cost);
		Assert.IsTrue(chanceOfSuccess >= 0f && chanceOfSuccess <= 1f);

		History.LogResources();
		
		//Applies the cost of the choice against the resources the player has (and clamps result to positive values)
		GameMaster.resources = +(GameMaster.resources + cost);

		History.MakeResourceEntry();
		
		//Updates the days passed for when this chain of events is finished
		if(time > 0) {
			GameMaster.RoverPosition += time;
		} else {
			GameMaster.AdvanceDays(-time, false);
		}
		
		//Adds any unlocks any chained event for use in random encounter system !do we want to not access success/failure events if they are locked?
		if(unlocksEventID != int.MaxValue) {
			EventMaster.UnlockEvent(EventMaster.GetEvent(unlocksEventID));
		}

		//Gets the next chained eventID (if there is one) based on success
		int eventID = (Random.Range(0f, 1f) < chanceOfSuccess) ? successEventID : failureEventID;

		//Reads the Event if it exists from file and loads
		if(eventID != int.MaxValue) {
			Event e = EventMaster.GetEvent(eventID);
			
			//Only loads the chained event if it is not locked
			if(!e.lockedEvent) e.Load();

		} else {
			//Otherwise, returns control to the GameMaster
			GameMaster.Run(false);
		}
	}

	/* Method to check if the choice is selectable */
	public bool IsSelectable() {
		return (GameMaster.resources + cost) >= 0.0f;
	}

	/* Method to print choice details */
	public override string ToString() {
		StringBuilder s = new StringBuilder();
		s.Append("\tChoice Text: " + text + "\n");
		s.Append("\tChance of Success: " + chanceOfSuccess + "\n");
		s.Append("\tRequires: " + requiredTrait.ToString() + "\n");
		s.Append("\tSuccess Event ID: " + XMLReadWriter.ConvertToHex(successEventID) + "\n");
		s.Append("\tFailure Event ID: " + XMLReadWriter.ConvertToHex(failureEventID) + "\n");
		s.Append("\tUnlocks Event ID: " + XMLReadWriter.ConvertToHex(unlocksEventID) + "\n");
		s.Append("\tTime: " + time + "(days)\n");
		s.Append("\tCost:\n" + cost + "\n\n");
		
		return s.ToString();
	}
}