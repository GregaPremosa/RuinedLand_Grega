using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    //action mode:
    /*
   actionMode = 0 //means movement,defend and decide if you want to attack
   actionMode = 1 //attack has been selected, move your Unit to new indicator and declare by doing so, you complete action ->return to 0 after
         */
    private int actionMode;
    private bool generateNewMovArea;
    private Unit targetUnit;

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
        actionMode = 0;
        generateNewMovArea = true;
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
            if (actionMode == 0)
            {
                //check Unit passives - TEMPORARY CHECK
                //for (int passiveCounter = 0; passiveCounter < priorityQueue[0].getPassives().Count; passiveCounter++)
                //{
                //    Debug.Log("Unit has passive effect: " + priorityQueue[0].getPassives()[passiveCounter]);
                //}

                //here we set unit actions - if condition for each action
                //for each Unit show how far a unit can move and limit him to that range
                if (generateNewMovArea)
                {
                    generateMovementArea(terrainGenerator);
                    generateNewMovArea = false;
                }
                //for use of testing we will just check mouse click 1
                if (Input.GetMouseButtonDown(0))
                {
                    //with raycast, we can check which gameObject we hit and make further actions based on this.
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100f))
                    {
                        if (hit.transform.gameObject.name == movementIndicator.transform.GetChild(0).gameObject.name)
                        {
                            changeUnitPosition(hit);
                            changeToNextUnit = true;
                        }
                        //if hit gameobject is a gameObject of Unit, check if its Unit of another player - if it is, deal melee damage to it (careful: beware of condition that you actualy need to be in range)
                        else if (hit.transform.gameObject.tag == "Unit") //on every Unit GameModel(be careful: add Tag to actual model, NOT modelHolder!) we add a 'Unit' tag - so we can check if hit gameobject is unit or not
                        {
                            //Get Unit class from clicked gameobject
                            //Debug.Log("You clicked on a Unit");
                            Unit clickedUnit = getUnitClassFromGameobject(hit.transform.gameObject);
                            //with if we check if currently selected Unit and clicked Unit are under the same player
                            if (determinePlayerOfUnit(clickedUnit.getModel()) != determinePlayerOfUnit(priorityQueue[0].getModel()))
                            {
                                Debug.Log("Current Unit and clicked Unit are not from same player");
                                //RANGED ATTACK
                                //for ranged attack, we need to check if selected Unit can even do ranged attack
                                if (priorityQueue[0].getAttackMode() == true)
                                {
                                    //check if any enemy Unit is in melee range of current Unit - if it is, we cannot do ranged attack
                                    int row_min_adjacent = priorityQueue[0].getRowPos() - 1;
                                    if (row_min_adjacent < 0) { row_min_adjacent = 0; }
                                    int row_max_adjacent = priorityQueue[0].getRowPos() + 1;
                                    if (row_max_adjacent >= terrainGenerator.getNumberOfRows()) { row_max_adjacent = terrainGenerator.getNumberOfRows() - 1; }
                                    int col_min_adjacent = priorityQueue[0].getColPos() - 1;
                                    if (col_min_adjacent < 0) { col_min_adjacent = 0; }
                                    int col_max_adjacent = priorityQueue[0].getColPos() + 1;
                                    if (col_max_adjacent >= terrainGenerator.getNumberOfColumns()) { col_max_adjacent = terrainGenerator.getNumberOfColumns() - 1; }
                                    //Debug.Log("Ranged Unit adjacent borders to check: rows" + row_min_adjacent +"-"+ row_max_adjacent +", cols: "+ col_min_adjacent +"-"+  col_max_adjacent);
                                    bool allowRangedAttack = true;
                                    for (int i = row_min_adjacent; i <= row_max_adjacent; i++)
                                    {
                                        for (int j = col_min_adjacent; j <= col_max_adjacent; j++)
                                        {
                                            if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity() != null)
                                            {
                                                if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity().transform.childCount > 0)
                                                {
                                                    if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity().transform.GetChild(0).tag == "Unit")
                                                    {
                                                        if (determinePlayerOfUnit(priorityQueue[0].getModel()) != determinePlayerOfUnit(terrainGenerator.getMatrixField()[i][j].getOccupyingEntity()))
                                                        {
                                                            Debug.Log("Around our ranged Unit is an enemy Unit - we cannot attack");
                                                            allowRangedAttack = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //if we did not find any enemy Unit adjacent to our ranged Unit, then we can do ranged attack
                                    if (allowRangedAttack)
                                    {
                                        Ranged ranged_unit = (Ranged)priorityQueue[0];
                                        //Debug.Log("Ranged Unit will attack");
                                        //Based on how far each unit is from another, we need to determine, if it should do full damage or quarter damage
                                        //set row check range
                                        int row_min = ranged_unit.getRowPos() - ranged_unit.getFullAttackRange();
                                        if (row_min < 0) { row_min = 0; }
                                        int row_max = ranged_unit.getRowPos() + ranged_unit.getFullAttackRange();
                                        if (row_max >= terrainGenerator.getNumberOfRows()) { row_max = terrainGenerator.getNumberOfRows() - 1; }
                                        //set column check range
                                        int col_min = ranged_unit.getColPos() - ranged_unit.getFullAttackRange();
                                        if (col_min < 0) { col_min = 0; }
                                        int col_max = ranged_unit.getColPos() + ranged_unit.getFullAttackRange();
                                        if (col_max >= terrainGenerator.getNumberOfColumns()) { col_max = terrainGenerator.getNumberOfColumns() - 1; }
                                        //first set it so that full damage active is false; if we find the opposite Unit in searched field, then we will set it to true and break the search
                                        ranged_unit.setFullDamageActive(false);
                                        //check in area specified in range above
                                        for (int i = row_min; i <= row_max; i++)
                                        {
                                            for (int j = col_min; j <= col_max; j++)
                                            {
                                                if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity() != null)
                                                {
                                                    if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity().transform.childCount > 0)
                                                    {
                                                        if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity().transform.GetChild(0).tag == "Unit")
                                                        {
                                                            if (terrainGenerator.getMatrixField()[i][j].getOccupyingEntity() == clickedUnit.getModel())
                                                            {
                                                                Debug.Log("Full attack range allowed");
                                                                ranged_unit.setFullDamageActive(true);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //now deal ranged damage and properly set variables for next Unit
                                        ranged_unit.dealRangedDamage(clickedUnit);
                                        removeDeadUnit(clickedUnit);
                                        changeToNextUnit = true;
                                        deleteMovementArea();
                                    }
                                }
                                //MELEE ATTACK
                                //check if Unit has an Indicator
                                if (clickedUnit.getBattleIndicator() != null && changeToNextUnit==false)
                                {
                                    //check if this indicator is battle indicator
                                    if (clickedUnit.getBattleIndicator() == attackIndicator)
                                    {
                                        //Debug.Log("Unit can be attacked in melee!");
                                        targetUnit = clickedUnit;
                                        List<GameObject> enemyAdjacentMovementIndicators = new List<GameObject>();
                                        enemyAdjacentMovementIndicators = findAdjacentMovementIndicators(clickedUnit);
                                        //Debug.Log("size of movement indicator: " + enemyAdjacentMovementIndicators.Count);
                                        //clear all OBJECT indicators, which are not meant for melee range possible movement position
                                        for (int movementIndex = 0; movementIndex < arrayOfIndicators.Count; movementIndex++)
                                        {
                                            bool delete = true;
                                            for (int adjMovementIndex = 0; adjMovementIndex < enemyAdjacentMovementIndicators.Count; adjMovementIndex++)
                                            {
                                                if (arrayOfIndicators[movementIndex].transform.position == enemyAdjacentMovementIndicators[adjMovementIndex].transform.position)
                                                {
                                                    delete = false;
                                                    break;
                                                }
                                            }
                                            if (delete == true)
                                            {
                                                Destroy(arrayOfIndicators[movementIndex].gameObject);
                                            }
                                        }
                                        //properly clear ALL indicators...
                                        arrayOfIndicators.Clear();
                                        //now save only those we preserved before and are still existing as Objects - the actual movement areas for attack
                                        for (int i = 0; i < enemyAdjacentMovementIndicators.Count; i++)
                                        {
                                            arrayOfIndicators.Add(enemyAdjacentMovementIndicators[i]);
                                        }
                                        //we wont need unit indicators anymore either, since we deleted their gameobjects
                                        removeUnitIndicators();
                                        actionMode = 1;
                                    }
                                }
                            }
                        }
                    }
                    //check which Unit was on bottom index (if he did action) or which Unit is on bottom index (if player did not do any action)
                    //Debug.Log("Unit: " + priorityQueue[0].getModel());
                }
            }
            else if (actionMode == 1)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100f))
                    {
                        if (hit.transform.gameObject.name == movementIndicator.transform.GetChild(0).gameObject.name)
                        {
                            //change Unit to new position
                            changeUnitPosition(hit);
                        }

                        //now deal damage to enemy Unit
                        priorityQueue[0].dealDamage(targetUnit);
                        removeDeadUnit(targetUnit);
                        if (targetUnit != null)
                        {
                            targetUnit.dealDamage(priorityQueue[0]);
                            removeDeadUnit(priorityQueue[0]);
                        }
                        targetUnit = null;
                        changeToNextUnit = true;
                        actionMode = 0;
                        deleteMovementArea();
                    }
                }
            }
        }
            
        //If we implement UI buttons via button functions, we can set this block of code - this if - outside of button click
        if (changeToNextUnit == true)
        {
            priorityQueue.RemoveAt(0);
            deleteMovementArea();
            removeUnitIndicators();
            generateNewMovArea = true;
            changeToNextUnit = false;
        }
        if (player1.getAlive() && player2.getAlive() && priorityQueue.Count == 0)
        {
            unitActions = false;
            startNewRound = true;
        }
        yield break;
    }

    public void changeUnitPosition(RaycastHit hit)
    {
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
        }
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

    //save movement indicators adjacent to enemy Unit, which we will use for melee engagement on that Unit
    public List<GameObject> findAdjacentMovementIndicators(Unit enemyUnit)
    {
        for (int row_i = 0; row_i < terrainGenerator.getNumberOfRows(); row_i++)
        {
            for (int col_i = 0; col_i < terrainGenerator.getNumberOfColumns(); col_i++)
            {
                if (terrainGenerator.getMatrixField()[row_i][col_i].getOccupyingEntity() != null)
                {
                    if (terrainGenerator.getMatrixField()[row_i][col_i].getOccupyingEntity() == enemyUnit.getModel())
                    {
                        //set lower column value
                        int lower_col = col_i - 1;
                        if (lower_col < 0) { lower_col = 0; }
                        //set max upper column index value
                        int upper_col = col_i + 1;
                        if (upper_col >= terrainGenerator.getNumberOfColumns()) { upper_col = terrainGenerator.getNumberOfColumns() - 1; }
                        //set lower row index value
                        int lower_row = row_i - 1;
                        if (lower_row < 0) { lower_row = 0; }
                        //set upper row index value
                        int upper_row = row_i + 1;
                        if (upper_row >= terrainGenerator.getNumberOfRows()) { upper_row = terrainGenerator.getNumberOfRows() - 1; }
                        List<GameObject> returnedMovementIndicators = new List<GameObject>();
                        for (int ind_r = lower_row; ind_r <= upper_row; ind_r++)
                        {
                            for (int ind_c = lower_col; ind_c <= upper_col; ind_c++)
                            {
                                if (terrainGenerator.getMatrixField()[ind_r][ind_c].getOccupyingEntity() != null)
                                {
                                    if (terrainGenerator.getMatrixField()[ind_r][ind_c].getOccupyingEntity().transform.childCount > 0)
                                    {
                                        if (terrainGenerator.getMatrixField()[ind_r][ind_c].getOccupyingEntity().transform.GetChild(0).gameObject.name == movementIndicator.transform.GetChild(0).gameObject.name)
                                        {
                                            // Debug.Log("MOVEMENT INDICATOR ADDED");
                                            returnedMovementIndicators.Add(terrainGenerator.getMatrixField()[ind_r][ind_c].getOccupyingEntity());
                                        }
                                    }
                                }
                            }
                        }
                        return returnedMovementIndicators;
                    }
                }
            }
        }
        Debug.Log("GameLogic - Enemy Units terrain block not found, eventhough it is on the field... Error");
        return null;
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

    //remove unit, if it died
    public void removeDeadUnit(Unit selectedUnit)
    {
        if (selectedUnit.getCurrentCount() == 0)
        {
            priorityQueue.Remove(selectedUnit);
            player1.checkUnitsStatus();
            player2.checkUnitsStatus();
            Destroy(selectedUnit.getModel());
        }
    }

    //after unit is deselected, release all movement areas he was to taking
    public void deleteMovementArea()
    {
        //delete indicator objects
        for (int index = 0; index < arrayOfIndicators.Count; index++)
        {
            Destroy(arrayOfIndicators[index]);
        }
        //clear array of indicators properly
        while (arrayOfIndicators.Count != 0)
        {
            arrayOfIndicators.RemoveAt(0);
        }
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
