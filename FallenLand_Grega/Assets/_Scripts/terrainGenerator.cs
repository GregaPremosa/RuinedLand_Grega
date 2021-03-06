﻿using UnityEngine;
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

    //this is connection to GameLogic class
    public GameLogic gamelogic;

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
        Archer testArcher_2 = new Archer();
        Warrior testWarrior_1 = new Warrior();
        Scout testScout_1 = new Scout();
        Chanter testChanter_1 = new Chanter();
        testArcher_1.setModel(arrayModels); //set model
        testArcher_1.setCount(10); //set some temporary count
        testArcher_2.setModel(arrayModels);
        testArcher_2.setCount(10);
        testWarrior_1.setModel(arrayModels);  //set model
        testWarrior_1.setCount(10); //set some temporary count
        testScout_1.setModel(arrayModels);  //set model
        testScout_1.setCount(10); //set some temporary count
        testChanter_1.setModel(arrayModels);
        testChanter_1.setCount(1);
        player1.addUnit(testArcher_1);
        testArcher_1.setPlayerOwner(player1);
        player1.addUnit(testScout_1);
        testScout_1.setPlayerOwner(player1);
        player2.addUnit(testWarrior_1);
        testWarrior_1.setPlayerOwner(player2);
        player2.addUnit(testArcher_2);
        testArcher_2.setPlayerOwner(player2);
        player1.addUnit(testChanter_1);
        testChanter_1.setPlayerOwner(player1);
        gamelogic.setTerrainGenerator(newBattleMode);
        gamelogic.setPlayer1(player1);
        gamelogic.setPlayer2(player2);
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