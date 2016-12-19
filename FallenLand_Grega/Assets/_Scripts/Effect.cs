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
    public void setEffectName(string newName) { effectName = newName; }
    //get
    public int getDuration() { return duration; }
    public bool getIsDurationBased() { return isDurationBased; }
    public string getEffectName() { return effectName; }
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
        setEffectName("Slowness");
        setIsDurationBased(true);
        setDuration(2);
    }

    override public void activate(Unit unit)
    {
        unit.setCurrentMovement( unit.getCurrentMovement()-Mathf.RoundToInt(unit.getCurrentMovement() * speedMultiplier));
    }
}

//unit moves faster
public class Haste : Effect
{
    private float speedMultiplier = 1.3f;

    public Haste() : base()
    {
        setEffectName("Haste");
        setIsDurationBased(true);
        setDuration(2);
    }

    public override void activate(Unit unit)
    {
        unit.setCurrentMovement( unit.getCurrentMovement()+Mathf.RoundToInt(unit.getCurrentMovement() * speedMultiplier) );
    }
}

//unit takes damage over time
public class Poison : Effect
{
    public Poison() : base()
    {
        setEffectName("Poison");
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
        setEffectName("Stone Skin");
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
        setEffectName("Imbued Weapon");
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
        setEffectName("Empowered");
        setIsDurationBased(true);
        setDuration(1);
    }
    public override void activate(Unit unit)
    {
        //increase initiative or 1 turn
        unit.setCurrentInitiative( unit.getCurrentInitiate() + Mathf.RoundToInt(unit.getCurrentInitiate()*multiplier) );
    }
}

//Unit is near a cover - defense increased by a small margin
public class nearCover : Effect
{
    private float multiplier = 0.2f;
    public nearCover() : base()
    {
        setEffectName("nearCover");
        setIsDurationBased(true);
        setDuration(1);
    }
    public override void activate(Unit unit)
    {
        //increase armour by 1 turn
        unit.setCurrentArmour( unit.getCurrentArmour() + Mathf.RoundToInt(unit.getCurrentArmour()*multiplier) );
    }
}

    //PRESENCE EFFECTS
//THIS CONCEPT SHALL BE ADDED LATER
//abstract class, that separates each aura
public class PresenceMagic : Effect
{
    public override void activate(Unit unit)
    {
        //get player ownership of current Unit
        Player playerOfUnit = null;
        playerOfUnit = unit.getPlayerOwner();
        for (int i = 0; i < playerOfUnit.getArrayUnits().Count; i++)
        {
            //if there is mage present, add presenceMage effect to this Unit
            if (playerOfUnit.getArrayUnits()[i].getType() == 3) //check Unit class to see, which unit is what type (as integer) -> 3 is mage
            {
                unit.addEffect(new PresenceMage());
            }
            //if there is chanter present, add presenceChanter effect to this Unit
            else if (playerOfUnit.getArrayUnits()[i].getType() == 4) //chanter is type 4
            {
                unit.addEffect(new PresenceChanter());
            }
        }
    }
}

//if mage is present, then for every round he is still alive/present, heal allied Units (but he cannot ressurect them - increase count)
public class PresenceMage : Effect
{
    public PresenceMage() : base()
    {
        setEffectName("PresenceMage");
        setIsDurationBased(true);
        setDuration(1);
    }
    public override void activate(Unit unit)
    {
        //set multiplier value
        float healingMultiplier = 0.5f;
        //if mage is present, then for every round he is still alive/present, heal allied Units (but he cannot ressurect them - increase count)
        int tryToSetNewHealth = unit.getCurrentHealth() + Mathf.CeilToInt(unit.getCurrentHealth()* healingMultiplier);
        if (tryToSetNewHealth > unit.getHealth()) { tryToSetNewHealth = unit.getHealth(); }
        unit.setCurrentHealth( tryToSetNewHealth );
    }
}

//if chanter is present, increase defense, attack and movement speed
public class PresenceChanter : Effect
{
    public PresenceChanter() : base()
    {
        setEffectName("PresenceChanter");
        setIsDurationBased(true);
        setDuration(1);
    }
    public override void activate(Unit unit)
    {
        //set multipliers
        float attackMultiplier = 0.3f;
        float armourMultiplier = 0.4f;
        float speedMultiplier = 0.3f;
        //increase numbers
        unit.setCurrentAttack( unit.getCurrentAttack() + Mathf.CeilToInt(unit.getCurrentAttack()*attackMultiplier) );
        unit.setCurrentArmour( unit.getCurrentArmour() + Mathf.CeilToInt(unit.getCurrentArmour()*armourMultiplier) );
        unit.setCurrentMovement( unit.getCurrentMovement() + Mathf.CeilToInt(unit.getCurrentMovement()*speedMultiplier) );

    }
}



//BUTTON PRESS DEFEND - increase defense for 15% for 1 turn
public class DefendYourself : Effect
{
    private float multiplier = 0.15f;

    public DefendYourself() : base()
    {
        setEffectName("DefendYourself");
        setIsDurationBased(true);
        setDuration(1);
    }
    public override void activate(Unit unit)
    {
        unit.setCurrentArmour( unit.getCurrentArmour()+Mathf.RoundToInt(unit.getCurrentArmour()*multiplier) );
    }
}