using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.AI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearing;
    [SerializeField] private GameObject path;
    [SerializeField] private Material lineMaterial;

    [SerializeField] private ButtonBehaviour buttonBehaviour;
    [SerializeField] private MouseBehaviour mouseBehaviour;

    private MapGenerator mapGenerator;
    private ClearingInfoGenerator clearingInfoGenerator;
    private FactionGenerator factionGenerator;
    private FileGenerator fileGenerator;

    private WorldState worldState;

    void Awake()
    {
        worldState = new WorldState(clearing, path);
        
        mapGenerator = new MapGenerator(worldState);
        clearingInfoGenerator = new ClearingInfoGenerator(worldState);
        factionGenerator = new FactionGenerator(worldState);
        fileGenerator = new FileGenerator(worldState);
        
        buttonBehaviour.Init(worldState);
        mouseBehaviour.Init(worldState, buttonBehaviour);
    }
    
    void Start()
    {
        mapGenerator.GenerateClearings();
        mapGenerator.GeneratePaths();
        clearingInfoGenerator.GenerateDenizens();
        clearingInfoGenerator.GenerateClearingNames();
        
        factionGenerator.SetupMarquisate();
        factionGenerator.SetupEyrieDynasties();
        factionGenerator.SetupWoodlandAlliance();
        factionGenerator.SetupDenizens();
        factionGenerator.Reset();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !mouseBehaviour.IsDoingAction() && !buttonBehaviour.IsDoingAction())
        {
            worldState.DeleteAllClearings();
            mapGenerator.GenerateClearings();
            mapGenerator.GeneratePaths();
            clearingInfoGenerator.GenerateDenizens();
            clearingInfoGenerator.GenerateClearingNames();
            
            factionGenerator.SetupMarquisate();
            factionGenerator.SetupEyrieDynasties();
            factionGenerator.SetupWoodlandAlliance();
            factionGenerator.SetupDenizens();
            factionGenerator.Reset();
        }

        if (Input.GetKeyDown(KeyCode.S) && !mouseBehaviour.IsDoingAction() && !buttonBehaviour.IsDoingAction())
        {
            fileGenerator.GenerateFile();
        }
    }
}
