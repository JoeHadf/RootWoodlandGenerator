using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class MapGenerator
{
    private WorldState worldState;
    
    private float xRange = 9.5f;
    private float yRange = 4.0f;
    
    private int clearingCount = 12;

    private int forceSteps = 500;
    private float maxForce = 0.5f;
    private float curveSteepness = 0.01f;

    public MapGenerator(WorldState worldState)
    {
        this.worldState = worldState;
    }

    public void GenerateClearings()
    {
        float clearingDistance =  2 * GlobalConstants.clearingRadius + 0.5f * GlobalConstants.pathWidth + GlobalConstants.minPathLength;
        
        Vector3[] clearingPositions = new Vector3[clearingCount];
        
        for (int i = 0; i < clearingPositions.Length; i++)
        {
            float xPosition = Random.Range(-xRange, xRange);
            float yPosition = Random.Range(-yRange, yRange);
            
            worldState.GenerateClearing(new Vector3(xPosition, yPosition, 0));
        }
        
        bool overlapping = ClearingsOverlap(clearingDistance);

        if (overlapping)
        {
            for (int i = 0; i < forceSteps; i++)
            {
                ApplyForce(clearingDistance);

                if (!ClearingsOverlap(clearingDistance))
                {
                    break;
                }
            }
        }
    }
    
    public void GeneratePaths()
    {
        HashSet<PathID> possiblePaths = GetPossiblePaths();
        Dictionary<int, Clearing> clearingsByID =  worldState.clearingsByID;
        bool isConnected = false;

        while (!isConnected)
        {
            int randomPathIndex = Random.Range(0, possiblePaths.Count);
            PathID pathToAdd = possiblePaths.ElementAt(randomPathIndex);
            possiblePaths.Remove(pathToAdd);
            Path newPath = worldState.GeneratePath(clearingsByID[pathToAdd.startID], clearingsByID[pathToAdd.endID]);

            isConnected = ClearingsAreConnected();

            if (!isConnected)
            {
                List<PathID> pathsToRemove = new List<PathID>();
                foreach (PathID path in possiblePaths)
                {
                    if (PathsIntersect(path, newPath.pathID))
                    {
                        pathsToRemove.Add(path);
                    }
                }

                for (int i = 0; i < pathsToRemove.Count; i++)
                {
                    possiblePaths.Remove(pathsToRemove[i]);
                }
            }
        }
    }

    private HashSet<PathID> GetPossiblePaths()
    {
        List<Clearing> clearings = worldState.clearings;
        HashSet<PathID> possiblePaths = new HashSet<PathID>();

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing startClearing = clearings[i];
            
            for (int j = i + 1; j < clearings.Count; j++)
            {
                Clearing endClearing = clearings[j];
                bool pathIntersectsClearing = PathIntersectsClearing(startClearing, endClearing);
                
                if (!pathIntersectsClearing)
                {
                    possiblePaths.Add(new PathID(startClearing.clearingID, endClearing.clearingID));
                }
            }
        }
        return possiblePaths;
    }

    private bool ClearingsAreConnected()
    {
        List<Clearing> clearings = worldState.clearings;
        Dictionary<int, HashSet<int>> adjacentClearingsByID = worldState.adjacentClearingsByID;
        
        int currentClearingCount = clearings.Count;
        if (currentClearingCount == 0)
        {
            return true;
        }

        Clearing initialClearing = clearings[0];
        int initialClearingID = initialClearing.clearingID;
        
        HashSet<int> connectedClearings = new HashSet<int>();
        connectedClearings.Add(initialClearingID);
        
        Queue<int> clearingsToCheck = new Queue<int>();
        clearingsToCheck.Enqueue(initialClearingID);

        while (clearingsToCheck.Count > 0)
        {
            int currentClearingID = clearingsToCheck.Dequeue();

            if (adjacentClearingsByID.TryGetValue(currentClearingID, out HashSet<int> adjacentClearings))
            {
                foreach (int adjacent in adjacentClearings)
                {
                    if (!connectedClearings.Contains(adjacent))
                    {
                        connectedClearings.Add(adjacent);
                        clearingsToCheck.Enqueue(adjacent);
                    }
                }
            }
        }

        return connectedClearings.Count == currentClearingCount;
    }

    //determines whether the path between clearing1 and clearing2 would intersect another clearing
    private bool PathIntersectsClearing(Clearing clearing1, Clearing clearing2)
    {
        List<Clearing> clearings = worldState.clearings;
        
        Vector3 c1Pos = clearing1.GetPosition();
        Vector3 c2Pos = clearing2.GetPosition();
        Vector3 c1ToC2 = c2Pos - c1Pos;

        float radius = GlobalConstants.clearingRadius + 0.5f * GlobalConstants.pathWidth;

        float squareTerm = (c1ToC2.x) * (c1ToC2.x) + (c1ToC2.y) * (c1ToC2.y);
        
        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            
            if (currentClearing.clearingID == clearing1.clearingID || currentClearing.clearingID == clearing2.clearingID)
            {
                continue;
            }
            
            Vector3 kPos = currentClearing.GetPosition();
            Vector3 kToC1 = c1Pos - kPos;

            float linearTerm = 2 * c1ToC2.x * (kToC1.x) + 2 * c1ToC2.y * (kToC1.y);
            float constantTerm = kToC1.x * kToC1.x + kToC1.y * kToC1.y - radius * radius;

            float discriminant = linearTerm * linearTerm - 4 * squareTerm * constantTerm;

            if (discriminant > 0)
            {
                double solution1 = (-linearTerm + Math.Sqrt(discriminant)) / (2 * squareTerm);
                double solution2 = (-linearTerm - Math.Sqrt(discriminant)) / (2 * squareTerm);

                if (solution1 is > 0 and < 1 || solution2 is > 0 and < 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool PathsIntersect(PathID path1, PathID path2)
    {
        Dictionary<int, Clearing> clearingsByID = worldState.clearingsByID;

        Clearing path1StartClearing = clearingsByID[path1.startID];
        Clearing path1EndClearing = clearingsByID[path1.endID];
        Clearing path2StartClearing = clearingsByID[path2.startID];
        Clearing path2EndClearing = clearingsByID[path2.endID];

        Vector3 path1Direction = path1EndClearing.GetPosition() - path1StartClearing.GetPosition();
        Vector3 path2Direction = path2EndClearing.GetPosition() - path2StartClearing.GetPosition();
        
        path1Direction.Normalize();
        path2Direction.Normalize();
        
        Vector3 path1Start = path1StartClearing.GetPathStart(path1Direction);
        Vector3 path1End = path1EndClearing.GetPathStart(-path1Direction);
        Vector3 path2Start = path2StartClearing.GetPathStart(path2Direction);
        Vector3 path2End = path2EndClearing.GetPathStart(-path2Direction);
        
        return LinesIntersect(path1Start, path1End, path2Start, path2End);
    }

    private bool ClearingsOverlap(float overlapDistance)
    {
        List<Clearing> clearings = worldState.clearings;
        
        for (int i = 1; i < clearings.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                float distance = (clearings[i].GetPosition() - clearings[j].GetPosition()).magnitude;
                if (distance < overlapDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ApplyForce(float maxForceDistance)
    {
        List<Clearing> clearings = worldState.clearings;

        Vector3[] forces = new Vector3[clearings.Count];

        for (int i = 0; i < clearings.Count; i++)
        {
            Vector3 currentPosition = clearings[i].GetPosition();

            forces[i] = Vector3.zero;

            for (int j = 0; j < clearings.Count; j++)
            {
                if (j == i)
                {
                    continue;
                }

                Vector3 otherPosition = clearings[j].GetPosition();

                Vector3 direction = currentPosition - otherPosition;
                float distance = direction.magnitude;

                float forceAmount = maxForce * InverseInvSquerp(distance, 0, maxForceDistance);
                forces[i] += direction.normalized * forceAmount;
            }
        }

        for (int i = 0; i < forces.Length; i++)
        {
            Clearing clearing = clearings[i];
            clearing.SetPosition(clearing.GetPosition() + forces[i]);
        }
    }
    
    //A value between 0 and 1 representing where value falls within a and b that squishes values closer to a or b
    private float InverseInvSquerp(float value, float a, float b)
    {
        if (value > b)
        {
            return 0;
        }
        
        float lerp = Mathf.InverseLerp(a, b, value);
        return Math.Min(1 / (lerp * lerp) * curveSteepness, 1.0f);
    }

    public int GetNumberOfPaths()
    {
        int dice1 = Random.Range(1, 6);
        int dice2 = Random.Range(1, 6);
        int sum = dice1 + dice2;

        switch (sum)
        {
            case 2:
                return 1;
            case 3:
            case 4:
                return 2;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return 3;
            case 10:
            case 11:
                return 4;
            case 12:
                return 5;
            default:
                return 0;
        }
    }
    
    private bool LinesIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return CounterClockWise(a, c, d) != CounterClockWise(b, c, d) &&
               CounterClockWise(a, b, c) != CounterClockWise(a, b, d);
    }

    private bool CounterClockWise(Vector3 a, Vector3 b, Vector3 c)
    {
        return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
    }
}

public struct PathID
{
    public int startID { get; private set; }
    public int endID { get; private set; }

    public PathID(int clearing1ID, int clearing2ID)
    {
        startID = Math.Min(clearing1ID, clearing2ID);
        endID = Math.Max(clearing1ID, clearing2ID);
    }

    public override bool Equals([CanBeNull] object obj)
    {
        if (obj is PathID other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(PathID other)
    {
        return startID == other.startID && endID == other.endID;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(startID, endID);
    }
}