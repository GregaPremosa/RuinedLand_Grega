﻿using UnityEngine;
using System.Collections.Generic;
//Unified - version 1
abstract public class Unit
{
    //position on matrix
    private int rowPos;
    private int colPos;

    //set to which player this Unit belongs to
    private Player playerOwner;

    private int health;
    private int currentHealth;
    private int armour;
    private int currentArmour;
    private int attack;
    private int currentAttack;
    private int count;
    private int currentCount;
    private int initiative;
    private int currentInitiative;
    private int movement;
    private int currentMovement;
    private int size; //how many ground blocks unit takes - can be 1(1x1) or 2(2x2)
    private bool rangedMode;

    //these are needed for certain (passive) effects based on race, for stats modification from upgrades, for model spawning(all variables).
    private int upgrade; //this integer holds which upgrade is unit (NEEDS UPDATE - implement how upgrade works for stats, models,effects)
    private Race race; // set which race is unit
    private int type; //type defines what Unit it is:Scout(0), Warrior(1), Archer(2), Mage(3), Chanter(4)

    private GameObject model;
    private GameObject battleIndicator; //here we hold indicator; if its currently selected, spawn proper gameobject, if its enemy unit that can be attack, spawn attack indicator, if its interactable ally, spawn proper gameobject,...
    private Sprite modelImage; //every Unit also has unit, which is used it Map view
    List<Effect> passives; //we can later add these and they will take effects in battle(boost certain unit types, debuff enemies, special unit powers, etc.)
                           //Race raceType //when we add races, we can assign race type here aswell


    //rangedMode/attackMode:
    //false - melee
    //true - ranged


    public Unit() : base()
    {
        colPos = -1;
        rowPos = -1;
        playerOwner = null;
        //changes while taking damage
        health = 0;
        currentHealth = 0;
        count = 0;
        currentCount = 0;
        //go through effects to change this
        attack = 0;
        currentAttack = 0;
        armour = 0; //armour is percentage based: from 0 to 100, it says reduction.
        currentArmour = 0;
        initiative = 0; //percentage based - from 1 to 100
        currentInitiative = 0;
        movement = 0;
        currentMovement = 0;
        //this doesnt changes
        size = 0;

        race = new Race();
        upgrade = 0;
        type = 0;

        passives = new List<Effect>();
        rangedMode = false;
        battleIndicator = null;
        //model = null;
    }
    //set
    //player ownership
    public void setPlayerOwner(Player newPlayerOwner) { playerOwner = newPlayerOwner; }
    //static values
    public void setHealth(int newHealth) { health = newHealth; }
    public void setArmour(int newArmour) { armour = newArmour; }
    public void setAttack(int newAttack) { attack = newAttack; }
    public void setCount(int newCount) { count = newCount; }
    public void setInitiate(int newInitiative) { initiative = newInitiative; }
    public void setAttackMode(bool newRangedMode) { rangedMode = newRangedMode; }
    public void setMovement(int newMovement) { movement = newMovement; }
    public void setSize(int newSize) { size = newSize; }
    public void setUpgrade(int newUpgrade) { upgrade = newUpgrade; }
    public void setRace(string newRace) { race.setLabel(newRace); }
    public void setType(int newType) { type = newType; }
    public void addEffect(Effect newEffect) { passives.Add(newEffect); }
    //current values
    public void setCurrentHealth(int newCurrentHealth) { currentHealth = newCurrentHealth; }
    public void setCurrentArmour(int newCurrentArmour) { currentArmour = newCurrentArmour; }
    public void setCurrentAttack(int newCurrentAttack) { currentAttack = newCurrentAttack; }
    public void setCurrentCount(int newCurrentCount) { currentCount = newCurrentCount; }
    public void setCurrentInitiative(int newCurrentInitiative) { currentInitiative = newCurrentInitiative; }
    public void setCurrentMovement(int newCurrentMovement) { currentMovement = newCurrentMovement; }
    //models & images
    public void setModel(UnitRacesArray modelHolder)
    {
        model = GameObject.Instantiate(modelHolder.arrayBasedOnRace[race.getRaceIndex()].arrayBasedOnType[getType()].arrayBasedOnUpgrade[getUpgrade()]) as GameObject;
    }
    public void setBattleIndicator(GameObject newBattleIndicator) { battleIndicator = newBattleIndicator; }
    public void setSprite(Sprite newModelImage) { modelImage = newModelImage; }
    //get
    //player ownership
    public Player getPlayerOwner() { return playerOwner; }
    //static values
    public int getHealth() { return health; }
    public int getArmour() { return armour; }
    public int getAttack() { return attack; }
    public int getCount() { return count; }
    public int getInitiate() { return initiative; }
    public bool getAttackMode() { return rangedMode; }
    public int getMovement() { return movement; }
    public int getSize() { return size; }
    public int getUpgrade() { return upgrade; }
    public Race getRace() { return race; }
    public int getType() { return type; }
    //current values
    public int getCurrentHealth() { return currentHealth; }
    public int getCurrentArmour() { return currentArmour; }
    public int getCurrentAttack() { return currentAttack; }
    public int getCurrentCount() { return currentCount; }
    public int getCurrentInitiate() { return currentInitiative; }
    public int getCurrentMovement() { return currentMovement; }
    public List<Effect> getPassives() { return passives; }
    //models & images
    public GameObject getModel() { return model; }
    public GameObject getBattleIndicator() { return battleIndicator; }
    public Sprite getSprite() { return modelImage; }

