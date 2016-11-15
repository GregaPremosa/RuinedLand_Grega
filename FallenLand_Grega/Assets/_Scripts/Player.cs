using System.Collections.Generic;

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