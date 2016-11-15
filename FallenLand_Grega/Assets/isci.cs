using UnityEngine;
using System.Collections.Generic;
//Unified - version 1
abstract public class Unit
{
    private int health;
    private int currentHealth;
    private int armour;
    private int currentArmour;
    private int count;
    private int currentCount;
    private int initiative;
    private int currentInitiative;
    private int movement;
    private int currentMovement;
    private int size; //how many ground blocks unit takes - can be 1(1x1) or 2(2x2)
    private bool rangedMode;
    private GameObject model;
    private Sprite modelImage; //every Unit also has unit, which is used it Map view
    List<Effect> passives; //we can later add these and they will take effects in battle(boost certain unit types, debuff enemies, special unit powers, etc.)
                           //Race raceType //when we add races, we can assign race type here aswell


    //rangedMode/attackMode:
    //false - melee
    //true - ranged


    public Unit() : base()
    {
        //changes while taking damage
        health = 0;
        currentHealth = 0;
        count = 0;
        currentCount = 0;
        //go through effects to change this
        armour = 0; //armour is percentage based: from 0 to 100, it says reduction.
        currentArmour = 0;
        initiative = 0; //percentage based - from 1 to 100
        currentInitiative = 0;
        movement = 0;
        currentMovement = 0;
        //this doesnt changes
        size = 0;
        rangedMode = false;
        model = new GameObject();
    }
    //set
    //static values
    public void setHealth(int newHealth) { health = newHealth; }
    public void setArmour(int newArmour) { armour = newArmour; }
    public void setCount(int newCount) { count = newCount; }
    public void setInitiate(int newInitiative) { initiative = newInitiative; }
    public void setAttackMode(bool newRangedMode) { rangedMode = newRangedMode; }
    public void setMovement(int newMovement) { movement = newMovement; }
    public void setSize(int newSize) { size = newSize; }
    //current values
    public void setCurrentHealth(int newCurrentHealth) { currentHealth = newCurrentHealth; }
    public void setCurrentArmour(int newCurrentArmour) { currentArmour = newCurrentArmour; }
    public void setCurrentCount(int newCurrentCount) { currentCount = newCurrentCount; }
    public void setCurrentInitiative(int newCurrentInitiative) { currentInitiative = newCurrentInitiative; }
    public void setCurrentMovement(int newCurrentMovement) { currentMovement = newCurrentMovement; }
    //models & images
    public void setModel(GameObject newModel) { model = newModel; }
    public void setSprite(Sprite newModelImage) { modelImage = newModelImage; }
    //get
    //static values
    public int getHealth() { return health; }
    public int getArmour() { return armour; }
    public int getCount() { return count; }
    public int getInitiate() { return initiative; }
    public bool getAttackMode() { return rangedMode; }
    public int getMovement() { return movement; }
    public int getSize() { return size; }
    //current values
    public int getCurrentHealth() { return currentHealth; }
    public int getcurrentArmour() { return currentArmour; }
    public int getCurrentCount() { return currentCount; }
    public int getCurrentInitiate() { return currentInitiative; }
    public int getCurrentMovement() { return currentMovement; }
    //models & images
    public GameObject getModel() { return model; }
    public Sprite getSprite() { return modelImage; }
    //taking damage
    public void takeDamage(int damage)
    {
        int damageCount = damage;
        while (damageCount > 0)
        {
            if (damageCount >= currentHealth)
            {
                damageCount = damageCount - currentHealth;
                currentHealth = health;
                currentCount--;
            }
        }
    }
}
//Melee - Subclass of Unit
abstract public class Melee : Unit
{
    private int attackRange;
    public Melee() : base()
    {
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
}
//Warrior - Subclass of Melee
public class Warrior : Melee
{
    public Warrior() : base()
    {
        //basic stats
        setHealth(6);
        setArmour(35);
        setInitiate(10);
        setMovement(4);
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
        //basic stats
        setHealth(4);
        setArmour(15);
        setInitiate(90);
        setMovement(7);
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
        //basic stats
        setHealth(10);
        setArmour(20);
        setInitiate(20);
        setMovement(3);
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
        //basic stats
        setHealth(6);
        setArmour(20);
        setInitiate(50);
        setMovement(4);
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
        //basic stats
        setHealth(5);
        setArmour(15);
        setInitiate(40);
        setMovement(2);
        //ranged exclusive
        setAttackMode(true);
        setSize(1);
        setFullAttackRange(8);
    }
}

//for each unit TYPE we store here model of their upgrade: For example, Warrior has upgrade 1,2,3; we store those here. Archer is stored in another simular list, saved inside common array.
public class UnitUpgradeModels
{
    //by default we want maybe 1 possible upgrade per unit - so normal Unit and upgraded unit
    private List<GameObject> arrayBasedOnUpgrade;
    private string unitType;
    public UnitUpgradeModels() : base()
    {
        unitType = "";
        arrayBasedOnUpgrade = new List<GameObject>();
    }
    //set
    public void setNewUpgradeModel(GameObject newModel) { arrayBasedOnUpgrade.Add(newModel); }
    public void setUnitType(string newUnitType) { unitType = newUnitType; }
    //get
    public GameObject getUpgradeModel(int upgradeLevel) { return arrayBasedOnUpgrade[upgradeLevel]; }
    public string getUnitType() { return unitType; }
}
public class UnitTypeModels
{
    private const int numberOfUnitTypes = 5; //this can be constant
    private string raceType;
    //default we can set size here - each race will have the SAME ammount of unit types - for current game development we can set it to 5(Warrior,Scout,Mage,Chanter,Archer).
    private UnitUpgradeModels[] arrayBasedOnType;
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

public class UnitRacesArray
{
    public UnitTypeModels[] arrayBasedOnRase;
    public UnitRacesArray() : base()
    {
        //by default, we set races here: Human, Elf, Dwarf, Orc.
        arrayBasedOnRase = new UnitTypeModels[4];
        arrayBasedOnRase[0].setRaceType("Human");
        arrayBasedOnRase[1].setRaceType("Elf");
        arrayBasedOnRase[2].setRaceType("Dwarf");
        arrayBasedOnRase[3].setRaceType("Orc");
    }
}