using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class EditModePanel : MonoBehaviour
{
    private WorldState worldState;

    [SerializeField] private Button writeButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button riverButton;
    [SerializeField] private Button deleteButton;

    [SerializeField] private GameObject selector;

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
    }

    public void EnterWriteMode()
    {
        EnterEditMode(EditMode.Write);
    }
    
    public void EnterMoveMode()
    {
        EnterEditMode(EditMode.Move);
    }
    
    public void EnterCreateMode()
    {
        EnterEditMode(EditMode.Create);
    }

    public void EnterRiverMode()
    {
        EnterEditMode(EditMode.River);
    }

    public void EnterDeleteMode()
    {
        EnterEditMode(EditMode.Delete);
    }

    private void EnterEditMode(EditMode editMode)
    {
        if (worldState.menuState == MenuState.Default)
        {
            if (editMode == EditMode.River)
            {
                worldState.river.ClearRiver();
            }
            
            worldState.ChangeEditMode(editMode);
            selector.transform.localPosition = GetEditModeButton(editMode).transform.localPosition;
        }
    }

    public Button GetEditModeButton(EditMode editMode)
    {
        switch (editMode)
        {
            case EditMode.Write:
                return writeButton;
            case EditMode.Move:
                return moveButton;
            case EditMode.Create:
                return createButton;
            case EditMode.River:
                return riverButton;
            case EditMode.Delete:
                return deleteButton;
            default:
                return writeButton;
            
        }
    }
}
