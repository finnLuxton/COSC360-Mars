using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class Supplies {
	
	/* Minimum and maximum to clamp values between */
	public static readonly float MIN = -100f;
	public static readonly float MAX = 100f;
	
	
	/* Supplies to store values of */
	public readonly float Alcohol;
	public readonly float Fuel;
	public readonly float Oxygen;
	public readonly float Rations;

	
	/* If no floats provided, defaults to maximum values */
	public Supplies () {
		this.Alcohol = MAX;
		this.Fuel = MAX;
		this.Oxygen = MAX;
		this.Rations = MAX;
	}

	/* Creates a new Supplies objects with identical values to the one provided */
	public Supplies(Supplies other) {
		this.Alcohol = other.Alcohol;
		this.Fuel = other.Fuel;
		this.Oxygen = other.Oxygen;
		this.Rations = other.Rations;
	}

	
	/* When a new instance is made, the floats provided are clamped betweent the min and max before being assigned */
	public Supplies (float Alcohol, float Fuel, float Oxygen, float Rations) {
		this.Alcohol = Mathf.Clamp(Alcohol, MIN, MAX);
		this.Fuel = Mathf.Clamp(Fuel, MIN, MAX);
		this.Oxygen = Mathf.Clamp(Oxygen, MIN, MAX);
		this.Rations = Mathf.Clamp(Rations, MIN, MAX);
	}

	
	/* Method to check for equality */
	public bool Equals(Supplies other) {
		return this.Alcohol == other.Alcohol && this.Fuel == other.Fuel && this.Oxygen == other.Oxygen && this.Rations == other.Rations;
	}

	public float[] ToArray() {
		return new float[4] { Alcohol, Fuel, Oxygen, Rations };
	}

	
	/* Overloads the >= comparison operator. Returns true if all values are greater than f */
	public static bool operator >=(Supplies s, float f) {
		return s.Alcohol >= f && s.Fuel >= f && s.Oxygen >= f && s.Rations >= f;
	}

	
	/* Overloads the <= comparison operator. Returns true if all values are less than f */
	public static bool operator <=(Supplies s, float f) {
		return s.Alcohol <= f && s.Fuel <= f && s.Oxygen <= f && s.Rations <= f;
	}

	
	/* Overloads the + unary operator to camp all values between 0 and MAX */
	public static Supplies operator +(Supplies s) {
		return new Supplies(Mathf.Clamp(s.Alcohol, 0, MAX), Mathf.Clamp(s.Fuel, 0, MAX), Mathf.Clamp(s.Oxygen, 0, MAX), Mathf.Clamp(s.Rations, 0, MAX));
	}

	
	/* Overloads the + binary operator for easy combination of supplies */
	public static Supplies operator +(Supplies s1, Supplies s2) {
		return new Supplies(s1.Alcohol + s2.Alcohol, s1.Fuel + s2.Fuel, s1.Oxygen + s2.Oxygen, s1.Rations + s2.Rations);
	}

	/* Overloads the - binary operator for easy combination of supplies */
	public static Supplies operator -(Supplies s1, Supplies s2) {
		return new Supplies(s1.Alcohol - s2.Alcohol, s1.Fuel - s2.Fuel, s1.Oxygen - s2.Oxygen, s1.Rations - s2.Rations);
	}

	/* Unit Test */
	public static void Test() {
		Supplies s1 = new Supplies();
		Supplies s2 = new Supplies(-10f, 10f, MAX + 10f, MIN - 10f);

		//Checks that equality method works correctly
		Assert.IsTrue((new Supplies()).Equals(new Supplies()));
		Assert.IsFalse(s1.Equals(s2));

		//Checks clamping between max and min when instantiated
		Assert.IsTrue(s2.Equals(new Supplies(-10f, 10f, MAX, MIN)));

		//Checks addition is performed correctly
		s1 = s1 + s2;
		Assert.IsTrue(s1.Equals(new Supplies(MAX - 10f, MAX, MAX, MAX + MIN))); //+MIN b/c MIN is negative

		//Checks clamping to positive values only works correctly
		Assert.IsTrue((+s2).Equals(new Supplies(0f, 10f, MAX, 0f)));
	}
	
	/* Method to print supplies details */
	public override string ToString() {
		StringBuilder s = new StringBuilder();
		s.Append("\t\tAlcohol : " + Alcohol.ToString() + "\t\tFuel : " + Fuel.ToString() + "\n");
		s.Append("\t\tOxygen : " + Oxygen.ToString() + "\t\tRations : " + Rations.ToString() + "\n");
		
		return s.ToString();
	}
}
