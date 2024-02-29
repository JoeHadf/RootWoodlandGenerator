using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EditModePanel : MonoBehaviour
{
    private WorldState worldState;

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
    }

    public void EnterWriteMode()
    {
        worldState.ChangeEditMode(EditMode.Write);
    }
    
    public void EnterMoveMode()
    {
        worldState.ChangeEditMode(EditMode.Move);
    }
    
    public void EnterCreateMode()
    {
        worldState.ChangeEditMode(EditMode.Create);
    }

    public void EnterRiverMode()
    {
        worldState.river.ClearRiver();
        worldState.ChangeEditMode(EditMode.River);
    }

    public void EnterDeleteMode()
    {
        worldState.ChangeEditMode(EditMode.Delete);
    }
}
