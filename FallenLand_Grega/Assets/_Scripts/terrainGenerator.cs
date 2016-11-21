using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class terrainGenerator : MonoBehaviour
{
    public GameObject[] prefabArray; //this has corresponding elements as state for blocks(whether it can be occupied, is a spawn or is an obstacle)
    public GameObject[] obstacleObjectArray;
    
    //set player1 and player2
    public Player player1;
    public Player player2;

    //have arrays, where we store models for units
    public UnitRacesArray arrayModels;

    //these are used in game logic
    public GameLogic gamelogic;
    public GameObject movementIndicator;

    void Start()
    {
        terrain newBattleMode = new terrain();
        player1 = new Player();
        player2 = new Player();
        newBattleMode.setPrefabArray(prefabArray);
        newBattleMode.generateBlocks();
        newBattleMode.setPlayers(player1,player2); //we set players
        newBattleMode.setOccupyingEntities(obstacleObjectArray);
        //This is testing interaction for players - TESTING FACILITY, STATIC IS ONLY TEMPORARY
        Archer testArcher_1 = new Archer();
        Warrior testWarrior_1 = new Warrior();
        Scout testScout_1 = new Scout();
        testArcher_1.setModel(arrayModels);
        testWarrior_1.setModel(arrayModels);
        testScout_1.setModel(arrayModels);
        player1.addUnit(testArcher_1);
        player1.addUnit(testScout_1);
        player2.addUnit(testWarrior_1);
        //start game logic - priority queue, unit actions and interactions
        GameLogic gameLogic = gameObject.AddComponent<GameLogic>() as GameLogic;
        //GameLogic gameLogic = gameObject.GetComponent<GameLogic>() as GameLogic;
        gameLogic.setTerrainGenerator(newBattleMode);
        gameLogic.setMovementIndicator(movementIndicator);
        gameLogic.setPlayer1(player1);
        gameLogic.setPlayer2(player2);
        gamelogic = gameLogic;
        //gamelogic.gameRuntime(player1, player2);
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
                    GameObject newObstacle = GameObject.Instantiate(occupyingObjects[Random.Range(0, occupyingObjects.Length)]) as GameObject;
                    matrixField[i][j].setOccupyingEntity( newObstacle );
                }
            }
        }
    }

    //get
    public terrainBlock[][] getMatrixField() { return matrixField; }
    public int getNumberOfRows() { return numOfRows; }
    public int getNumberOfColumns() { return numOfColumns; }
    public Player getPlayer1() { return player1; }
    public Player getPlayer2() { return player2; }

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
        //GameObject tmp = GameObject.Instantiate( newOccupyingEntity ) as GameObject;
        //tmp.transform.position = block.transform.position;
        //occupyingEntity = tmp;
        if (newOccupyingEntity != null)
        {
            newOccupyingEntity.transform.position = block.transform.position;
        }
        occupyingEntity = newOccupyingEntity;
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

public class GameLogic: MonoBehaviour
{

    //private only to this class
    private List<Unit> priorityQueue;
    //we set these 2 values from terrain Start() function.
    private terrain terrainGenerator;
    private GameObject movementIndicator;
    private List<GameObject> arrayOfMovementIndicators;

    //set this from terrain terrainGenerator
    private Player player1;
    private Player player2;

    public GameLogic() : base()
    {
        priorityQueue = null;
        movementIndicator = null;
        terrainGenerator = null;
        player1 = null;
        player2 = null;
        arrayOfMovementIndicators = new List<GameObject>();
    }

    public void setPlayer1(Player newPlayer1) { player1 = newPlayer1; }
    public void setPlayer2(Player newPlayer2) { player2 = newPlayer2; }
    public void setTerrainGenerator(terrain newTerrainGenerator) { terrainGenerator = newTerrainGenerator; }
    public void setMovementIndicator(GameObject newMovementIndicator) { movementIndicator = newMovementIndicator; }