    //prepare Units stats for start battleMode
    public void prepareUnitStats()
    {
        //currentHealth = health;
        currentArmour = armour;
        currentAttack = attack;
        currentInitiative = initiative;
        currentMovement = movement;
        currentCount = count;
    }
    //calculate damage - NOTE: here, you need to take in consideration base damage, effects and armour of enemy unit.
    public void dealDamage(Unit enemyUnit)
    {
        enemyUnit.applyEffects();
        applyEffects();
        //currentAttack = attack;
        int dealDamage = currentAttack;
        dealDamage = dealDamage * getCurrentCount(); //every actual unit inside class Unit does damage
        //Debug.Log("Damage pre reduction: " + dealDamage );
        //Debug.Log("Enemy (RAW) armour: " + enemyUnit.getCurrentArmour() );
        //Debug.Log("Enemy (CALCULATED) armour: " + Mathf.RoundToInt( dealDamage * Mathf.RoundToInt(enemyUnit.getCurrentArmour()) / 100));
        dealDamage = dealDamage - Mathf.CeilToInt(dealDamage * enemyUnit.getCurrentArmour() / 100);
        //Debug.Log("Damage post reduction: " + dealDamage);
        enemyUnit.takeDamage(dealDamage);
    }
    //taking damage - doesnt count armour yet
    public void takeDamage(int damage)
    {
        Debug.Log("Damage to take(pre damage): " + damage + ", currentCount: " + currentCount + ", health: " + currentHealth);
        int damageCounter = damage;
        bool changeState = false;
        while (!changeState && damageCounter>0)
        {
            if (currentCount <= 0) { currentCount = 0; changeState = true; }
            else if (currentCount == 1 && currentHealth == 0) { currentCount = 0; changeState = true; }
            else if (currentCount == 1 && currentHealth > 0) { currentHealth--; damageCounter--; }
            else if (currentCount >= 2 && currentHealth == 0) { currentCount--; currentHealth = health; }
            else if (currentCount >= 2 && currentHealth > 0) { currentHealth--; damageCounter--; }
            else { Debug.Log("It should not come here"); damageCounter--; }
        }
        Debug.Log("Damage to take(post damage): " + damage + ", currentCount: " + currentCount + ", health: " + currentHealth);
        //adjust health after battle - if count is >= 2 and currentHealth is ==0, change currentHealth to health and decrement count
        if ( currentCount >= 2 && currentHealth==0 )
        {
            currentHealth = health;
            currentCount--;
        }
        //set current count into count
        count = currentCount;
    }

