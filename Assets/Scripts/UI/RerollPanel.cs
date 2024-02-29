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
        worldState.DeleteAllClearings();
        worldState.DeleteAllClearings();
        mapGenerator.GenerateClearings();
        riverGenerator.GenerateRiver();
        mapGenerator.GeneratePaths();
        clearingInfoGenerator.GenerateDenizens();
        clearingInfoGenerator.GenerateClearingNames();
    }

    public void RerollPaths()
    {
        worldState.DeleteAllPaths();
        mapGenerator.GeneratePaths();
    }

    public void RerollRiver()
    {
        riverGenerator.GenerateRiver();
    }

    public void RerollDenizens()
    {
        clearingInfoGenerator.GenerateDenizens();
    }

    public void RerollNames()
    {
        clearingInfoGenerator.GenerateClearingNames();
    }
}