    bool startNewRound = true;
    bool unitActions = false;
    bool changeToNextUnit = false;
    //we manage how game interacts with user by having a boolean and a Coroutine
    void Update()
    {
        if (player1 != null && player2 != null)
        {
            if (startNewRound)
            {
                StartCoroutine(roundSet());
                /*for (int i = 0; i < terrainGenerator.getNumberOfRows(); i++)
                {
                    for (int j = 0; j < terrainGenerator.getNumberOfColumns(); j++)
                    {
                        Debug.Log("terrain of [" + i + "][" + j + "] is " + terrainGenerator.getMatrixField()[i][j].getOccupyingEntity() );
                    }
                }*/
            }
            if (!startNewRound && unitActions)
            {
                StartCoroutine(checkUnitactions());
            }
        }
    }

    public IEnumerator roundSet()
    {
        startNewRound = false;
        //instanciate new priorityQueue
        priorityQueue = new List<Unit>();
        //first go through player 1 units and put them in priorityQueue
        for (int index = 0; index < player1.getArrayUnits().Count; index++)
        {
            priorityQueue.Add(player1.getArrayUnits()[index]);
        }
        //now go into player 2 units and put them in priorityQueue
        for (int index = 0; index < player2.getArrayUnits().Count; index++)
        {
            priorityQueue.Add(player2.getArrayUnits()[index]);
        }
        //for each unit set current stats now and check their effects, passives, etc
        for (int index = 0; index < priorityQueue.Count; index++)
        {
            priorityQueue[index].prepareUnitStats();
            priorityQueue[index].applyEffects();
        }
        //sort Units in order of currentPriority first, then by LOWER currentAttack, then by HIGHER currentDefense, then whichever is first in queue is sooner
        //we have 8 elements max (or maybe something like 20 if we expand the game so more Units can be placed - literaly any sorting algorithm works here)
        //We can just go with BubbleSort - very fast and easy implementation
        for (int i = 0; i < priorityQueue.Count; i++)
        {
            for (int j = 0; j < (priorityQueue.Count - 1); j++)
            {
                //firstly sort by currentInitiative - higher to left
                if (priorityQueue[j].getCurrentInitiate() < priorityQueue[j + 1].getCurrentInitiate())
                {
                    Unit tmp;
                    tmp = priorityQueue[j];
                    priorityQueue[j] = priorityQueue[j + 1];
                    priorityQueue[j + 1] = tmp;
                }
                else if (priorityQueue[j].getCurrentInitiate() == priorityQueue[j + 1].getCurrentInitiate())
                {
                    //secondly sort by currentAttack - lower to left
                    if (priorityQueue[j].getCurrentAttack() > priorityQueue[j + 1].getCurrentAttack())
                    {
                        Unit tmp;
                        tmp = priorityQueue[j];
                        priorityQueue[j] = priorityQueue[j + 1];
                        priorityQueue[j + 1] = tmp;
                    }
                    else if (priorityQueue[j].getCurrentAttack() == priorityQueue[j + 1].getCurrentAttack())
                    {
                        //lastly sort by currentArmour - higher to left
                        if (priorityQueue[j].getCurrentArmour() < priorityQueue[j + 1].getCurrentArmour())
                        {
                            Unit tmp;
                            tmp = priorityQueue[j];
                            priorityQueue[j] = priorityQueue[j + 1];
                            priorityQueue[j + 1] = tmp;
                        }
                        else { } //otherwise, we dont really care which is before, since these 3 stats are the most important
                    }
                }
            }
        }
        unitActions = true;
        yield break;
    }

