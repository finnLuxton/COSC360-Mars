using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SerialEventRewriter : MonoBehaviour {

	class ErrorList {
		public ArrayList events = new ArrayList();
		public StringBuilder str = new StringBuilder();
		public string label;

		public ErrorList(string label) {
			this.label = label;
		}

		public void Add(string s) { str.Append(s); }
		public void Add(Event e) {
			str.Append(e.PrintEventSummary() + "\n");
			events.Add(e);
		}
		public void Add(string s, Event e) {
			str.Append(s);
			events.Add(e);
		}
		new public string ToString() {
			return str.ToString();
		}
		public static void ReplaceEventList(ErrorList list) {
			if(list.events.Count > 0) {
				EventMaster.Init();

				for(int i = 0; i < list.events.Count; i++) {
					EventMaster.AddEvent((Event)list.events[i], false);
				}

				GameMaster.CurrentEvent = (Event)list.events[0];
				SceneManager.LoadScene("EventEditor");
			}
		}
	}

	private ErrorList[] lists;

	// Use this for initialization
	private void Start () {
		GameMaster.SPECIAL_EDITOR = true;
		EventMaster.Init();
		XMLReadWriter.ReadEventList();
		XMLReadWriter.WriteEventList();

		Event e;
		for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
			e = EventMaster.GetEventFromIndex(i);

			if(e.isEventHead) EventMaster.RecursiveParent(null, e);
			XMLReadWriter.WriteEvent(e);
		}

		lists = new ErrorList[] {
			FindPlaceholderArt("Find Placeholder Art"),
			FindEmptyArtHeads("Find Empty Art Heads"),
			FindOrphanEvents("Find Orphan Events"),
			FindQuestionMarks("Find Question Marks")
		};
		
		foreach(ErrorList list in lists) {
			Debug.Log(list.ToString());
		}
	}

	private void OnGUI() {
		for(int i = 0; i < lists.Length; i++) {
			if(GUI.Button(new Rect(100, 100 + (i * 30), 200, 20), lists[i].label)) {
				ErrorList.ReplaceEventList(lists[i]);
			}
		}
	}

	private ErrorList FindPlaceholderArt(string label) {
		ErrorList list = new ErrorList(label);
		list.Add("Events with placeholder art:\n");

		Event e;
		for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
			e = EventMaster.GetEventFromIndex(i);

			if(e.imageSource.Length > 3 && e.imageSource.Substring(0, 3).Equals("art")) {
				list.Add(e);
			}
		}

		return list;
	}

	private ErrorList FindEmptyArtHeads(string label) {
		ErrorList list = new ErrorList(label);
		list.Add("Event heads with no art:\n");

		Event e;
		for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
			e = EventMaster.GetEventFromIndex(i);

			if(e.isEventHead && e.imageSource.Length == 0) {
				list.Add(e);
			}
		}

		return list;
	}

	private ErrorList FindOrphanEvents(string label) {
		ErrorList list = new ErrorList(label);
		list.Add("Orphan events (not event head and has no known parent):\n");

		Event e;
		for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
			e = EventMaster.GetEventFromIndex(i);

			if(!e.isEventHead && e.parentID == int.MaxValue) {
				if(e.eventID.Equals(GameMaster.NO_FUEL_EVENT)
				|| e.eventID.Equals(GameMaster.OXYGEN_TANK_EVENT)
				|| e.eventID.Equals(GameMaster.SHOP_EVENT)
				|| e.eventID.Equals(Survivor.CYBERPUNK_SHOP_EVENT)
				|| e.eventID.Equals(GameMaster.STARTING_EVENTS[0])
				|| e.eventID.Equals(GameMaster.STARTING_EVENTS[1])
				|| e.eventID.Equals(GameMaster.STARTING_EVENTS[2])
				|| e.eventID.Equals(GameMaster.STARTING_EVENTS[3])
				|| e.eventID.Equals(GameMaster.STARTING_EVENTS[4])
				|| e.eventID.Equals(GameMaster.STARTING_EVENTS[0])) continue;

				list.Add(e);
			}
		}

		return list;
	}

	private ErrorList FindQuestionMarks(string label) {
		ErrorList list = new ErrorList(label);
		list.Add("Event heads with no art:\n");

		Event e;
		for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
			e = EventMaster.GetEventFromIndex(i);

			if(e.text.Contains("?")) {
				list.Add(e);
			} else {
				foreach(Choice c in e.choices) {
					if(c.text.Contains("?")) {
						list.Add(e);
						break;
					}
				}
			}
		}

		return list;
	}

}
