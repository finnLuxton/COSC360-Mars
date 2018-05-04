using System;
using System.Text;
using UnityEngine;
public class History {

	private static string[] history;
	private static int time;
	private static int index;
	private static Supplies lastResources;
	private static int[] lastSurvivorData;
	private static int[] lastSurvivorID;
	private static string[] resourceNames = { "alcohol", "fuel", "oxygen", "rations" };

	public static readonly string WHITE = "#000000ff";
	public static readonly string YELLOW = "#ffdd00ff";
	public static readonly string GREEN = "#00ff00ff";
	public static readonly string RED = "#ff0000ff";
	public static readonly string BLOOD_RED = "#AF111Cff";

	public static void Init() {
		history = new string[200];
		for(int i = 0; i < history.Length; i++) {
			history[i] = "";
		}

		index = 0;
		time = 1;
	}

	public static string Get() {
		StringBuilder s = new StringBuilder();
		int i = index - 1;
		if(i < 0) i += history.Length;

		s.Append((history[index].Length > 2 && history[index].Substring(0, 2).Equals("\n\n")) ? history[index].Substring(2) : history[index]);
		while(--i != index) {
			if(i < 0) i += history.Length;
			s.Append(history[i]);
		}

		return s.ToString();
	}

	public static int GetDay() {
		return time;
	}

	public static void NewDay() {
		if(time > 0) {
			history[index++ % history.Length] = "\n\n------------------------------------\nDay " + time.ToString() + "\n\n";
		}
		
		time++;
	}

	public static void MakeHealthEntry() {
		int diff;

		foreach(Survivor s in GameMaster.Survivors) {
			diff = GetHPDifference(s);

			if(diff > 0) AddManualEntry(s.GivenName + " healed " + diff.ToString() + "HP!", GREEN);
			else if(diff < 0) {
				AddManualEntry(s.GivenName + " was hurt! (" + diff.ToString() + "HP)", RED);
				if(s.IsDead()) AddManualEntry(s.GivenName + " was killed!", BLOOD_RED);
			}
		}
	}

	public static void MakeResourceEntry() {
		Supplies diff = GetResourceDifference();
		float[] diffArray = { diff.Alcohol, diff.Fuel, diff.Oxygen, diff.Rations };

		for(int i = 0; i < 4; i++) {
			if(diffArray[i] > 0) AddManualEntry("We gained " + diffArray[i].ToString("F0") + " units of " + resourceNames[i] + ".", GREEN);
			else if(diffArray[i] < 0) AddManualEntry("We used " + diffArray[i].ToString("F0") + " units of " + resourceNames[i] + ".", RED);
		}
	}

	public static void AddManualEntry(string message, string colour) {
		history[index++ % history.Length] = "<color=" + colour + ">" + message + "</color>\n";
	}

	public static void LogResources() {
		lastResources = new Supplies(GameMaster.resources);
	}

	public static void LogSurvivorData() {
		lastSurvivorData = new int[GameMaster.Survivors.Length];
		lastSurvivorID = new int[GameMaster.Survivors.Length];

		for(int i = 0; i < GameMaster.Survivors.Length; i++) {
			lastSurvivorID[i] = GameMaster.Survivors[i].uniquePrime;
			lastSurvivorData[i] = GameMaster.Survivors[i].HP;
		}
	}

	public static Supplies GetResourceDifference() {
		return GameMaster.resources - lastResources;
	}

	public static int GetHPDifference(Survivor s) {
		return s.HP - lastSurvivorData[Array.IndexOf(lastSurvivorID, s.uniquePrime)];
	}
}
