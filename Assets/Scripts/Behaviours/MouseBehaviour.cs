using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseBehaviour : MonoBehaviour
{
    private WorldState worldState;
    private ButtonBehaviour buttonBehaviour;

    private Vector3 mouseWorldPosition = Vector3.zero;
    
    //double clicking
    private bool hasClickedRecently = false;
    private float firstClickTime = 0.0f;
    private float doubleClickTime = 0.5f;
    
    //moving clearings
    private Clearing followingClearing;
    private bool isFollowing = false;
    private Vector3 clickStart;
    private Vector3 clearingStart;

    [SerializeField] private RectTransform canvasTransform;
    [SerializeField] private GameObject changeNameButton;

    //creating paths
    private bool hasTempPath = false;
    private Clearing temporaryPathStart;
    private Path temporaryPath;

    public void Init(WorldState worldState, ButtonBehaviour buttonBehaviour)
    {
        this.worldState = worldState;
        this.buttonBehaviour = buttonBehaviour;
    }
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.x = mousePos.x;
        mouseWorldPosition.y = mousePos.y;
        
        if (Input.GetMouseButtonDown(0))
        {
            bool doubleClick = false;
            if (!hasClickedRecently || Time.time - firstClickTime > doubleClickTime)
            {
                hasClickedRecently = true;
                firstClickTime = Time.time;
            }
            else
            {
                if (Time.time - firstClickTime < doubleClickTime)
                {
                    doubleClick = true;
                    hasClickedRecently = true;
                }
            }
            
            bool isOverButton = UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (!isOverButton)
            {
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, 0, LayerMask.GetMask("ClearingCircle", "ClearingPath"));

                switch (worldState.editMode)
                {
                    case EditMode.Create:
                        CreateMode(hit);
                        break;
                    case EditMode.Destroy:
                        DestroyMode(hit);
                        break;
                    case EditMode.Modify:
                        ModifyMode(hit, doubleClick);
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
        return isFollowing || hasTempPath || buttonBehaviour.changingName;
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
    
    private void ModifyMode(RaycastHit2D hit, bool doubleClick)
    {
        if (hit.collider != null && hit.collider.CompareTag("Clearing"))
        {
            Clearing clearing = hit.collider.gameObject.GetComponent<Clearing>();
            if (doubleClick)
            {
                Vector3 buttonScreenPosition = Camera.main.WorldToScreenPoint(clearing.GetPosition() + new Vector3(1,0,0));
                changeNameButton.transform.position = buttonScreenPosition;
                changeNameButton.SetActive(true);
                buttonBehaviour.ChangeEditingClearing(clearing);
            }
            else
            {
                changeNameButton.SetActive(false);
                followingClearing = clearing;
                isFollowing = true;
                clickStart = mouseWorldPosition;
                clearingStart = clearing.GetPosition();
            }
        }
    }
}
