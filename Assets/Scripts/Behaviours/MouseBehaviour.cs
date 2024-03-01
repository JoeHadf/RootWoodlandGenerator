using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class MouseBehaviour : MonoBehaviour
{
    private WorldState worldState;
    private EditClearingPanel editClearingPanel;
    
    private Vector3 mouseWorldPosition = Vector3.zero;
    
    //moving clearings
    private Clearing followingClearing;
    private bool isFollowing = false;
    private Vector3 clickStart;
    private Vector3 clearingStart;

    //creating paths
    private bool hasTempPath = false;
    private Clearing temporaryPathStart;
    private Path temporaryPath;

    public void Init(WorldState worldState, EditClearingPanel editClearingPanel)
    {
        this.worldState = worldState;
        this.editClearingPanel = editClearingPanel;
    }
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.x = mousePos.x;
        mouseWorldPosition.y = mousePos.y;
        
        if (Input.GetMouseButtonDown(0))
        {
            bool isOverButton = UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (!isOverButton && worldState.menuState == MenuState.Default)
            {
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, 0, LayerMask.GetMask("ClearingCircle", "ClearingPath"));

                switch (worldState.editMode)
                {
                    case EditMode.Write:
                        WriteMode(hit);
                        break;
                    case EditMode.Move:
                        MoveMode(hit);
                        break;
                    case EditMode.Create:
                        CreateMode(hit);
                        break;
                    case EditMode.River:
                        RiverMode(hit);
                        break;
                    case EditMode.Delete:
                        DestroyMode(hit);
                        break;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isFollowing = false;

            if (hasTempPath)
            {
                hasTempPath = false;
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, 0, LayerMask.GetMask("ClearingCircle"));

                if (hit.collider != null && hit.collider.CompareTag("Clearing"))
                {
                    Clearing endClearing = hit.collider.gameObject.GetComponent<Clearing>();

                    bool endDifferentToStart = temporaryPathStart.clearingID != endClearing.clearingID;
                    bool pathAlreadyExists = worldState.ClearingsAreAdjacent(temporaryPathStart.clearingID, endClearing.clearingID);

                    bool createPath = endDifferentToStart && !pathAlreadyExists;

                    if (createPath)
                    {
                        worldState.GeneratePath(temporaryPathStart, endClearing);
                    }
                }
                Destroy(temporaryPath.gameObject);
            }
        }

        if (isFollowing)
        {
            Vector3 displacement = mouseWorldPosition - clickStart;
            followingClearing.SetPosition(clearingStart + displacement);
        }
    }

    public bool IsDoingAction()
    {
        return isFollowing || hasTempPath;
    }
    
    private void WriteMode(RaycastHit2D hit)
    {
        if (hit.collider != null && hit.collider.CompareTag("Clearing"))
        {
            Clearing clearing = hit.collider.gameObject.GetComponent<Clearing>();
            editClearingPanel.EnterEditClearingMenuState(clearing);
        }
    }
    
    private void MoveMode(RaycastHit2D hit)
    {
        if (hit.collider != null && hit.collider.CompareTag("Clearing"))
        {
            Clearing clearing = hit.collider.gameObject.GetComponent<Clearing>();
            
            followingClearing = clearing;
            isFollowing = true;
            clickStart = mouseWorldPosition;
            clearingStart = clearing.GetPosition();
        }
    }

    private void CreateMode(RaycastHit2D hit)
    {
        if (hit.collider == null)
        {
            worldState.GenerateClearing(mouseWorldPosition);
        }
        else if (hit.collider.CompareTag("Clearing"))
        {
            Clearing startClearing = hit.collider.gameObject.GetComponent<Clearing>();
            Path newPath = worldState.GenerateTemporaryPath(startClearing);
            temporaryPathStart = startClearing;
            temporaryPath = newPath;
            hasTempPath = true;
        }
    }
    
    private void RiverMode(RaycastHit2D hit)
    {
        if (hit.collider != null && hit.collider.CompareTag("Clearing"))
        {
            Clearing clearing = hit.collider.gameObject.GetComponent<Clearing>();
            worldState.river.AddClearingToRiver(clearing);
        }
    }
    
    private void DestroyMode(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Clearing"))
            {
                Clearing clearing = hit.collider.gameObject.GetComponent<Clearing>();
                worldState.DeleteClearing(clearing.clearingID);
            }
            else if (hit.collider.CompareTag("Path"))
            {
                Path path = hit.collider.gameObject.GetComponent<Path>();
                worldState.DeletePath(path.pathID);
            }
        }
    }
}