    //Unit movement,attack,defense,actions are set here - priorityQueue, stats, effects are already active.
    public IEnumerator checkUnitactions()
    {
        if (priorityQueue.Count > 0)
        {
            //here we set unit actions - if condition for each action
            //for each Unit show how far a unit can move and limit him to that range
            generateMovementArea(terrainGenerator);
            //for use of testing we will just check mouse click 1
            if (Input.GetMouseButtonDown(0))
            {
                //with raycast, we can check which gameObject we hit and make further actions based on this.
                Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit ,100f))
                {
                    //if hit gameobject transforms name is equal to movement indicators name (cant compare gameobject itself, since every movement indicator gameobject is unique), then change position to this clicked movementIndicator and count that as action
                    if ( hit.transform.gameObject.name == movementIndicator.transform.GetChild(0).gameObject.name )
                    {
                        //change Unit to new position
                        //find clicked position on matrix - find gameobject of clicked indicator
                        int newRow = 0;
                        int newCol = 0;
                        for (int i = 0; i < terrainGenerator.getNumberOfRows(); i++)
                        {
                            for (int j = 0; j < terrainGenerator.getNumberOfColumns(); j++)
                            {
                                //check if occupying gameobject isnt null - that it exists
                                if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity() != null)
                                {
                                    //check if occupying gameobject has a child
                                    if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity().transform.childCount > 0)
                                    {
                                        //check if occupying gameobjects child is gameobject of raycast hit gameobject
                                        if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity().transform.GetChild(0).gameObject == hit.transform.gameObject)
                                        {
                                            newRow = i;
                                            newCol = j;
                                            Debug.Log("Found the movement indicator!");
                                        }
                                    }
                                }
                            }
                        }
                        terrainGenerator.getMatrixField()[priorityQueue[0].getRowPos()][priorityQueue[0].getColPos()].setOccupyingEntity(null);
                        terrainGenerator.getMatrixField()[newRow][newCol].setOccupyingEntity(priorityQueue[0].getModel());
                        priorityQueue[0].setColPos(newCol);
                        priorityQueue[0].setRowPos(newRow);
                        changeToNextUnit = true;
                    }
                }
                //check which Unit was is on bottom index
                Debug.Log("Unit: " + priorityQueue[0].getModel());
                if (changeToNextUnit == true)
                {
                    priorityQueue.RemoveAt(0);
                    deleteMovementArea();
                    changeToNextUnit = false;
                }
            }
        }
        if (player1.getAlive() && player2.getAlive() && priorityQueue.Count == 0)
        {
            unitActions = false;
            startNewRound = true;
        }
        yield break;
    }

    public void generateMovementArea( terrain BM_terrain)
    {
        int movementSize = priorityQueue[0].getCurrentMovement();
        int Rowpos = priorityQueue[0].getRowPos();
        int Colpos = priorityQueue[0].getColPos();
        //Debug.Log("row of unit: " + Rowpos);
        //Debug.Log("column position of unit: " + Colpos);
        //just in case check if everything is okay
        if ((Colpos == -1) && (Rowpos == -1)){ Debug.LogError("GameLogic - Element Unit ni v polju, je pa v priorityQeueue...generirannje movementArea fiiled...ERROR"); }
        //generate area from Left to Right - you have borders set in variables bellow
        //check borders for spawning
        //mostLeft and mostRight
        int mostLeft = Colpos - movementSize;
        if (mostLeft < 0) { mostLeft = 0; }
        int mostRight = Colpos + movementSize;
        if (mostRight >= BM_terrain.getNumberOfColumns()) { mostRight = BM_terrain.getNumberOfColumns() - 1; }
        //mostTop and mostBot (mostTop/mostBot are inverse to logic of mostLeft/Right: vertical is lowest index at mostTop, while horizontal is at mostLeft )
        int mostTop = Rowpos - movementSize;
        if (mostTop < 0) { mostTop = 0; }
        int mostBot = Rowpos + movementSize;
        if (mostBot >= BM_terrain.getNumberOfRows()) { mostBot = BM_terrain.getNumberOfRows() - 1; }
        //first generate Left-Right
        for (int index_col = mostLeft; index_col <= mostRight; index_col++)
        {
            int index_row_topLimit = Rowpos - (movementSize - Mathf.Abs(index_col - Colpos));
            if (index_row_topLimit < mostTop) { index_row_topLimit = mostTop; }
            int index_row_botLimit = Rowpos + (movementSize - Mathf.Abs(index_col - Colpos));
            if (index_row_botLimit > mostBot) { index_row_botLimit = mostBot; }
            for (int index_row = index_row_topLimit; index_row <= index_row_botLimit; index_row++)
            {
                if (BM_terrain.getMatrixField()[index_row][index_col].getOccupyingEntity() == null)
                {
                    GameObject newIndicator = GameObject.Instantiate(movementIndicator) as GameObject;
                    arrayOfMovementIndicators.Add(newIndicator);
                    BM_terrain.getMatrixField()[index_row][index_col].setOccupyingEntity(newIndicator);
                }
            }
        }
    }
    //after unit is deselected, release all movement areas he was to taking
    public void deleteMovementArea()
    {
        for (int index = 0; index < arrayOfMovementIndicators.Count; index++)
        {
            Destroy( arrayOfMovementIndicators[index].gameObject );
        }
        arrayOfMovementIndicators.Clear();
    }

}