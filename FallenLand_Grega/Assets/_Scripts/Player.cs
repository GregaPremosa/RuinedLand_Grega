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
    public void addUnit(Unit unit)
    {
        arrayUnits.Add(unit);
        int index = arrayUnits.IndexOf(unit);
        spawnPositions[index].setOccupyingEntity( unit.getModel() );
    }
    //get whole list of spawnPositions
    public List<terrainBlock> getSpawnPosition() { return spawnPositions; }
    public List<Unit> getArrayUnits() { return arrayUnits; }
}