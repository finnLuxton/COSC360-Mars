using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Event {

	/* Event Variables */
	public readonly bool allowLethalBlows, isEventHead, lockedEvent, repeatableEvent;
	public readonly float chanceOfDamage;
	public readonly int eventID, maxDamage, minDamage, damageProduct;
	public readonly string imageSource, text;
	public readonly Choice[] choices;
	public readonly Traits requiredTrait;
	public int parentID;


	/* Simple Constructor */
	public Event(int eventID, string text, Choice[] choices) 
	: this(eventID, int.MaxValue, true, false, true, text, Traits.NONE, "", 0f, 0, 0, 1, false, choices) {

	}

	/* Full Constructor */
	public Event(int eventID, int parentID, bool isEventHead, bool lockedEvent, bool repeatableEvent, string text, Traits requiredTrait, string imageSource, float chanceOfDamage, int minDamage, int maxDamage, int damageProduct, bool allowLethalBlows, Choice[] choices) {
		this.eventID = eventID;
		this.parentID = parentID;
		this.isEventHead = isEventHead;
		this.repeatableEvent = repeatableEvent;
		this.lockedEvent = lockedEvent;
		this.text = text;
		this.requiredTrait = requiredTrait;
		this.imageSource = imageSource;
		this.chanceOfDamage = chanceOfDamage;
		this.minDamage = minDamage;
		this.maxDamage = maxDamage;
		this.damageProduct = damageProduct;
		this.allowLethalBlows = allowLethalBlows;
		this.choices = choices;
	}

	/* Method to get the list of choices available to the player */
	public Choice[] GetChoices() {
		ArrayList availableChoices = new ArrayList(GameMaster.MAX_ACTIONS);

		foreach(Choice c in choices) {
			if(Survivor.PartyHasTrait(c.requiredTrait)) {
				availableChoices.Add(c);
			}
		}

		return (Choice[])availableChoices.ToArray(typeof(Choice));
	}

	/* Method to load the event instance */
	public void Load() {
		if(GameMaster.IN_GAME) {
			GameMaster.CurrentEvent = this;
			GameMaster.UpdateActions();
			GameMaster.EventImage = imageSource;

			if(!repeatableEvent) EventMaster.LockEvent(this);

			History.LogSurvivorData();
			
			foreach(Survivor s in GameMaster.Survivors) {
				if(damageProduct % s.uniquePrime == 0) {
					if(s.HasTrait(Traits.GAMBLER) && Random.Range(0f, 1f) < chanceOfDamage - Survivor.GAMBLER_LUCK_DAMAGE_CHANCE_OFFSET) {
						s.ModifyHP(Random.Range(minDamage, maxDamage + 1), allowLethalBlows);
					} else if(damageProduct % s.uniquePrime == 0 && Random.Range(0f, 1f) < chanceOfDamage) {
						s.ModifyHP(Random.Range(minDamage, maxDamage + 1), allowLethalBlows);
					}
				}
			}

			History.MakeHealthEntry();

			//If the event HAS text, load the event screen like normal
			if(text.Length > 0) SceneManager.LoadScene("UI_Screen1");

			//If the event has NO text AND ONLY ONE choice, automatically activate that choice
			else if(choices.Length == 1) choices[0].ActivateChoice();

			//If the event has NO text AND MORE THAN ONE choice, loop back to the choices screen
			else if(choices.Length > 1) SceneManager.LoadScene("UI_Screen3");

			//Otherwise, the event must be invalid (should never happen thanks to error checking in the evend editor)
			else Debug.LogError("INVALID EVENT! Event has no text or choices!");
		}
	}

	public bool LinkedTo(int searchID) {
		foreach(Choice c in choices) {
			if(searchID == c.successEventID || searchID == c.failureEventID) return true;
		}

		return false;
	}

	/* Method to print event summary */
	public string PrintEventSummary() {
		return "Event_" + XMLReadWriter.ConvertToHex(eventID) + "   \t\"" + text.Substring(0, (text.Length < 50) ? text.Length : 50) + "\"";
	}

	/* Method to print event details */
	public override string ToString() {
		StringBuilder s = new StringBuilder();
		s.Append("Event_" + XMLReadWriter.ConvertToHex(eventID) + ".xml\n");
		s.Append("Event Head? " + isEventHead.ToString() + "\n");
		s.Append("Locked Event? " + lockedEvent.ToString() + "\n");
		s.Append("Repeatable Event? " + repeatableEvent.ToString() + "\n");
		s.Append("Image source: " + imageSource + "\n");
		s.Append("Event Text: " + text + "\n");
		s.Append("Survivor Damage:\n");
		s.Append("\tChance: " + chanceOfDamage.ToString() + "\n");
		s.Append("\tMin: " + minDamage.ToString() + "\n");
		s.Append("\tMax: " + maxDamage.ToString() + "\n");
		s.Append("\tLethal Blows? " + allowLethalBlows.ToString() + "\n\n");

		for(int i = 0; i < choices.Length; i++) {
			s.Append("Choice " + i + ":\n");
			s.Append(choices[i].ToString());
		}
		
		return s.ToString();
	}
}