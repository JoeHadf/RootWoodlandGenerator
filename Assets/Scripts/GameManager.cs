using System;
using System.Collections.Generic;
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

    private WorldState worldState;

    void Awake()
    {
        worldState = new WorldState(clearing, path);
        
        mapGenerator = new MapGenerator(worldState);
        clearingInfoGenerator = new ClearingInfoGenerator(worldState);
        buttonBehaviour.Init(worldState);
        mouseBehaviour.Init(worldState);
    }
    
    void Start()
    {
        mapGenerator.GenerateClearings();
        mapGenerator.GeneratePaths();
        clearingInfoGenerator.GenerateDenizens();
        clearingInfoGenerator.GenerateClearingNames();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !mouseBehaviour.IsDoingAction())
        {
            worldState.DeleteAllClearings();
            mapGenerator.GenerateClearings();
            mapGenerator.GeneratePaths();
            clearingInfoGenerator.GenerateDenizens();
            clearingInfoGenerator.GenerateClearingNames();

        }
    }
}
