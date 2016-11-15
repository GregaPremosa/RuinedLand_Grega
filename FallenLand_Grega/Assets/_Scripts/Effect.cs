using UnityEngine;
using System.Collections.Generic;
using System;

abstract public class Effect
{
    //variables
    private string effectName;
    private bool isDurationBased;
    private int duration; //if duration is set to a positive number, it will go toward 0. When it hits 0, it self destroys.
    //default constructor
    public Effect() : base()
    {
        effectName = "";
        isDurationBased = false;
        duration = -1;
    }
    //set
    public void setDuration(int newDuration) { duration = newDuration; }
    public void setIsDurationBased(bool newIsDurationBased) { isDurationBased = newIsDurationBased; }
    public void setEffektName(string newName) { effectName = newName; }
    //get
    public int getDuration() { return duration; }
    public bool getIsDurationBased() { return isDurationBased; }
    public string getEffektName() { return effectName; }
    //decrement duration by 1 toward 0
    public void decrementDuration()
    {
        if (isDurationBased == true)
        {
            duration--;
            if (duration < 0)
            {
                duration = 0;
            }
        }
    }
    //this will be used in every effect to call it.
    abstract public void activate(Unit unit);
}



    //SPELLS & ENVIRONMENT EFFECTS
//unit moves slower
public class Slow : Effect
{
    private float speedMultiplier = 0.75f;

    public Slow() : base()
    {
        setEffektName("Slowness");
        setIsDurationBased(true);
        setDuration(2);
    }

    override public void activate(Unit unit)
    {
        unit.setCurrentMovement(Mathf.RoundToInt(unit.getCurrentMovement() * speedMultiplier));
    }
}

//unit moves faster
public class Haste : Effect
{
    private float speedMultiplier = 1.3f;

    public Haste() : base()
    {
        setEffektName("Haste");
        setIsDurationBased(true);
        setDuration(2);
    }

    public override void activate(Unit unit)
    {
        unit.setCurrentMovement( Mathf.RoundToInt(unit.getCurrentMovement() * speedMultiplier) );
    }
}

//unit takes damage over time
public class Poison : Effect
{
    public Poison() : base()
    {
        setEffektName("Poison");
        setIsDurationBased(true);
        setDuration(3);
    }
    public override void activate(Unit unit)
    {
        int damageTick = Mathf.RoundToInt((unit.getHealth() * unit.getCurrentCount()) / unit.getCount());
        unit.takeDamage(damageTick);
    }
}

//increase current defense
public class stoneSkin : Effect
{
    private float bonusArmour = 0.2f;
    public stoneSkin() : base()
    {
        setEffektName("Stone Skin");
        setIsDurationBased(true);
        setDuration(3);
    }
    public override void activate(Unit unit)
    {
        unit.setCurrentArmour( Mathf.RoundToInt(unit.getCurrentArmour() + (unit.getCurrentArmour() * bonusArmour)) );
    }
}

//increase current attack
public class imbueWeapon : Effect
{
    private float multiplier = 0.2f;

    public imbueWeapon() : base()
    {
        setEffektName("Imbued Weapon");
        setIsDurationBased(true);
        setDuration(3);
    }
    public override void activate(Unit unit)
    {

        unit.setCurrentAttack( Mathf.RoundToInt( unit.getCurrentAttack()+unit.getCurrentAttack()*multiplier ) );
    }
}

public class empowerAlly : Effect
{
    private float multiplier = 0.3f;

    public empowerAlly() : base()
    {
        setEffektName("Empowered");
        setIsDurationBased(true);
        setDuration(1);
    }
    public override void activate(Unit unit)
    {
        //increase initiative or 1 turn
        unit.setCurrentInitiative( Mathf.RoundToInt(unit.getCurrentInitiate()*multiplier) );
    }
}



    //PRESENCE EFFECTS
//THIS CONCEPT SHALL BE ADDED LATER
//abstract class, that separates each aura
abstract public class PresenceMagic : Effect
{
}

public class PresenceMage : Effect
{
    public override void activate(Unit unit)
    {
        //naredi, da vsi ally-i imajo nekaj
    }
}

public class PresenceChanter : Effect
{
    public override void activate(Unit unit)
    {
        //naredi, da vsi ally-i imajo nekaj
    }
}