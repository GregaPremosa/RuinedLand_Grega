using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class terrainGenerator : MonoBehaviour
{
    public GameObject[] prefabArray; //this has corresponding elements as state
    public GameObject[] obstacleObjectArray;
    public Player player1;
    public Player player2;

    void Start()
    {
        terrain newBattleMode = new terrain();
        player1 = new Player();
        player2 = new Player();
        newBattleMode.setPrefabArray(prefabArray);
        newBattleMode.generateBlocks();
        newBattleMode.setPlayers(player1,player2); //we set players
        newBattleMode.setOccupyingEntities(obstacleObjectArray);
    }
}

public class terrain{
    //we import this from other classes or from public objects (players, prefabs)
    private GameObject[] prefabs;
    private Player player1;
    private Player player2;

    //inner variables specific to this (battle)mode
    private obstacleArray[] obstaclePositions;
    private terrainBlock[][] matrixField; //this is double array, representing and holding values of each field zone.
    private int numOfRows = 9;
    private int numOfColumns = 13;

    public terrain() : base()
    {
        //set prefabs array

        //create double array thats the size of terrain
        matrixField = new terrainBlock[numOfRows][];
        for (int index = 0; index < numOfRows; index++)
        {
            matrixField[index] = new terrainBlock[numOfColumns];
        }
        //fill each zone of area with object terrainBlock
        for (int i = 0; i < numOfRows; i++)
        {
            for (int j = 0; j < numOfColumns; j++)
            {
                matrixField[i][j] = new terrainBlock();
                matrixField[i][j].setPosition(i,j);
            }
        }
        //set spawn positions
        //first set spawn for left side
        for (int i = 1; i < numOfRows; i+=2)
        {
            matrixField[i][1].setState(1);
        }
        //now set spawn position for right side
        for (int i = 1; i < numOfRows; i += 2)
        {
            matrixField[i][11].setState(1);
        }

        //set obstacle locations - we do this staticly, since dynamicly it would take alot of time and resource but we would not profit gameplay based. Furthermore, the obstacles itself will be randomed from these static positions, making field look dynamic.
        fillObstacleArray();
        //now choose obstacle locations and adjust their state on field.
        for (int i = 0; i < Random.Range(3,8); i++) //this is random range between 3 and 7
        {
            obstacleArray newObstacle = obstaclePositions[Random.Range(0,obstaclePositions.Length)];
            while (matrixField[newObstacle.getRow()][newObstacle.getColumn()].getState() != 0)
            {
                newObstacle = obstaclePositions[Random.Range(0, obstaclePositions.Length)];
            }
            matrixField[newObstacle.getRow()][newObstacle.getColumn()].setState(2);
        }
    }

    //we staticly set obstacle positions here.
    private void fillObstacleArray()
    {
        obstaclePositions = new obstacleArray[17];
        for (int i = 0; i < obstaclePositions.Length; i++)
        {
            obstaclePositions[i] = new obstacleArray();
        }
        obstaclePositions[0].setPosition(0, 6);
        obstaclePositions[1].setPosition(1, 4);
        obstaclePositions[2].setPosition(1, 8);
        obstaclePositions[3].setPosition(2, 4);
        obstaclePositions[4].setPosition(2, 5);
        obstaclePositions[5].setPosition(2, 8);
        obstaclePositions[6].setPosition(3, 6);
        obstaclePositions[7].setPosition(4, 4);
        obstaclePositions[8].setPosition(4, 8);
        obstaclePositions[9].setPosition(5, 5);
        obstaclePositions[10].setPosition(5, 8);
        obstaclePositions[11].setPosition(6, 5);
        obstaclePositions[12].setPosition(7, 4);
        obstaclePositions[13].setPosition(7, 5);
        obstaclePositions[14].setPosition(7, 7);
        obstaclePositions[15].setPosition(7, 8);
        obstaclePositions[16].setPosition(8, 4);
    }

    //set which blocks we will generate
    public void setPrefabArray(GameObject[] newPrefabs)
    {
        prefabs = newPrefabs;
    }

    //set player1 and player2
    public void setPlayers(Player newPlayer1, Player newPlayer2)
    {
        //connect players
        player1 = newPlayer1;
        player2 = newPlayer2;
        //set spawn locations
        for (int i = 1; i < numOfRows; i += 2)
        {
            player1.addUnitSpawn(matrixField[i][1]);
            player2.addUnitSpawn(matrixField[i][11]);
        }
    }

    //add generation of blocks
    public void generateBlocks()
    {
        for (int i = 0; i < numOfRows; i++)
        {
            for (int j = 0; j < numOfColumns; j++)
            {
                matrixField[i][j].setBlockGameObject(prefabs);
                matrixField[i][j].getBlock().transform.position = new Vector3(i,0,j);
            }
        }
    }

    //set occupying gameObjects on field
    //we can set it for: obstacles(from occupyingObjects array), units for player1 (from class player player1), units for player2 (from class player player2)
    public void setOccupyingEntities(GameObject[] occupyingObjects)
    {
        for (int i = 0; i < numOfRows; i++)
        {
            for (int j = 0; j < numOfColumns; j++)
            {
                //first set obstacles
                if (matrixField[i][j].getState() == 2)
                {
                    matrixField[i][j].setOccupyingEntity(occupyingObjects[Random.Range(0, occupyingObjects.Length)]);
                }
            }
        }
    }