    //iterate effects and apply to current stats
    public void applyEffects()
    {
        if (this != null)
        {
            PresenceMagic presenceMagicCheck = new PresenceMagic();
            presenceMagicCheck.activate(this);
        }

        for (int i = 0; i < passives.Count; i++)
        {
            //activate passive effect
            passives[i].activate(this);
            //if passive is duration based, decrement the value aswell
            if (passives[i].getIsDurationBased())
            {
                passives[i].decrementDuration();
            }
            //remove any passive, that's duration is 0
            if (passives[i].getDuration() == 0)
            {
                passives.RemoveAt(i);
            }
        }
    }
    //for location on matrix
    public int getRowPos() { return rowPos; }
    public int getColPos() { return colPos; }
    public void setRowPos(int newPosRow) { rowPos = newPosRow; }
    public void setColPos(int newPosCol) { colPos = newPosCol; }
}
//Melee - Subclass of Unit
abstract public class Melee : Unit
{
    private int attackRange;
    public Melee() : base()
    {
        setAttackMode(false);
        attackRange = 0;
    }
    //set
    public void setAttackRange(int newAttackRange) { attackRange = newAttackRange; }
    //get
    public int getAttackRange() { return attackRange; }
}
//Ranged - Subclass of Unit
abstract public class Ranged : Unit
{
    private int fullAttackRange;
    private bool fullDamageActive; //full damage is done, when unit is in fullAttackRange, otherwise damage is just a quarter of full damage.
    private bool canRangedAttack; //if there is enemy melee unit in 1 block range of ranged unit, then ranged unit is prevented from performing a ranged attack.
    public Ranged() : base()
    {
        setAttackMode(true);
        fullAttackRange = 0;
        canRangedAttack = true;
        fullDamageActive = false;
    }
    //set
    public void setFullAttackRange(int newFullAttackRange) { fullAttackRange = newFullAttackRange; }
    public void setFullDamageActive(bool newFullDamageActive) { fullDamageActive = newFullDamageActive; }
    public void setCanRangedAttack(bool newCanRangedAttack) { canRangedAttack = newCanRangedAttack; }
    //get
    public int getFullAttackRange() { return fullAttackRange; }
    public bool getFullDamageActive() { return fullDamageActive; }
    public bool getCanRangedAttack() { return canRangedAttack; }

    //deal ranged damage - its alot like dealing melee damage, but also take in consideration how much damage should be deal(full attack range is true or false)
    public void dealRangedDamage(Unit enemyUnit)
    {
        //melee
        enemyUnit.applyEffects();
        applyEffects();
        //currentAttack = attack;
        int dealDamage = getCurrentAttack();
        dealDamage = dealDamage * getCurrentCount(); //every actual unit inside class Unit does damage
        //if enemy is not in full attack range, then deal only a part of actual damage
        if (getFullDamageActive() == false)
        {
            Debug.Log("This damage(full) is: " + Mathf.CeilToInt(dealDamage));
            Debug.Log("This damage(quarter) is: " + Mathf.CeilToInt(dealDamage /4));
            dealDamage = Mathf.CeilToInt(dealDamage /4);
            if (dealDamage == 0) { dealDamage = 1; }
        }
        //Debug.Log("Damage pre reduction: " + dealDamage );
        //Debug.Log("Enemy (RAW) armour: " + enemyUnit.getCurrentArmour() );
        //Debug.Log("Enemy (CALCULATED) armour: " + Mathf.RoundToInt( dealDamage * Mathf.RoundToInt(enemyUnit.getCurrentArmour()) / 100));
        dealDamage = dealDamage - Mathf.CeilToInt(dealDamage * enemyUnit.getCurrentArmour() / 100);
        //Debug.Log("Damage post reduction: " + dealDamage);
        enemyUnit.takeDamage(dealDamage);
    }

}
//Warrior - Subclass of Melee
public class Warrior : Melee
{
    public Warrior() : base()
    {
        //type
        setType(1);
        //basic stats
        setHealth(6);
        setCurrentHealth(getHealth());
        setArmour(35);
        setAttack(6);
        setInitiate(10);
        setMovement(3);
        //melee exclusive
        setAttackRange(1);
        setSize(1);
        setAttackMode(false);
    }
}
//Scout - Subclass of Ranged
public class Scout : Melee
{
    public Scout() : base()
    {
        //type
        setType(0);
        //basic stats
        setHealth(4);
        setCurrentHealth(getHealth());
        setArmour(15);
        setAttack(2);
        setInitiate(90);
        setMovement(5);
        //melee exclusive
        setAttackRange(1);
        setSize(1);
        setAttackMode(false);
    }
}
//Chanter - Subclass of Melee
public class Chanter : Melee
{
    public Chanter() : base()
    {
        //type
        setType(4);
        //basic stats
        setHealth(10);
        setCurrentHealth(getHealth());
        setArmour(20);
        setAttack(3);
        setInitiate(20);
        setMovement(2);
        //melee exclusive
        setAttackRange(1);
        setSize(1);
        setAttackMode(false);
    }
}
//Archer - Subclass of Ranged
public class Archer : Ranged
{
    public Archer() : base()
    {
        //type
        setType(2);
        //basic stats
        setHealth(6);
        setCurrentHealth(getHealth());
        setArmour(20);
        setAttack(3);
        setInitiate(50);
        setMovement(3);
        //ranged exlusive
        setAttackMode(true);
        setSize(1);
        setFullAttackRange(6);
    }
}
//Mage - Subclass of Ranged
public class Mage : Ranged
{
    public Mage() : base()
    {
        //type
        setType(3);
        //basic stats
        setHealth(5);
        setCurrentHealth(getHealth());
        setArmour(15);
        setAttack(4);
        setInitiate(40);
        setMovement(2);
        //ranged exclusive
        setAttackMode(true);
        setSize(1);
        setFullAttackRange(8);
    }
}

