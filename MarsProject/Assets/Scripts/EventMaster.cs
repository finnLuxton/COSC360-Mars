using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EventMaster {

	private static ArrayList EventHeads;
	private static int repeatableEventCooldown;
	private static int[] EventHistory;
	private static int EventHistoryIndex;

	/* Two private Arraylists to act as a extendable hashtable */
	private static ArrayList EventList;
	private static ArrayList EventIDList;

	public static void Init() {
		EventHeads = new ArrayList();
		EventList = new ArrayList();
		EventIDList = new ArrayList();
	}
	
	/* Method for adding an event and updating the file */
	public static void AddEvent(Event e) {
		#if UNITY_EDITOR

		AddEvent(e, true);

		#endif
	}

	
	/* Method for adding an event, optionally updating the file. Will not add if e is null */
	public static void AddEvent(Event e, bool writeToFile) {
		if(e != null) {
			EventList.Add(e);
			EventIDList.Add(e.eventID);

			#if UNITY_EDITOR
			if(writeToFile) XMLReadWriter.WriteEventList();
			#endif
		}
	}

	public static void RecursiveParent(Event parent, Event node) {
		Event child;

		foreach(Choice c in node.choices) {
			child = GetEvent(c.successEventID);
			if(child != null && child.parentID != node.eventID) RecursiveParent(node, child);

			child = GetEvent(c.failureEventID);
			if(child != null && child.parentID != node.eventID) RecursiveParent(node, child);
		}

		if(parent != null) node.parentID = parent.eventID;
		XMLReadWriter.WriteEvent(node);
	}
	
	
	/* Method to search the event list and return the relevant event. Returns null if event is not found */
	public static Event GetEvent(int eventID) {
		int index = EventIDList.IndexOf(eventID);
		if(index == -1) return null;
		return (Event)EventList[index];
	}

	public static int GetEventIndex(int eventID) {
		return EventIDList.IndexOf(eventID);
	}

	
	/* Method to retrieve an event via index */
	public static Event GetEventFromIndex(int index) {
		if(index >= EventList.Count) return null;
		return (Event)EventList[index];
	}

	
	/* Returns the event list length */
	public static int GetEventListLength() {
		return EventList.Count;
	}

	/* Returns a random event head */
	public static Event GetRandomEventHead() {
		Event e;
		bool err;
		
		if(EventHeads.Count != 0) {
			do {
				err = false;

				e = (Event)EventHeads[UnityEngine.Random.Range(0, EventHeads.Count)];

				if(!Survivor.PartyHasTrait(e.requiredTrait)) {
					LockEvent(e);
					err = true;
				}
			} while (err || (e.repeatableEvent && EventHistory.Length > 0 && Array.IndexOf(EventHistory, e.eventID) != -1));
		} else {
			Debug.LogError("Run out of events!");
			e = GetEvent(XMLReadWriter.ConvertToDec("84FBE8F5"));
		}

		if(EventHistory.Length > 0) EventHistory[EventHistoryIndex++ % repeatableEventCooldown] = e.eventID;
		return e;
	}

	
	/* Method for permanently deleting events */
	public static void PermanentlyDeleteEvent(int eventID, string path) {
		RemoveEvent(eventID);

		#if UNITY_EDITOR
		System.IO.File.Delete(path);
		XMLReadWriter.WriteEventList();
		#endif
	}

	
	/* Method for reloading an event into the game (assuming the file has changed) */
	public static void ReloadEvent(Event e) {
		EventList[EventIDList.IndexOf(e.eventID)] = e;

		#if UNITY_EDITOR
		AssetDatabase.ImportAsset(XMLReadWriter.path + XMLReadWriter.prefix + XMLReadWriter.ConvertToHex(e.eventID) + XMLReadWriter.type, ImportAssetOptions.ForceUpdate);
		#endif
	}

	
	/* Method for removing events from the lists within the game (but not deleting the file) */
	public static void RemoveEvent(int eventID) {
		EventList.RemoveAt(EventIDList.IndexOf(eventID));
		EventIDList.Remove(eventID);
	}

	public static void SearchForEventHeads() {
		repeatableEventCooldown = 0;

		foreach(Event e in EventList) {
			if(e.isEventHead) {
				EventHeads.Add(e);
				if(e.repeatableEvent) repeatableEventCooldown++;
			}
		}

		repeatableEventCooldown /= 2;

		EventHistory = new int[repeatableEventCooldown];
		EventHistoryIndex = 0;
	}

	public static void LockEvent(Event e) {
		EventHeads.Remove(e);
	}

	public static void UnlockEvent(Event e) {
		if(!EventHeads.Contains(e))	EventHeads.Add(e);
	}
}