    //get
    public terrainBlock[][] getMatrixField() { return matrixField; }

}

//represents each class that is stored inside matrixField
public class terrainBlock
{
    private GameObject block;
    private GameObject occupyingEntity; //here we store either a Unit that is on current block or obstacle incase the state of terrainBlock is 1
    private int rowNumber = 0;
    private int ColumnNumber = 0;
    private int state;
    /*
     Explanation of states:
     0 - normal walkable area, default area
     1 - spawn position
     2 - obstacle
     this corresponds equally as we add gameobjects into prefabArray - look at class terrainGenerator:MonoBehaviour. If we decide to add more types, the system auto updates itself, aslong as we keep corresponding state numbers with arrayOfPrefabs.
    */
    public terrainBlock() : base() { block = null; occupyingEntity = null; state = 0; }

    //set
    public void setState(int newState) { state = newState; }
    public void setPosition(int newRow, int newColumn)
    {
        rowNumber = newRow;
        ColumnNumber = newColumn;
    }

    //we set ground object type here
    public void setBlockGameObject(GameObject[] prefabs)
    {
        GameObject tmp = GameObject.Instantiate(prefabs[getState()]) as GameObject;
        block = tmp;
    }
    //we set occupying entity object here
    public void setOccupyingEntity(GameObject newOccupyingEntity)
    {
        GameObject tmp = GameObject.Instantiate( newOccupyingEntity ) as GameObject;
        tmp.transform.position = block.transform.position;
        occupyingEntity = tmp;
    }
    //get
    public int getState() { return state; }
    public int getRow() { return rowNumber; }
    public int getColumn() { return ColumnNumber; }
    public GameObject getBlock() { return block; }
    public GameObject getOccupyingEntity() { return occupyingEntity; }
}

//a structure that makes assigning obstacle positions easier and faster.
public class obstacleArray
{
    private int rowNumber;
    private int columnNumber;

    public obstacleArray() : base()
    {
        rowNumber = 0;
        columnNumber = 0;
    }

    //set
    public void setPosition(int newRow,int newColumn)
    {
        rowNumber = newRow;
        columnNumber = newColumn;
    }
    //get
    public int getRow() { return rowNumber; }
    public int getColumn() { return columnNumber; }
}

    //PLAYER and UNITS
/*
//THIS IS TO BE UNIFIED - change after copying
public class Player
{
    private List<Unit> arrayUnits;
    private List<terrainBlock> spawnPositions;

    public Player() : base()
    {
        arrayUnits = new List<Unit>();
        spawnPositions = new List<terrainBlock>();
    }

    //set
    public void addUnitSpawn(terrainBlock newSpawnPos)
    {
        spawnPositions.Add(newSpawnPos);
    }
}
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
    //List<Effects> passives; //we can later add these and they will take effects in battle(boost certain unit types, debuff enemies, special unit powers, etc.)
    //Race raceType //when we add races, we can assign race type here aswell

    
    //rangedMode/attackMode:
    //false - melee
    //true - ranged
     

    public Unit() : base()
    {
        health = 0;
        currentHealth = 0;
        armour = 0; //armour is percentage based: from 0 to 100, it says reduction.
        currentArmour = 0;
        count = 0;
        currentCount = 0;
        initiative = 0; //percentage based - from 1 to 100
        currentInitiative = 0;
        movement = 0;
        currentMovement = 0;
        size = 0;
        rangedMode = false;
        model = new GameObject();
        //for now, we can modify 1 current method, since Effect class is yet now implemented. Idea is to have Units normal static values and whats units currentCounter - current counter is what we calculate in battle, static values are where we gather information without effects.
    }
    //set
    public void setHealth(int newHealth) { health = newHealth; }
    public void setCurrentHealth(int damage)
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
    public void setArmour(int newArmour) { armour = newArmour; }
    public void setCount(int newCount) { count = newCount; }
    public void setInitiate(int newInitiative) { initiative = newInitiative; }
    public void setAttackMode(bool newRangedMode) { rangedMode = newRangedMode; }
    public void setMovement(int newMovement) { movement = newMovement; }
    public void setSize(int newSize) { size = newSize; }
    public void setModel(GameObject newModel) { model = newModel; }
    public void setSprite(Sprite newModelImage) { modelImage = newModelImage; }
    //get
    public int getHealth() { return health; }
    public int getArmour() { return armour; }
    public int getCount() { return count; }
    public int getInitiate() { return initiative; }
    public bool getAttackMode() { return rangedMode; }
    public int getMovement() { return movement; }
    public int getSize() { return size; }
    public GameObject getModel() { return model; }
    public Sprite getSprite() { return modelImage; }
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
    private bool fullDamageActive; //full damage is done, when unit is in fullAttackRange, otherwise damage is just a QUARTER of full damage.
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
    public void setRaceType(string newRaceType){ raceType = newRaceType; }
    public void setUnitTypeModel(UnitUpgradeModels newUnitTypeModel,int index){ arrayBasedOnType[index] = newUnitTypeModel; }
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
*/