using System.Collections;
using UnityEngine;
public class Survivor {

	/* Survivor's Static Variables */
	public static readonly float ACOHOLIC_DAILY_CONSUMPTION = 2f;
	public static readonly float CAR_JOCKEY_FUEL_SAVINGS = 2f;
	public static readonly int GAMBLER_LUCK_DAMAGE_CHANCE_OFFSET = 15;
	public static readonly string CYBERPUNK_SHOP_EVENT = "DC2C9F90";

	private static readonly int MAX_HP = 100;


	/* Survivor's Variables */
	public readonly int uniquePrime;
	public readonly string Biography, GivenName, PortraitImage, Surname;
	private int age;
	private int health;
	private Traits[] traits;


	/* Public Get Methods */
	public string Age { get { return age.ToString(); } }
	public int HP { get { return health; } }
	public string[] CharacterTraits { get { return new string[2] {GetHumanReadableTrait(traits[0]), GetHumanReadableTrait(traits[1])}; } }


	/* Constructor */
	public Survivor(int uniquePrime, int age, string givenName, string surname, string biography, string portraitImage, Traits[] traits) {
		this.uniquePrime = uniquePrime;
		this.age = age;
		this.GivenName = givenName;
		this.Surname = surname;
		this.Biography = biography;
		this.PortraitImage = portraitImage;
		this.health = MAX_HP;
		this.traits = traits;
	}


	/* Method for checking if a Survivor bears the required trait */
	public bool HasTrait(Traits trait) {
		foreach(Traits t in traits) {
			if(t == trait) return true;
		}

		return false;
	}


	/* Method to tell if a player is dead */
	public bool IsDead() {
		return health <= 0;
	}


	/* Method for modifying a Survivor's HP */
	public bool ModifyHP(int points, bool canDie) {
		int total = health + points;
		health = Mathf.Clamp(total, (canDie) ? 0 : 1, MAX_HP);
		return total > 0 && total <= MAX_HP;
	}

	public static Survivor GetByTrait(Traits t) {
		foreach(Survivor s in GameMaster.Survivors) {
			if(s.HasTrait(t)) return s;
		}

		return null;
	}

	public static Survivor[] DefineSurvivors() {
		return new Survivor[] {
			new Survivor(2, 40, "Reed", "Willby", "Reed is a short irish man who has quite a way with words, able to talk his way out of the stickiest of situations. Even talked the recruiter into giving him a raise after meeting. We found him swindling people in bars and wasting his talents, so he was pretty much hired on the spot as communications. I never asked him about his past, professional integrity and all that but that man definitely has his demons. Hopefully they don't come to bite him in the ass.", "char_01", new Traits[] {Traits.CHARLATAN, Traits.EXBANDIT}),
			new Survivor(3, 22, "Meg", "Render", "Meg is a typical looking computer nerd or at least that’s what I thought when we first met. Her technical prowess was unparalleled. Reminded me of someone weaving magic when we had the trial run. Yet, what truly surprised me was she was always willing to put herself on the line for others. Even caught her feeding her rations to hounders on one occasion. I’m sure she will be a great fit for the team.", "char_02", new Traits[] {Traits.COMPASSIONATE, Traits.CYBERPUNK}),
			new Survivor(5, 38, "Randy", "Moore", "Randy was hired by us after the MSS’ failed attempt at culling the Hounder population. I don’t think he ever quite recovered from it, last I heard he was the only one left in his platoon after a mass of hounders got them. Picked up drinking on the way, but his military skills never left him. Security is hard to come by these days, so we hope he continues doing the right thing.", "char_03", new Traits[] {Traits.ALCOHOLIC, Traits.TRIGGER_HAPPY}),
			new Survivor(7, 27, "Kate", "Hex", "Kate is easily an unlikeable person who demands respect for her talents. I remember during trails when she was able to disassemble a gun without any initial instructions. Says that she just gets how mechanical stuff works. We put her on engine repair and she picked it up instantly along with great driving skills. Good for us, because this expedition team desperately needed a driver and a mechanic. Now we got both.", "char_04", new Traits[] {Traits.CAR_JOCKEY, Traits.PRACTICAL}),
			new Survivor(11, 33, "Miles", "Hannigan", "I have met some weirdos in my time, but Miles takes the cake. He’s always flipping his coin, muttering the correct side every time. I never understood how he did it, and he never told me. Either way, intuition like that is underrated in this line of work. We put him on navigations and he hasn't let us down so far.", "char_05", new Traits[] {Traits.INSIGHTFUL, Traits.GAMBLER}),
			new Survivor(13, 5, "Isaac", "Asimo V1.0", "Isaac is a new technology developed by MSS is an attempt to create a realistic artificial life. When the MSS approached us to take him outside the base for a bit so he can experience the outside world, we didn't realise how ‘realistic’ this AI was. He completely believed he was human. Even ate food and breathed oxygen. Can be a bit childish, but we're hoping he won't be much of a burden.", "char_06", new Traits[] {Traits.ROBOT, Traits.THINKS_HES_HUMAN})
		};
	}

	public static bool PartyKilled(Survivor[] survivors) {
		foreach(Survivor s in survivors) {
			if(!s.IsDead()) return false;
		}

		return true;
	}

	public static bool FullParty() {
		foreach(Survivor s in GameMaster.Survivors) {
			if(s.IsDead()) return false;
		}

		return true;
	}

	public static string GetHumanReadableTrait(Traits t) {
		switch(t) {
			case Traits.CAR_JOCKEY:
				return "CAR JOCKEY";
			case Traits.EXBANDIT:
				return "EX-BANDIT";
			case Traits.THINKS_HES_HUMAN:
				return "THINKS HE'S HUMAN";
			case Traits.TRIGGER_HAPPY:
				return "TRIGGER HAPPY";
			default:
				return t.ToString();
		}
	}

	public static bool PartyHasTrait(Traits trait) {
		if(trait == Traits.NONE) return true;

		foreach(Survivor s in GameMaster.Survivors) {
			if(!s.IsDead() && s.HasTrait(trait)) return true;
		}

		return false;
	}

	public static Survivor[] GetByHealth() {
		ArrayList sorted = new ArrayList();

		for(int i = 0; i < GameMaster.Survivors.Length; i++) {
			if(!GameMaster.Survivors[i].IsDead()) {
				int j;
				for(j = 0; sorted.Count != 0 && j < sorted.Count && GameMaster.Survivors[i].HP > ((Survivor)sorted[j]).HP; j++);
				sorted.Insert(j, GameMaster.Survivors[i]);
			}
		}

		return (Survivor[])sorted.ToArray(typeof(Survivor));
	}
}