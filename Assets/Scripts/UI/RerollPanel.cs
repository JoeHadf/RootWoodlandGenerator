using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RerollPanel : MonoBehaviour
{
    private WorldState worldState;
    private MapGenerator mapGenerator;
    private ClearingInfoGenerator clearingInfoGenerator;
    private RiverGenerator riverGenerator;

    public void Init(WorldState worldState, MapGenerator mapGenerator, ClearingInfoGenerator clearingInfoGenerator,
        RiverGenerator riverGenerator)
    {
        this.worldState = worldState;
        this.mapGenerator = mapGenerator;
        this.clearingInfoGenerator = clearingInfoGenerator;
        this.riverGenerator = riverGenerator;
    }
    
    
    public void RerollWoodland()
    {
        if (worldState.menuState == MenuState.Default)
        {
            worldState.DeleteAllClearings();
            worldState.DeleteAllClearings();
            mapGenerator.GenerateClearings();
            riverGenerator.GenerateRiver();
            mapGenerator.GeneratePaths();
            clearingInfoGenerator.GenerateDenizens();
            clearingInfoGenerator.GenerateClearingNames();
        }
    }

    public void RerollPaths()
    {
        if (worldState.menuState == MenuState.Default)
        {
            worldState.DeleteAllPaths();
            mapGenerator.GeneratePaths();
        }
    }

    public void RerollRiver()
    {
        if (worldState.menuState == MenuState.Default)
        {
            riverGenerator.GenerateRiver();
        }
    }

    public void RerollDenizens()
    {
        if (worldState.menuState == MenuState.Default)
        {
            clearingInfoGenerator.GenerateDenizens();
        }
    }

    public void RerollNames()
    {
        if (worldState.menuState == MenuState.Default)
        {
            clearingInfoGenerator.GenerateClearingNames();
        }
    }
}
