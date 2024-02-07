
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FactionGenerator
{
    private WorldState worldState;

    private List<Corner> usedCorners = new List<Corner>();
    private HashSet<int> usedCornerClearingIDs = new HashSet<int>();
    private HashSet<int> contestedClearingIDs = new HashSet<int>();

    public FactionGenerator(WorldState worldState)
    {
        this.worldState = worldState;
    }
    
    public void Reset()
    {
        usedCorners.Clear();
        usedCornerClearingIDs.Clear();
        contestedClearingIDs.Clear();
    }

    public void SetupMarquisate()
    {
        Corner strongholdCorner = ChooseRandomCorner();
        Clearing strongholdClearing = GetCornerClearing(strongholdCorner);
        
        strongholdClearing.SetClearingControl(FactionType.Marquisate);
        strongholdClearing.SetHasBuilding(true);

        Dictionary<int, List<int>> clearingsByDistanceFromStronghold = BreadthFirstSearch(strongholdClearing.clearingID);

        Dictionary<int, Clearing> clearingsByID = worldState.clearingsByID;

        for (int distance = 1; distance <= 4; distance++)
        {
            if (clearingsByDistanceFromStronghold.TryGetValue(distance, out List<int> currentDistanceClearings))
            {
                foreach (int clearingID in currentDistanceClearings)
                {
                    bool inControl = MarquisateIsInControl(distance);

                    if (inControl)
                    {
                        clearingsByID[clearingID].SetClearingControl(FactionType.Marquisate);
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    public void SetupEyrieDynasties()
    {
        Corner startingRoostCorner = ChooseRandomCorner();
        Clearing startingRoostClearing = GetCornerClearing(startingRoostCorner);
        
        startingRoostClearing.SetClearingControl(FactionType.EyrieDynasties);
        startingRoostClearing.SetHasBuilding(true);

        Dictionary<int, List<int>> clearingsByDistanceFromStartingRoost = BreadthFirstSearch(startingRoostClearing.clearingID);
        
        Dictionary<int, Clearing> clearingsByID = worldState.clearingsByID;
        int controlCount = 0;
        int roostCount = 0;

        bool roostsExceeded = false;
        bool controlExceeded = false;

        for (int distance = 1; distance <= 3; distance++)
        {
            if (!controlExceeded && clearingsByDistanceFromStartingRoost.TryGetValue(distance, out List<int> currentDistanceClearings))
            {
                foreach (int clearingID in currentDistanceClearings)
                {
                    (bool inControl, bool hasRoost) = EyrieIsInControl(distance);

                    Clearing currentClearing = clearingsByID[clearingID];

                    if (!currentClearing.hasBuilding)
                    {
                        if (inControl && !controlExceeded)
                        {
                            if (currentClearing.clearingControl != FactionType.Denizens &&
                                currentClearing.clearingControl != FactionType.EyrieDynasties)
                            {
                                contestedClearingIDs.Add(currentClearing.clearingID);
                            }
                            controlCount++;
                            currentClearing.SetClearingControl(FactionType.EyrieDynasties);
                            if (controlCount >= 6)
                            {
                                controlExceeded = true;
                                roostsExceeded = true;
                            }
                        }

                        if (hasRoost && !roostsExceeded)
                        {
                            roostCount++;
                            currentClearing.SetHasBuilding(true);
                            if (roostCount >= 4)
                            {
                                roostsExceeded = true;
                            }
                        }

                        if (controlExceeded)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    public void SetupWoodlandAlliance()
    {
        List<Clearing> clearings = worldState.clearings;
        List<int> sympatheticClearingIDs = new List<int>();

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            bool hasSympathy = HasWoodlandAllianceSympathy(currentClearing);
            if (hasSympathy)
            {
                currentClearing.SetHasSympathy(true);
                sympatheticClearingIDs.Add(currentClearing.clearingID);
            }
        }

        Dictionary<int, Clearing> clearingsByID = worldState.clearingsByID;

        int baseClearingID = -1;
        bool makeAdjacentClearingsSympathetic = false;

        foreach (int clearingID in sympatheticClearingIDs)
        {
            Clearing sympatheticClearing = clearingsByID[clearingID];

            (bool inControl, bool adjacentClearingsSympathetic) = WoodlandAllianceIsInControl();

            if (inControl)
            {
                sympatheticClearing.SetClearingControl(FactionType.WoodlandAlliance);
                sympatheticClearing.SetHasBuilding(true);
                baseClearingID = sympatheticClearing.clearingID;
                makeAdjacentClearingsSympathetic = adjacentClearingsSympathetic;
                break;
            }
        }

        if (makeAdjacentClearingsSympathetic)
        {
            HashSet<int> adjacentClearings = worldState.adjacentClearingsByID[baseClearingID];

            foreach (int adjacent in adjacentClearings)
            {
                clearingsByID[adjacent].SetHasSympathy(true);
            }
        }
    }

    public void SetupDenizens()
    {
        List<Clearing> clearings = worldState.clearings;

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];

            if (currentClearing.clearingControl != FactionType.Denizens && !currentClearing.hasBuilding)
            {
                bool inControl = DenizensAreInControl();
                if (inControl)
                {
                    currentClearing.SetClearingControl(FactionType.Denizens);
                }
            }
        }
    }

    private bool MarquisateIsInControl(int distanceFromStronghold)
    {
        int controlThreshold;

        switch (distanceFromStronghold)
        {
            case 1:
                controlThreshold = 5;
                break;
            case 2:
                controlThreshold = 7;
                break;
            case 3:
                controlThreshold = 10;
                break;
            case 4:
                controlThreshold = 12;
                break;
            default:
                controlThreshold = 13;
                break;
        }

        int diceRoll = Roll2D6();

        bool inControl = diceRoll >= controlThreshold;
        return inControl;
    }

    private (bool, bool) EyrieIsInControl(int distanceFromStartingRoost)
    {
        int controlWithRoostThreshold;
        int controlWithoutRoostThreshold;

        switch (distanceFromStartingRoost)
        {
            case 1:
                controlWithRoostThreshold = 9;
                controlWithoutRoostThreshold = 6;
                break;
            case 2:
                controlWithRoostThreshold = 11;
                controlWithoutRoostThreshold = 9;
                break;
            case 3:
                controlWithRoostThreshold = 12;
                controlWithoutRoostThreshold = 11;
                break;
            default:
                controlWithRoostThreshold = 13;
                controlWithoutRoostThreshold = 13;
                break;
        }

        int diceRoll = Roll2D6();

        bool inControl;
        bool hasRoost;

        if (diceRoll >= controlWithRoostThreshold)
        {
            inControl = true;
            hasRoost = true;
        }
        else if (diceRoll >= controlWithoutRoostThreshold)
        {
            inControl = true;
            hasRoost = false;
        }
        else
        {
            inControl = false;
            hasRoost = false;
        }
        
        return (inControl, hasRoost);
    }

    private (bool, bool) WoodlandAllianceIsInControl()
    {
        int diceRoll = Roll2D6();

        bool inControl = diceRoll >= 10;
        bool adjacentClearingsSympathetic = diceRoll >= 12;

        return (inControl, adjacentClearingsSympathetic);
    }
    
    private bool HasWoodlandAllianceSympathy(Clearing clearing)
    {
        int sympathyThreshold;

        if (clearing.clearingControl == FactionType.Denizens)
        {
            sympathyThreshold = 9;
        }
        else if (contestedClearingIDs.Contains(clearing.clearingID))
        {
            sympathyThreshold = 8;
        }
        else
        {
            sympathyThreshold = 11;
        }

        int diceRoll = Roll2D6();

        bool hasSympathy = diceRoll >= sympathyThreshold;

        return hasSympathy;
    }

    private bool DenizensAreInControl()
    {
        int diceRoll = Roll2D6();

        bool inControl = diceRoll >= 11;
        return inControl;
    }

    private int Roll2D6()
    {
        int dice1 = Random.Range(1, 7);
        int dice2 = Random.Range(1, 7);
        return dice1 + dice2;
    }

    private Dictionary<int, List<int>> BreadthFirstSearch(int startClearingID)
    {
        Dictionary<int, int> clearingDistances = new Dictionary<int, int>();
        HashSet<int> foundClearings = new HashSet<int>();
        Queue<int> clearingsToCheck = new Queue<int>(); 
        clearingDistances.Add(startClearingID, 0);
        foundClearings.Add(startClearingID);
        clearingsToCheck.Enqueue(startClearingID);

        while (clearingsToCheck.Count > 0)
        {
            int currentClearing = clearingsToCheck.Dequeue();
            int currentClearingDistance = clearingDistances[currentClearing];
            HashSet<int> adjacentClearings = worldState.GetAdjacentClearings(currentClearing);

            foreach (int adjacent in adjacentClearings)
            {
                if (!foundClearings.Contains(adjacent))
                {
                    foundClearings.Add(adjacent);
                    clearingsToCheck.Enqueue(adjacent);
                    clearingDistances.Add(adjacent, currentClearingDistance + 1);
                }
            }
        }

        Dictionary<int, List<int>> clearingsByDistance = new Dictionary<int, List<int>>();

        foreach (KeyValuePair<int, int> clearingDistancePair in clearingDistances)
        {
            int clearingID = clearingDistancePair.Key;
            int distance = clearingDistancePair.Value;

            if (!clearingsByDistance.ContainsKey(distance))
            {
                clearingsByDistance[distance] = new List<int>();
            }
            
            clearingsByDistance[distance].Add(clearingID);
        }

        return clearingsByDistance;
    }

    private Clearing GetCornerClearing(Corner corner)
    {
        List<Clearing> clearings = worldState.clearings;

        float currentClosestDistance = float.MinValue;
        Clearing currentClosestClearing = clearings[0];

        for (int i = 1; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            
            if (usedCornerClearingIDs.Contains(currentClearing.clearingID))
            {
                continue;
            }
            
            Vector3 currentClearingPosition = currentClearing.GetPosition();

            float vertical = (corner.isNorth) ? currentClearingPosition.y : -currentClearingPosition.y;
            float horizontal = (corner.isEast) ? currentClearingPosition.x : -currentClearingPosition.x;

            float clearingClosestDistance = vertical + horizontal;

            if (clearingClosestDistance > currentClosestDistance)
            {
                currentClosestClearing = currentClearing;
                currentClosestDistance = clearingClosestDistance;
            }
        }

        usedCornerClearingIDs.Add(currentClosestClearing.clearingID);
        return currentClosestClearing;
    }

    private Corner ChooseRandomCorner()
    {
        Corner corner;
        if (usedCorners.Count == 1)
        {
            corner = usedCorners[0].GetOppositeCorner();
        }
        else if (usedCorners.Count == 2)
        {
            Corner firstCorner = usedCorners[0];
            bool flipNorth = Convert.ToBoolean(Random.Range(0,2));
            corner = (flipNorth) ? firstCorner.FlipNorth() : firstCorner.FlipEast();
        }
        else if (usedCorners.Count == 3)
        {
            Corner thirdCorner = usedCorners[2];
            corner = thirdCorner.GetOppositeCorner();
        }
        else
        {
            bool isNorth = Convert.ToBoolean(Random.Range(0, 2));
            bool isEast = Convert.ToBoolean(Random.Range(0, 2));
            corner = new Corner(isNorth, isEast);
        }
        
        usedCorners.Add(corner);
        return corner;
    }
}

public struct Corner
{
    public bool isNorth;
    public bool isEast;

    public Corner(bool isNorth, bool isEast)
    {
        this.isNorth = isNorth;
        this.isEast = isEast;
    }

    public Corner GetOppositeCorner()
    {
        return new Corner(!isNorth, !isEast);
    }

    public Corner FlipNorth()
    {
        return new Corner(!isNorth, isEast);
    }

    public Corner FlipEast()
    {
        return new Corner(isNorth, !isEast);
    }
}

public enum FactionType
{
    Marquisate = 1,
    EyrieDynasties = 2,
    WoodlandAlliance = 3,
    LizardCult = 4,
    RiverfolkCompany = 5,
    GrandDuchy = 6,
    CorvidConspiracy = 7,
    Denizens = 8
}
