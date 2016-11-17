using UnityEngine;
using System.Collections.Generic;

public class Player
{
    private List<Unit> arrayUnits;
    private List<terrainBlock> spawnPositions;
    private Race race;
    private bool alive;

    public Player() : base()
    {
        alive = true;
        arrayUnits = new List<Unit>();
        spawnPositions = new List<terrainBlock>();
        race = new Race();
    }

    //set
    public void addUnitSpawn(terrainBlock newSpawnPos)
    {
        spawnPositions.Add(newSpawnPos);
    }
    public void addUnit(Unit unit)
    {
        arrayUnits.Add(unit);
        int index = arrayUnits.IndexOf(unit);
        unit.setColPos( spawnPositions[index].getColumn() );
        unit.setRowPos( spawnPositions[index].getRow() );
        spawnPositions[index].setOccupyingEntity( unit.getModel() );
    }
    //get whole list of spawnPositions
    public List<terrainBlock> getSpawnPosition() { return spawnPositions; }
    public List<Unit> getArrayUnits() { return arrayUnits; }
    public bool getAlive() { return alive; }
    //check array of Units; if its empty, change 'alive' to false
    public void checkAliveStatus()
    {
        Debug.Log("player has " + getArrayUnits().Count + " units.");
        if (getArrayUnits().Count == 0)
        {
            Debug.Log("player has died.");
            alive = false;
        }
    }
    //when we have a working priority queue we can delete set method - it is just so we can manually test alive state(it works auto with function above)
    public void setAlive(bool newalive) { alive = newalive; }
}

