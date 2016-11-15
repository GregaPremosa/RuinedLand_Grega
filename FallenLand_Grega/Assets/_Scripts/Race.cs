using UnityEngine;
using System.Collections.Generic;

public class Race
{
    //variables
    private string label;
    private int raceIndex;
    //Race index: is connected to models and generaly its easier to work with integer than string - faster and less work, but requires that we know what integer represents:
    //0 - Human
    //1 - Elf
    //2 - Dwarf
    //3 - Orc
    public Race(): base()
    {
        raceIndex = 0; //default to Human
        label = "";
    }
    //set
    public void setLabel(string newLabel)
    {
        label = newLabel;
        if (label == "Human") { raceIndex = 0; }
        else if (label == "Elf") { raceIndex = 1; }
        else if (label == "Dwarf") { raceIndex = 2; }
        else if (label == "Orc") { raceIndex = 3; }
        else { Debug.LogError("Race - Napaka pri nastavljanju label indexov...ERROR"); raceIndex = 0; }
    }
    //get
    public string getLabel() { return label; }
    public int getRaceIndex() { return raceIndex; }
}
