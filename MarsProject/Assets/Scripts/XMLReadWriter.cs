using System;
using System.Collections;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class XMLReadWriter : MonoBehaviour {

    /* Static variables for helping to find and name events */
    public static readonly string path = "Assets/Resources/";
    public static readonly string prefix = "Event_";
    public static readonly string type = ".xml";


    /*************************************************************/
    /***                 STATIC CLASS METHODS                  ***/
    /*************************************************************/


    /* Converts hexadecimal eventID string into a decimal integer */
    public static int ConvertToDec(string eventID) {
        Assert.IsNotNull(eventID);
        Assert.IsFalse(eventID.Equals(""));

        return int.Parse(eventID, System.Globalization.NumberStyles.HexNumber);
    }


    /* Converts decimal eventID into a hexadecimal string */
    public static string ConvertToHex(int eventID) {
        return eventID.ToString("X8");
    }


    /* Unit Test */
    public static void Test() {
        int eventID = int.MaxValue - 1;

        Choice[] choices = {
            new Choice("choice 1", 0.8f, new Supplies(-10f, 10f, 80, -8000)),
            new Choice("choice 2", 0.6f, 10, new Supplies(), Traits.ALCOHOLIC, 254, 256, 257),
            new Choice("choice 3", 0.4f, new Supplies()),
            new Choice("choice 4", 0.3f, new Supplies()),
            new Choice("choice 5", 1f, new Supplies())
        };

        Event e = new Event(eventID, "TEST EVENT", choices);

        WriteEvent(e);

        e = ReadEvent(eventID);
    }


    /* Method for reading events from file */
    public static Event ReadEvent(int eventID) {
        ArrayList choices = new ArrayList();
        bool allowLethalBlows = true;
        bool eventHead = true;
        bool lockedEvent = false;
        bool repeatableEvent = true;
        float alcohol = 0f;
        float chanceOfDamage = 0f;
        float chanceOfSuccess = 0f;
        float fuel = 0f;
        float oxygen = 0f;
        float rations = 0f;
        int damageProduct = 1;
        int days = 0;
        int failureEventID = int.MaxValue;
        int maxDamage = 0;
        int minDamage = 0;
        int parentID = int.MaxValue;
        int successEventID = int.MaxValue;
        int unlocksEventID = int.MaxValue;
        string choiceText = "";
        string eventText = "";
        string imageSource = "";
        TextAsset textAsset;
        Traits choiceTrait = Traits.NONE;
        Traits eventTrait = Traits.NONE;
        XmlDocument xmlDocument = new XmlDocument();;
        XmlNode eventNode;
        XmlNode damageNode;
        XmlNodeList nodeList;

        //Load XML document as a TextAsset from Resources folder
        textAsset = Resources.Load(prefix + ConvertToHex(eventID)) as TextAsset;
        
        //Returns null if file is empty or cannot be found
        if(textAsset == null || textAsset.text.Equals("")) return null;

        //Creates a new XmlDocument object and loads the TextAsset into it
        xmlDocument.LoadXml(textAsset.text);

        //Selects the top node - the Event element
        eventNode = xmlDocument.DocumentElement.SelectSingleNode("/Event");

        //Gets the parent, eventHead, lockedEvent, repeatableEvent, imageSource and eventText values respectively
        parentID = int.Parse(eventNode.SelectSingleNode("Parent").InnerText);
        eventHead = bool.Parse(eventNode.SelectSingleNode("EventHead").InnerText);
        lockedEvent = bool.Parse(eventNode.SelectSingleNode("LockedEvent").InnerText);
        repeatableEvent = bool.Parse(eventNode.SelectSingleNode("RepeatEvent").InnerText);
        imageSource = eventNode.SelectSingleNode("Image").Attributes.GetNamedItem("source").Value;
        eventText = eventNode.SelectSingleNode("Text").InnerText;

        //Gets the eventTrait and converts it into type Traits
        try {
            eventTrait = (Traits)Enum.Parse(typeof(Traits), eventNode.SelectSingleNode("RequiredTrait").InnerText);
        } catch (ArgumentException) {
            Debug.LogError("ArgumentException: The required trait of 'Event_" + ConvertToHex(eventID) + "' is not a known Traits definition!");
            Debug.LogError("Loading as default Traits.NONE");
            eventTrait = Traits.NONE;
        }
        
        //Gets the Damage node and it's attributes
        damageNode = eventNode.SelectSingleNode("Damage");
        chanceOfDamage = float.Parse(damageNode.Attributes.GetNamedItem("chance").Value);
        minDamage = int.Parse(damageNode.Attributes.GetNamedItem("min").Value);
        maxDamage = int.Parse(damageNode.Attributes.GetNamedItem("max").Value);
        damageProduct = int.Parse(damageNode.Attributes.GetNamedItem("damageProduct").Value);
        allowLethalBlows = bool.Parse(damageNode.Attributes.GetNamedItem("allowLethalBlows").Value);

        //Gets a list of choice nodes
        nodeList = eventNode.SelectNodes("Choice");

        //A new arraylist to store the choices in
        choices = new ArrayList();

        //Cycles through each choice and stores them in an ArrayList
        foreach(XmlNode n in nodeList) {
            
            //Gets the attributes of the choice node
            chanceOfSuccess = float.Parse(n.Attributes.GetNamedItem("chance").Value);
            alcohol = float.Parse(n.Attributes.GetNamedItem("alcohol").Value);
            fuel = float.Parse(n.Attributes.GetNamedItem("fuel").Value);
            oxygen = float.Parse(n.Attributes.GetNamedItem("oxygen").Value);
            rations = float.Parse(n.Attributes.GetNamedItem("rations").Value);
            days = int.Parse(n.Attributes.GetNamedItem("time").Value);

            //Gets the choiceTrait and converts it into type Traits
            try {
                choiceTrait = (Traits)Enum.Parse(typeof(Traits), n.SelectSingleNode("RequiredTrait").InnerText);
            } catch (ArgumentException) {
                Debug.LogError("ArgumentException: The required trait of a choice in 'Event_" + ConvertToHex(eventID) + "' is not a known Traits definition!");
                Debug.LogError("Loading as default Traits.NONE");
                choiceTrait = Traits.NONE;
            }

            //Gets the Choice Text
            choiceText = n.SelectSingleNode("Text").InnerText;

            //Gets the chained EventIDs and converts them intto ints
            successEventID = ConvertToDec(n.SelectSingleNode("SuccessEvent").Attributes.GetNamedItem("id").Value);
            failureEventID = ConvertToDec(n.SelectSingleNode("FailureEvent").Attributes.GetNamedItem("id").Value);
            unlocksEventID = ConvertToDec(n.SelectSingleNode("UnlocksEvent").Attributes.GetNamedItem("id").Value);

            //Add the choice to the ArrayList
            choices.Add(new Choice(choiceText, chanceOfSuccess, days, new Supplies(alcohol, fuel, oxygen, rations), choiceTrait, successEventID, failureEventID, unlocksEventID));
        }

        //Only if it escapes the loop will it return a new Event with all the information it requires
        return new Event(eventID, parentID, eventHead, lockedEvent, repeatableEvent, eventText, eventTrait, imageSource, chanceOfDamage, minDamage, maxDamage, damageProduct, allowLethalBlows, (Choice[]) choices.ToArray(typeof(Choice)));
    }


    /* Method for reading all available event ID numbers from file */
    public static void ReadEventList() {
        Event e;
        int eventID;
        XmlNodeList nodeList;
        TextAsset textAsset;
        bool rewrite = false;
        XmlDocument xmlDocument = new XmlDocument();


        //Load XML document as a TextAsset from Resources folder
        textAsset = Resources.Load("EventList") as TextAsset;

        //If the file couldn't be found, writes a new one before continuing
        if(textAsset == null || textAsset.text.Equals("")) {
            #if UNITY_EDITOR
            Debug.LogError("Could not find EventList.xml! Creating new file...");
            WriteEventList();
            textAsset = Resources.Load("EventList") as TextAsset;
            #else
            Debug.LogError("Could not find EventList.xml!");
            #endif
        }

        //Creates a new XmlDocument object and loads the TextAsset into it
        xmlDocument.LoadXml(textAsset.text);

        //Selects the top node - the EventList element
        nodeList = xmlDocument.DocumentElement.SelectSingleNode("/EventList").SelectNodes("Event");

        //Gets the EventIDs and adds them to the EventMaster's event lists without rewriting the file
        foreach(XmlNode n in nodeList) {
            //Gets the eventID
            eventID = ConvertToDec(n.Attributes.GetNamedItem("id").Value);

            //Checks if ID is unique in the EventList (so far)
            if(EventMaster.GetEvent(eventID) == null) {
                //Reads in the event
                e = ReadEvent(eventID);

                //Adds a valid event to the EventMaster's list
                if(e != null) EventMaster.AddEvent(e, false);
                else {
                    //If an associated file could not be found/was blank, set rewrite to true, report and skip this eventID
                    Debug.LogError("'Event_" + ConvertToHex(eventID) + "'.xml was blank or missing!\nWill rewrite EventList.xml after this read to disinclude this file.");
                    rewrite = true;
                    continue;
                }
            } else {
                Debug.LogError("Duplicate eventID '" + ConvertToHex(eventID) + "' found!.\nWill rewrite EventList.xml after this read to remove duplicates.");
                rewrite = true;
            } 
        }

        //If duplicate ID's were found in the file, rewrite the file to not include them
        if(rewrite) WriteEventList();
    }


    /* Method for writing events to file */
    public static void WriteEvent(Event e) {
        #if UNITY_EDITOR

        Assert.IsNotNull(e);
        Assert.IsNotNull(e.text);
        Assert.IsNotNull(e.imageSource);

        //Creates an XmlTextWriter object with UTF8 encoding at the specified path
        XmlTextWriter w = new XmlTextWriter(path + prefix + ConvertToHex(e.eventID) + type, new UTF8Encoding(false));

        //Generates the XML declaration with the version "1.0"
        w.WriteStartDocument(true);

        //Defines the formatting and indentation of the document
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        //Writes the Event element
        w.WriteStartElement("Event");

        //Writes the Parent element and attribute
        w.WriteStartElement("Parent");
        w.WriteString(e.parentID.ToString());
        w.WriteEndElement();

        //Writes the EventHead element and attribute
        w.WriteStartElement("EventHead");
        w.WriteString(e.isEventHead.ToString());
        w.WriteEndElement();

        //Writes the LockedEvent element and attribute
        w.WriteStartElement("LockedEvent");
        w.WriteString(e.lockedEvent.ToString());
        w.WriteEndElement();

        //Writes the RepeatEvent element and attribute
        w.WriteStartElement("RepeatEvent");
        w.WriteString(e.repeatableEvent.ToString());
        w.WriteEndElement();

        //Writes the Image element and attribute
        w.WriteStartElement("Image");
        w.WriteAttributeString("source", e.imageSource);
        w.WriteEndElement();

        //Writes the Event>Text element and contents
        w.WriteStartElement("Text");
        w.WriteCData(e.text);
        w.WriteEndElement();

        //Writes RequiredTrait element and contents
        w.WriteStartElement("RequiredTrait");
        w.WriteString(e.requiredTrait.ToString());
        w.WriteEndElement();

        //Writes the Damage element and contents
        w.WriteStartElement("Damage");
        w.WriteAttributeString("chance", e.chanceOfDamage.ToString());
        w.WriteAttributeString("min", e.minDamage.ToString());
        w.WriteAttributeString("max", e.maxDamage.ToString());
        w.WriteAttributeString("damageProduct", e.damageProduct.ToString());
        w.WriteAttributeString("allowLethalBlows", e.allowLethalBlows.ToString());
        w.WriteEndElement();

        //Writes each choice passed to it
        foreach(Choice c in e.choices) {
            Assert.IsNotNull(c.text);

            //Begins the Choice element and writes it's attributes
            w.WriteStartElement("Choice");
            w.WriteAttributeString("chance", 	c.chanceOfSuccess.ToString());
            w.WriteAttributeString("alcohol", 	c.cost.Alcohol.ToString());
            w.WriteAttributeString("fuel", 		c.cost.Fuel.ToString());
            w.WriteAttributeString("oxygen", 	c.cost.Oxygen.ToString());
            w.WriteAttributeString("rations", 	c.cost.Rations.ToString());
            w.WriteAttributeString("time",	 	c.time.ToString());

            //Writes RequiredTrait element and contents
            w.WriteStartElement("RequiredTrait");
            w.WriteString(c.requiredTrait.ToString());
            w.WriteEndElement();

            //Writes the Choice>Text element and contents
            w.WriteStartElement("Text");
            w.WriteCData(c.text);
            w.WriteEndElement();

            //Writes the SuccessEvent element and id attribute
            w.WriteStartElement("SuccessEvent");
            w.WriteAttributeString("id", ConvertToHex(c.successEventID));
            w.WriteEndElement();

            //Writes the FailureEvent element and id attribute
            w.WriteStartElement("FailureEvent");
            w.WriteAttributeString("id", ConvertToHex(c.failureEventID));
            w.WriteEndElement();

            //Writes the UnlocksEvent element and id attribute
            w.WriteStartElement("UnlocksEvent");
            w.WriteAttributeString("id", ConvertToHex(c.unlocksEventID));
            w.WriteEndElement();

            //Closes the Choice attribute
            w.WriteEndElement();
        }

        //Closes the Event element
        w.WriteEndElement();

        //Closes any open elements or attributes and puts the writer back in the Start state
        w.WriteEndDocument();

        //Flush to the underlying stream and close
        w.Flush();
        w.Close();

        Debug.Log("Event written to file! Printout of " + e.ToString());

        #endif
    }


    /* Method for writing new event ID numbers to file (as they are created) */
    public static void WriteEventList() {
        #if UNITY_EDITOR

        //Determines the eventlist file to write
        string file = "EventList.xml";

        //Creates an XmlTextWriter object with UTF8 encoding at the specified path
        XmlTextWriter w = new XmlTextWriter(path + file, new UTF8Encoding(false));

        //Generates the XML declaration with the version "1.0"
        w.WriteStartDocument(true);

        //Defines the formatting and indentation of the document
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        //Writes the EventList element
        w.WriteStartElement("EventList");

        //Writes each eventID in the EventMaster's array list
        for(int i = 0; i < EventMaster.GetEventListLength(); i++) {
            w.WriteStartElement("Event");
            w.WriteAttributeString("id", ConvertToHex(EventMaster.GetEventFromIndex(i).eventID));
            w.WriteEndElement();
        }

        //Closes the EventList element
        w.WriteEndElement();

        //Closes any open elements or attributes and puts the writer back in the Start state
        w.WriteEndDocument();

        //Flush to the underlying stream and close
        w.Flush();
        w.Close();

        Debug.Log(file + " has been updated!");

        #endif
    }
}