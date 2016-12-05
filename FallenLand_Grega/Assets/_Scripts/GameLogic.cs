using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    //add in editor
    public GameObject movementIndicator;
    public GameObject attackIndicator;
    public GameObject previewPositionIndicator;
    public GameObject currentUnitIndicator;

    //private only to this class
    private List<Unit> priorityQueue;
    //we set this values from terrain Start() function.
    private terrain terrainGenerator;
    private List<GameObject> arrayOfIndicators;

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
        arrayOfIndicators = new List<GameObject>();
    }

    public void setPlayer1(Player newPlayer1) { player1 = newPlayer1; }
    public void setPlayer2(Player newPlayer2) { player2 = newPlayer2; }
    public void setTerrainGenerator(terrain newTerrainGenerator) { terrainGenerator = newTerrainGenerator; }

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
            //check Unit passives - TEMPORARY CHECK
            for (int passiveCounter = 0; passiveCounter < priorityQueue[0].getPassives().Count; passiveCounter++)
            {
                Debug.Log("Unit has passive effect: " + priorityQueue[0].getPassives()[passiveCounter]);
            }

            //here we set unit actions - if condition for each action
            //for each Unit show how far a unit can move and limit him to that range
            generateMovementArea(terrainGenerator);
            //for use of testing we will just check mouse click 1
            if (Input.GetMouseButtonDown(0))
            {
                //with raycast, we can check which gameObject we hit and make further actions based on this.
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100f))
                {
                    //if hit gameobject transforms name is equal to movement indicators name (cant compare gameobject itself, since every movement indicator gameobject is unique), then change position to this clicked movementIndicator and count that as action
                    if (hit.transform.gameObject.name == movementIndicator.transform.GetChild(0).gameObject.name)
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
                                            //Debug.Log("Found the movement indicator!");
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
                    //if hit gameobject is a gameObject of Unit, check if its Unit of another player - if it is, deal melee damage to it (careful: beware of condition that you actualy need to be in range)
                    if (hit.transform.gameObject.tag == "Unit") //on every Unit GameModel(be careful: add Tag to actual model, NOT modelHolder!) we add a 'Unit' tag - so we can check if hit gameobject is unit or not
                    {
                        //Get Unit class from clicked gameobject
                        //Debug.Log("You clicked on a Unit");
                        Unit clickedUnit = getUnitClassFromGameobject(hit.transform.gameObject);
                        //with if we check if currently selected Unit and clicked Unit are under the same player
                        if (determinePlayerOfUnit(clickedUnit.getModel()) != determinePlayerOfUnit(priorityQueue[0].getModel()))
                        {
                            Debug.Log("Current Unit and clicked Unit are not from same player");
                            //check if Unit has an Indicator
                            if (clickedUnit.getBattleIndicator() != null)
                            {
                                //check if this indicator is battle indicator
                                if (clickedUnit.getBattleIndicator() == attackIndicator)
                                {
                                    //Debug.Log("Selected Unit: " + priorityQueue[0]);
                                    //Debug.Log("Enemy Unit: " + clickedUnit);
                                    //Debug.Log("Unit can be attacked in melee!");
                                    Debug.Log("Current Unit count: " + priorityQueue[0].getCurrentCount() + ", health: " + priorityQueue[0].getCurrentHealth() + ",attack: " + priorityQueue[0].getCurrentAttack());
                                    Debug.Log("Clicked Unit count: " + clickedUnit.getCurrentCount() + ", health: " + clickedUnit.getCurrentHealth() + ", attack: " + clickedUnit.getCurrentAttack());
                                    priorityQueue[0].dealDamage(clickedUnit);
                                    Debug.Log("Clicked Unit count: " + clickedUnit.getCurrentCount() + ", health: " + clickedUnit.getCurrentHealth() + ", attack: " + clickedUnit.getCurrentAttack());
                                }
                            }
// UNDER CONSTRUCTION - ranged attack - check if selected Unit is ranged, check which damage to deal(quarter or full)
                        }
                    }
                }
                //check which Unit was on bottom index (if he did action) or which Unit is on bottom index (if player did not do any action)
                //Debug.Log("Unit: " + priorityQueue[0].getModel());
            }
        }
        //If we implement UI buttons via button functions, we can set this block of code - this if - outside of button click
        if (changeToNextUnit == true)
        {
            priorityQueue.RemoveAt(0);
            deleteMovementArea();
            removeUnitIndicators();
            changeToNextUnit = false;
        }
        if (player1.getAlive() && player2.getAlive() && priorityQueue.Count == 0)
        {
            unitActions = false;
            startNewRound = true;
        }
        yield break;
    }

    public void generateMovementArea(terrain BM_terrain)
    {
        int movementSize = priorityQueue[0].getCurrentMovement();
        int Rowpos = priorityQueue[0].getRowPos();
        int Colpos = priorityQueue[0].getColPos();
        //Debug.Log("row of unit: " + Rowpos);
        //Debug.Log("column position of unit: " + Colpos);
        //just in case check if everything is okay
        if ((Colpos == -1) && (Rowpos == -1)) { Debug.LogError("GameLogic - Element Unit is not in array, but is in priorityQueue...generating movementArea failed...ERROR"); }
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
                //spawn movementPosition, where Unit can move
                if (BM_terrain.getMatrixField()[index_row][index_col].getOccupyingEntity() == null)
                {
                    GameObject newIndicator = GameObject.Instantiate(movementIndicator) as GameObject;
                    arrayOfIndicators.Add(newIndicator);
                    BM_terrain.getMatrixField()[index_row][index_col].setOccupyingEntity(newIndicator);
                }
                //indicate, that enemy can be attacked - via some red aura or indicator
                else
                {
                    //attackIndicator & selfUnit indicator spawning -->
                    //check if this Gameobjects transform has children under it
                    if (BM_terrain.getMatrixField()[index_row][index_col].getOccupyingEntity().transform.childCount > 0)
                    {
                        //if we detect a Unit as first child
                        if (BM_terrain.getMatrixField()[index_row][index_col].getOccupyingEntity().transform.GetChild(0).tag == "Unit")
                        {
                            //save this unit into gameobject reff - clearer code
                            GameObject unit = BM_terrain.getMatrixField()[index_row][index_col].getOccupyingEntity();
                            //determine which players is it
                            Player playerOfUnit = determinePlayerOfUnit(unit);
                            //go into that player and gt its Unit class
                            for (int unitIndex = 0; unitIndex < playerOfUnit.getArrayUnits().Count; unitIndex++)
                            {
                                if (playerOfUnit.getArrayUnits()[unitIndex].getModel() == unit)
                                {
                                    //now check if this Unit already has indicator of any sort - here we will set its indicators and spawn them
                                    if (playerOfUnit.getArrayUnits()[unitIndex].getBattleIndicator() == null)
                                    {
                                        //if movement area spawner detects currently selected Unit as occupying entity
                                        if (playerOfUnit.getArrayUnits()[unitIndex].getModel() == priorityQueue[0].getModel())
                                        {
                                            //Debug.Log("We are targeting selected Unit");
                                            GameObject indicator_self = GameObject.Instantiate(currentUnitIndicator) as GameObject;
                                            indicator_self.transform.position = new Vector3(unit.transform.position.x, currentUnitIndicator.transform.position.y, unit.transform.position.z);
                                            playerOfUnit.getArrayUnits()[unitIndex].setBattleIndicator(indicator_self);
                                            arrayOfIndicators.Add(indicator_self);
                                        }
                                        //if movement area spawner detects another non-selected Unit as occupying entity
                                        else
                                        {
                                            if (playerOfUnit != determinePlayerOfUnit(priorityQueue[0].getModel()))
                                            {
                                                //Debug.Log("Enemy Unit detected");
                                                GameObject indicator_attack = GameObject.Instantiate(attackIndicator) as GameObject;
                                                indicator_attack.transform.position = new Vector3(unit.transform.position.x,attackIndicator.transform.position.y,unit.transform.position.z);
                                                playerOfUnit.getArrayUnits()[unitIndex].setBattleIndicator(attackIndicator);
                                                arrayOfIndicators.Add(indicator_attack);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //get Unit class from GameObject we parse as parameter - if that is possible
    public Unit getUnitClassFromGameobject(GameObject selectedUnitGameObject)
    {
        for (int i = 0; i < player1.getArrayUnits().Count; i++)
        {
            if (player1.getArrayUnits()[i].getModel().transform.childCount > 0) //first check if Unit is maybe as child in transform of selectedUnitGameobject
            {
                for (int childCounter = 0; childCounter < player1.getArrayUnits()[i].getModel().transform.childCount; childCounter++)
                {
                    if (player1.getArrayUnits()[i].getModel().transform.GetChild(childCounter).gameObject == selectedUnitGameObject)
                    {
                        //Debug.Log("Unit is in player1 array");
                        return player1.getArrayUnits()[i];
                    }
                }
            }
        }
        for (int i = 0; i < player2.getArrayUnits().Count; i++)
        {
            for (int childCounter = 0; childCounter < player2.getArrayUnits()[i].getModel().transform.childCount; childCounter++)
            {
                if (player2.getArrayUnits()[i].getModel().transform.GetChild(childCounter).gameObject == selectedUnitGameObject)
                {
                    //Debug.Log("Unit is in player2 array");
                    return player2.getArrayUnits()[i];
                }
            }
        }
        Debug.Log("Unit is in neither of player's arrays");
        return null;
    }

    //remove all Unit indicators - self target, attack indicator & ground indicators
    public void removeUnitIndicators()
    {
        for (int indexUnit = 0; indexUnit < player1.getArrayUnits().Count; indexUnit++)
        {
            player1.getArrayUnits()[indexUnit].setBattleIndicator(null);
        }
        for (int indexUnit = 0; indexUnit < player2.getArrayUnits().Count; indexUnit++)
        {
            player2.getArrayUnits()[indexUnit].setBattleIndicator(null);
        }
    }

    //determine to which Player does Unit belong
    public Player determinePlayerOfUnit(GameObject checkedUnit)
    {
        //first check in player1's array
        for (int i = 0; i < player1.getArrayUnits().Count; i++)
        {
            if (player1.getArrayUnits()[i].getModel() == checkedUnit)
            {
                return player1;
            }
        }
        //now check in player2's array
        for (int i = 0; i < player2.getArrayUnits().Count; i++)
        {
            if (player2.getArrayUnits()[i].getModel() == checkedUnit)
            {
                return player2;
            }
        }
        //Unit isnt in either's player array
        Debug.LogError("Unit is in neither players array");
        return null;
    }

    //after unit is deselected, release all movement areas he was to taking
    public void deleteMovementArea()
    {
        for (int index = 0; index < arrayOfIndicators.Count; index++)
        {
            Destroy(arrayOfIndicators[index].gameObject);
        }
        arrayOfIndicators.Clear();
    }

    //UI CLICK FUNCTIONS - these can and should be separate from clicks we make on the game Board itself since this is UI and Board is game itself (2 separate elements of the game as a whole)
    //OnMouseClick for button-Defend. Raise Unit defense for 20% - as effect
    public void onButtonClickDefendYourself()
    {
        DefendYourself newEffect = new DefendYourself();
        priorityQueue[0].addEffect(newEffect);
        changeToNextUnit = true;
    }

}
