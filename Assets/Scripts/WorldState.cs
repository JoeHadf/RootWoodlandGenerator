using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class WorldState
{
    public EditMode editMode { get; private set; }
    public MenuState menuState { get; private set; }

    public List<Clearing> clearings { get; private set; }
    public Dictionary<int, Clearing> clearingsByID { get; private set; }
    public List<Path> paths { get; private set; }
    public Dictionary<PathID, Path> pathsByID { get; private set; }
    public Dictionary<int, HashSet<int>> adjacentClearingsByID { get; private set; }

    public River river;
    public delegate void EnterMenuStateEventHandler();
    
    public event EnterMenuStateEventHandler OnEnterDefaultMenuState;
    public event EnterMenuStateEventHandler OnExitDefaultMenuState;
    public event EnterMenuStateEventHandler OnEnterEditClearingMenuState;
    public event EnterMenuStateEventHandler OnExitEditClearingMenuState;
    public event EnterMenuStateEventHandler OnEnterRerollFactionMenuState;
    public event EnterMenuStateEventHandler OnExitRerollFactionMenuState;
    public event EnterMenuStateEventHandler OnEnterEscapeMenuState;
    public event EnterMenuStateEventHandler OnExitEscapeMenuState;
    public event EnterMenuStateEventHandler OnEnterSaveMenuState;
    public event EnterMenuStateEventHandler OnExitSaveMenuState;
    public event EnterMenuStateEventHandler OnEnterLoadMenuState;
    public event EnterMenuStateEventHandler OnExitLoadMenuState;

    private GameObject clearingObject;
    private GameObject pathObject;
    
    private Transform clearingsParent;
    private Transform pathsParent;
    
    private int nextID;

    public WorldState(GameObject clearingObject, GameObject pathObject, River river)
    {
        editMode = EditMode.Write;
        clearings = new List<Clearing>();
        clearingsByID = new Dictionary<int, Clearing>();
        paths = new List<Path>();
        pathsByID = new Dictionary<PathID, Path>();
        adjacentClearingsByID = new Dictionary<int, HashSet<int>>();
        
        this.clearingObject = clearingObject;
        this.pathObject = pathObject;
        this.river = river;
        
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

    public bool CanEnterMenuState(MenuState nextState)
    {
        switch (menuState)
        {
            case MenuState.Default:
                if (nextState is MenuState.EditClearing or MenuState.RerollFaction or MenuState.Escape)
                {
                    return true;
                }
                break;
            case MenuState.EditClearing:
            case MenuState.RerollFaction:
                if (nextState == MenuState.Default)
                {
                    return true;
                }
                break;
            case MenuState.Escape:
                if (nextState is MenuState.Default or MenuState.Save or MenuState.Load)
                {
                    return true;
                }
                break;
            case MenuState.Save:
            case MenuState.Load:
                if (nextState == MenuState.Escape)
                {
                    return true;
                }
                break;
        }

        return false;
    }

    public bool TryEnterMenuState(MenuState nextState)
    {
        if (CanEnterMenuState(nextState))
        {
            InvokeOnExitMenuStateEvent(menuState);
            InvokeOnEnterMenuStateEvent(nextState);
            this.menuState = nextState;
            return true;
        }

        return false;
    }

    public bool TryGetClearingWithID(int clearingID, out Clearing clearing)
    {
        bool clearingExists = clearingsByID.TryGetValue(clearingID, out Clearing foundClearing);
        clearing = foundClearing;
        return clearingExists;
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

            adjacentClearingsByID.Remove(id);
        }

        Clearing clearingToDelete = clearingsByID[id];
        
        river.RemoveClearingFromRiver(clearingToDelete);

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

        int clearingCount = clearings.Count;
        
        for (int i = 0; i < clearingCount; i++)
        {
            DeleteClearing(clearings[0].clearingID);
        }
    }

    public void DeleteAllPaths()
    {
        int pathCount = paths.Count;
        
        for (int i = 0; i < pathCount; i++)
        {
            DeletePath(paths[0].pathID);
        }
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

    private void InvokeOnEnterMenuStateEvent(MenuState enteringState)
    {
        switch (enteringState)
        {
            case MenuState.Default:
                OnEnterDefaultMenuState?.Invoke();
                break;
            case MenuState.EditClearing:
                OnEnterEditClearingMenuState?.Invoke();
                break;
            case MenuState.RerollFaction:
                OnEnterRerollFactionMenuState?.Invoke();
                break;
            case MenuState.Escape:
                OnEnterEscapeMenuState?.Invoke();
                break;
            case MenuState.Save:
                OnEnterSaveMenuState?.Invoke();
                break;
            case MenuState.Load:
                OnEnterLoadMenuState?.Invoke();
                break;
        }
    }
    
    private void InvokeOnExitMenuStateEvent(MenuState enteringState)
    {
        switch (enteringState)
        {
            case MenuState.Default:
                OnExitDefaultMenuState?.Invoke();
                break;
            case MenuState.EditClearing:
                OnExitEditClearingMenuState?.Invoke();
                break;
            case MenuState.RerollFaction:
                OnExitRerollFactionMenuState?.Invoke();
                break;
            case MenuState.Escape:
                OnExitEscapeMenuState?.Invoke();
                break;
            case MenuState.Save:
                OnExitSaveMenuState?.Invoke();
                break;
            case MenuState.Load:
                OnExitLoadMenuState?.Invoke();
                break;
        }
    }
}

public enum EditMode
{
    Write,
    Move,
    Create,
    Delete,
    River
}

public enum MenuState
{
    Default,
    EditClearing,
    RerollFaction,
    Escape,
    Save,
    Load
}
