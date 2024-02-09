using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class WorldState
{
    public EditMode editMode { get; private set; }

    public List<Clearing> clearings { get; private set; }
    public Dictionary<int, Clearing> clearingsByID { get; private set; }
    public List<Path> paths { get; private set; }
    public Dictionary<PathID, Path> pathsByID { get; private set; }
    
    public Dictionary<int, HashSet<int>> adjacentClearingsByID { get; private set; }

    private GameObject clearingObject;
    private GameObject pathObject;
    
    private Transform clearingsParent;
    private Transform pathsParent;
    
    private int nextID;

    public WorldState(GameObject clearingObject, GameObject pathObject)
    {
        editMode = EditMode.Modify;
        clearings = new List<Clearing>();
        clearingsByID = new Dictionary<int, Clearing>();
        paths = new List<Path>();
        pathsByID = new Dictionary<PathID, Path>();
        adjacentClearingsByID = new Dictionary<int, HashSet<int>>();
        
        this.clearingObject = clearingObject;
        this.pathObject = pathObject;
        
        clearingsParent = new GameObject("ClearingsParent").transform;
        pathsParent = new GameObject("PathsParent").transform;

        nextID = 0;
    }

    public void ChangeEditMode(EditMode newMode)
    {
        editMode = newMode;
    }
    
    public Clearing GenerateClearing(Vector3 clearingPosition)
    {
        GameObject currentClearingObject = Object.Instantiate(clearingObject, clearingPosition, Quaternion.identity, clearingsParent);
        Clearing currentClearing = currentClearingObject.GetComponent<Clearing>();
        currentClearing.Init(GetID());
        RegisterNewClearing(currentClearing);
        return currentClearing;
    }
    
    public Path GeneratePath(int startClearingID, int endClearingID)
    {
        return GeneratePath(clearingsByID[startClearingID], clearingsByID[endClearingID]);
    }
    
    public Path GeneratePath(Clearing startClearing, Clearing endClearing)
    {
        PathID generatedPathID = new PathID(startClearing.clearingID, endClearing.clearingID);

        if (!pathsByID.ContainsKey(generatedPathID))
        {
            GameObject currentPathObject = Object.Instantiate(pathObject, Vector3.zero, Quaternion.identity, pathsParent);
            Path currentPath = currentPathObject.GetComponent<Path>();
            currentPath.Init(startClearing, endClearing);
            RegisterNewPath(currentPath);
        }

        return pathsByID[generatedPathID];
    }

    public Path GenerateTemporaryPath(Clearing startClearing)
    {
        GameObject currentPathObject = Object.Instantiate(pathObject, Vector3.zero, Quaternion.identity, pathsParent);
        Path currentPath = currentPathObject.GetComponent<Path>();
        currentPath.Init(startClearing);
        return currentPath;
    }

    private void RegisterNewClearing(Clearing newClearing)
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
    
    private int GetID()
    {
        int id = nextID;
        nextID++;
        return id;
    }

    public void DeleteClearing(int id)
    {
        if (adjacentClearingsByID.TryGetValue(id, out HashSet<int> adjacentClearings))
        {
            HashSet<int> adjacentCopy = new HashSet<int>(adjacentClearings);
            foreach (int adjacent in adjacentCopy)
            {
                PathID adjacentPathID = new PathID(id, adjacent);
                DeletePath(adjacentPathID);
            }
        }

        Clearing clearingToDelete = clearingsByID[id];

        for (int i = 0; i < clearings.Count; i++)
        {
            if (clearings[i].clearingID == id)
            {
                clearings.RemoveAt(i);
                break;
            }
        }
        
        Object.Destroy(clearingToDelete.gameObject);
    }

    public void DeletePath(PathID id)
    {
        Path pathToDelete = pathsByID[id];

        pathToDelete.DeregisterAdjacentClearings();
        
        pathsByID.Remove(id);

        adjacentClearingsByID[id.startID].Remove(id.endID);
        adjacentClearingsByID[id.endID].Remove(id.startID);

        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].pathID.Equals(id))
            {
                paths[i].OnDestroy();
                paths.RemoveAt(i);
                break;
            }
        }
        
        Object.Destroy(pathToDelete.gameObject);
    }

    public void DeleteAllClearings()
    {
        DeleteAllPaths();
        
        for (int i = 0; i < clearings.Count; i++)
        {
            Object.Destroy(clearings[i].GameObject());
        }
        
        clearings.Clear();
        clearingsByID.Clear();
    }

    public void DeleteAllPaths()
    {
        for (int i = 0; i < paths.Count; i++)
        {
            paths[i].OnDestroy();
            Object.Destroy(paths[i].GameObject());
        }

        for (int i = 0; i < clearings.Count; i++)
        {
            clearings[i].DeregisterAllPaths();
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

    public HashSet<int> GetAdjacentClearings(int clearingID)
    {
        if (adjacentClearingsByID.TryGetValue(clearingID, out HashSet<int> adjacentClearings))
        {
            return adjacentClearings;
        }

        return new HashSet<int>();
    }
}

public enum EditMode
{
    Modify,
    Create,
    Destroy
}
