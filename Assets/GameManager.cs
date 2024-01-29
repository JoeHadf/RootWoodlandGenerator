using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearing;
    [SerializeField] private GameObject path;
    [SerializeField] private Material lineMaterial;

    [SerializeField] private ButtonBehaviour buttonBehaviour;

    private MapGenerator mapGenerator;
    private ClearingInfoGenerator clearingInfoGenerator;
    
    private float clearingRadius = 1.0f;
    private float lineWidth = 0.2f;

    private WorldState worldState;

    void Awake()
    {
        worldState = new WorldState();
        
        mapGenerator = new MapGenerator(worldState, clearing, path);
        clearingInfoGenerator = new ClearingInfoGenerator(worldState);
        buttonBehaviour.Init(worldState);
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            worldState.DeleteClearings();
            mapGenerator.GenerateClearings();
            mapGenerator.GeneratePaths();
            clearingInfoGenerator.GenerateDenizens();
            clearingInfoGenerator.GenerateClearingNames();

        }
    }
}
