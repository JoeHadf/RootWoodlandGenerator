
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FactionGenerator
{
    private WorldState worldState;

    private List<CornerType> usedCorners = new List<CornerType>();
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
        CornerType strongholdCorner = ChooseRandomCorner();
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
        CornerType startingRoostCorner = ChooseRandomCorner();
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
                currentClearing.SetClearingPresence(FactionType.WoodlandAlliance);
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
                clearingsByID[adjacent].SetClearingPresence(FactionType.WoodlandAlliance);
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

    private Clearing GetCornerClearing(CornerType corner)
    {
        if (corner == CornerType.NoCorner)
        {
            return null;
        }
        
        int cornerID = (int)corner;
        bool isNorth = cornerID <= 2;
        bool isEast = cornerID % 2 == 0;

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

            float vertical = (isNorth) ? currentClearingPosition.y : -currentClearingPosition.y;
            float horizontal = (isEast) ? currentClearingPosition.x : -currentClearingPosition.x;

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

    private CornerType ChooseRandomCorner()
    {
        if (usedCorners.Count == 0)
        {
            int cornerID = Random.Range(1, 5);
            CornerType corner = (CornerType)cornerID;
            usedCorners.Add(corner);
            return corner;
        }
        
        if (usedCorners.Count == 1)
        {
            CornerType usedCorner = usedCorners[0];
            CornerType oppositeCorner = GetOppositeCorner(usedCorner); 
            usedCorners.Add(oppositeCorner);
            return oppositeCorner;
        }

        if (usedCorners.Count == 2)
        {
            int unusedCornerIndex = Random.Range(0, 2);
            
            CornerType usedCorner1 = usedCorners[0];
            CornerType usedCorner2 = usedCorners[1];
            
            int foundUnusedCornerCount = 0;

            int cornerID = -1;

            for (int i = 1; i <= 4; i++)
            {
                CornerType currentCorner = (CornerType)i;
                if (currentCorner == usedCorner1 || currentCorner == usedCorner2)
                {
                    continue;
                }
                
                if (foundUnusedCornerCount == unusedCornerIndex)
                {
                    cornerID = i;
                    break;
                }
                    
                foundUnusedCornerCount++;
            }

            CornerType corner = (CornerType)cornerID;
            usedCorners.Add(corner);
            return corner;
        }

        if (usedCorners.Count == 3)
        {
            CornerType usedCorner1 = usedCorners[0];
            CornerType usedCorner2 = usedCorners[1];
            CornerType usedCorner3 = usedCorners[2];
            
            for (int i = 1; i <= 4; i++)
            {
                CornerType currentCorner = (CornerType)i;

                bool isFirstCorner = currentCorner == usedCorner1;
                bool isSecondCorner = currentCorner == usedCorner2;
                bool isThirdCorner = currentCorner == usedCorner3;

                bool isRemainingCorner = !isFirstCorner && !isSecondCorner && !isThirdCorner;

                if (isRemainingCorner)
                {
                    usedCorners.Add(currentCorner);
                    return currentCorner;
                }
            }
        }

        return CornerType.NoCorner;
    }

    private CornerType GetOppositeCorner(CornerType corner)
    {
        switch (corner)
        {
            case CornerType.NorthWest:
                return CornerType.SouthEast;
            case CornerType.NorthEast:
                return CornerType.SouthWest;
            case CornerType.SouthWest:
                return CornerType.NorthEast;
            case CornerType.SouthEast:
                return CornerType.NorthWest;
        }

        return CornerType.NoCorner;
    }
}

public enum CornerType
{
    NoCorner = 0,
    NorthWest = 1,
    NorthEast = 2,
    SouthWest = 3,
    SouthEast = 4,
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
