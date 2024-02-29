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
        worldState.ChangeEditMode(EditMode.Write);
        selector.transform.localPosition = writeButton.transform.localPosition;
    }
    
    public void EnterMoveMode()
    {
        worldState.ChangeEditMode(EditMode.Move);
        selector.transform.localPosition = moveButton.transform.localPosition;
    }
    
    public void EnterCreateMode()
    {
        worldState.ChangeEditMode(EditMode.Create);
        selector.transform.localPosition = createButton.transform.localPosition;
    }

    public void EnterRiverMode()
    {
        worldState.river.ClearRiver();
        worldState.ChangeEditMode(EditMode.River);
        selector.transform.localPosition = riverButton.transform.localPosition;
    }

    public void EnterDeleteMode()
    {
        worldState.ChangeEditMode(EditMode.Delete);
        selector.transform.localPosition = deleteButton.transform.localPosition;
    }
}
