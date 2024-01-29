using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class WorldState
{
    public static EditMode editMode { get; private set; }

    public List<Clearing> clearings { get; private set; }
    public Dictionary<int, Clearing> clearingsByID { get; private set; }
    public List<Path> paths { get; private set; }
    public Dictionary<PathID, Path> pathsByID { get; private set; }
    
    public Dictionary<int, HashSet<int>> adjacentClearingsByID { get; private set; }

    public WorldState()
    {
        editMode = EditMode.Modify;
        clearings = new List<Clearing>();
        clearingsByID = new Dictionary<int, Clearing>();
        paths = new List<Path>();
        pathsByID = new Dictionary<PathID, Path>();
        adjacentClearingsByID = new Dictionary<int, HashSet<int>>();
    }

    public void ChangeEditMode(EditMode newMode)
    {
        editMode = newMode;
    }

    public void RegisterNewClearing(Clearing newClearing)
    {
        clearings.Add(newClearing);
        clearingsByID.Add(newClearing.clearingID, newClearing);
    }

    public void RegisterNewPath(Path newPath)
    {
        paths.Add(newPath);
        pathsByID.Add(newPath.pathID, newPath);

        RegisterAdjacency(newPath.pathID.startID, newPath.pathID.endID);
    }

    private void RegisterAdjacency(int clearingID1, int clearingID2)
    {
        EnsureAdjacencySetExists(clearingID1);
        EnsureAdjacencySetExists(clearingID2);

        adjacentClearingsByID[clearingID1].Add(clearingID2);
        adjacentClearingsByID[clearingID2].Add(clearingID1);
    }

    private void EnsureAdjacencySetExists(int clearingID)
    {
        if (!adjacentClearingsByID.ContainsKey(clearingID))
        {
            adjacentClearingsByID[clearingID] = new HashSet<int>();
        }
    }

    public void DeleteClearings()
    {
        DeletePaths();
        
        for (int i = 0; i < clearings.Count; i++)
        {
            Object.Destroy(clearings[i].GameObject());
        }
        
        clearings.Clear();
        clearingsByID.Clear();
    }

    public void DeletePaths()
    {
        for (int i = 0; i < paths.Count; i++)
        {
            Object.Destroy(paths[i].GameObject());
        }

        adjacentClearingsByID.Clear();
        paths.Clear();
        pathsByID.Clear();
    }

    public bool ClearingsAreAdjacent(int clearing1ID, int clearing2ID)
    {
        if (adjacentClearingsByID.TryGetValue(clearing1ID, out HashSet<int> adjacentClearings))
        {
            return adjacentClearings.Contains(clearing2ID);
        }

        return false;
    }
}

public enum EditMode
{
    Modify,
    Create,
    Destroy
}