//for each unit TYPE we store here model of their upgrade: For example, Warrior has upgrade 1,2,3; we store those here. Archer is stored in another simular list, saved inside common array.
[System.Serializable]
public class UnitUpgradeModels
{
    //by default we want maybe 1 possible upgrade per unit - so normal Unit and upgraded unit
    private int numberOfUpgrades = 3; //we can set how many upgrades we allow each unit
    public GameObject[] arrayBasedOnUpgrade;
    private string unitType;
    public UnitUpgradeModels() : base()
    {
        unitType = "";
        arrayBasedOnUpgrade = new GameObject[numberOfUpgrades];
    }
    //set
    public void setUnitType(string newUnitType) { unitType = newUnitType; }
    //get
    public GameObject getUpgradeModel(int upgradeLevel) { return arrayBasedOnUpgrade[upgradeLevel]; }
    public string getUnitType() { return unitType; }
}

[System.Serializable]
public class UnitTypeModels
{
    private const int numberOfUnitTypes = 5; //this can be constant
    private string raceType;
    //default we can set size here - each race will have the SAME ammount of unit types - for current game development we can set it to 5(Warrior,Scout,Mage,Chanter,Archer).
    public UnitUpgradeModels[] arrayBasedOnType;
    public UnitTypeModels() : base()
    {
        arrayBasedOnType = new UnitUpgradeModels[numberOfUnitTypes];
        for (int i = 0; i < numberOfUnitTypes; i++)
        {
            arrayBasedOnType[i] = new UnitUpgradeModels();
        }
        raceType = "";
        arrayBasedOnType[0].setUnitType("Scout");
        arrayBasedOnType[1].setUnitType("Warrior");
        arrayBasedOnType[2].setUnitType("Archer");
        arrayBasedOnType[3].setUnitType("Mage");
        arrayBasedOnType[4].setUnitType("Chanter");
    }

    //set
    public void setRaceType(string newRaceType) { raceType = newRaceType; }
    public void setUnitTypeModel(UnitUpgradeModels newUnitTypeModel, int index) { arrayBasedOnType[index] = newUnitTypeModel; }
    //get
    public string getRaceType() { return raceType; }
    public UnitUpgradeModels getUnitUpgradeModel(int index) { return arrayBasedOnType[index]; }
}

[System.Serializable]
public class UnitRacesArray
{
    public UnitTypeModels[] arrayBasedOnRace;
    public UnitRacesArray() : base()
    {
        //by default, we set races here: Human, Elf, Dwarf, Orc.
        int numOfRaces = 4;
        arrayBasedOnRace = new UnitTypeModels[numOfRaces];
        for (int index = 0; index < numOfRaces; index++)
        {
            arrayBasedOnRace[index] = new UnitTypeModels();
        }
        arrayBasedOnRace[0].setRaceType("Human");
        arrayBasedOnRace[1].setRaceType("Elf");
        arrayBasedOnRace[2].setRaceType("Dwarf");
        arrayBasedOnRace[3].setRaceType("Orc");
    }
}