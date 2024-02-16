using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearing;
    [SerializeField] private GameObject path;
    [SerializeField] private Material lineMaterial;

    [SerializeField] private ButtonBehaviour buttonBehaviour;
    [SerializeField] private MouseBehaviour mouseBehaviour;

    [SerializeField] private FileScrollList fileScrollList;

    private MapGenerator mapGenerator;
    private ClearingInfoGenerator clearingInfoGenerator;
    private FactionGenerator factionGenerator;
    private FileGenerator fileGenerator;

    private FileManager fileManager;

    private WorldState worldState;

    void Awake()
    {
        worldState = new WorldState(clearing, path);
        
        mapGenerator = new MapGenerator(worldState);
        clearingInfoGenerator = new ClearingInfoGenerator(worldState);
        factionGenerator = new FactionGenerator(worldState);
        fileGenerator = new FileGenerator(worldState);

        fileManager = new FileManager();
        
        buttonBehaviour.Init(worldState);
        mouseBehaviour.Init(worldState, buttonBehaviour);
        fileScrollList.Init(fileGenerator);
    }
    
    void Start()
    {
        List<string> savedWoodlands = fileManager.GetAllSavedWoodlands();
        fileScrollList.StartScrollList(savedWoodlands);
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
