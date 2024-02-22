using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearing;
    [SerializeField] private GameObject path;

    [SerializeField] private ButtonBehaviour buttonBehaviour;
    [SerializeField] private MouseBehaviour mouseBehaviour;

    [SerializeField] private FileScrollList fileScrollList;
    [SerializeField] private FileSaveMenu fileSaveMenu;

    [SerializeField] private River river;

    private MapGenerator mapGenerator;
    private ClearingInfoGenerator clearingInfoGenerator;
    private RiverGenerator riverGenerator;
    private FactionGenerator factionGenerator;
    private FileGenerator fileGenerator;

    private WorldState worldState;

    void Awake()
    {
        worldState = new WorldState(clearing, path, river);
        
        mapGenerator = new MapGenerator(worldState);
        clearingInfoGenerator = new ClearingInfoGenerator(worldState);
        riverGenerator = new RiverGenerator(worldState);
        factionGenerator = new FactionGenerator(worldState);
        fileGenerator = new FileGenerator(worldState);
        
        buttonBehaviour.Init(worldState);
        mouseBehaviour.Init(worldState, buttonBehaviour);
        fileScrollList.Init(fileGenerator);
        fileSaveMenu.Init(fileGenerator);
    }
    
    void Start()
    {
        /*
        fileGenerator.ReadFileWithName("littleWoodland");

        List<Clearing> predefinedRiver = new List<Clearing>()
            { worldState.clearingsByID[2], worldState.clearingsByID[5], worldState.clearingsByID[4] };
        worldState.riverClearings = predefinedRiver;
        UpdateRiver();

        for (int i = 0; i < predefinedRiver.Count; i++)
        {
            predefinedRiver[i].OnClearingPositionChanged += UpdateRiver;
        }
        */
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !mouseBehaviour.IsDoingAction() && !buttonBehaviour.IsDoingAction())
        {
            worldState.DeleteAllClearings();
            mapGenerator.GenerateClearings();
            riverGenerator.GenerateRiver();
            mapGenerator.GeneratePaths();
            clearingInfoGenerator.GenerateDenizens();
            clearingInfoGenerator.GenerateClearingNames();
            
            factionGenerator.SetupMarquisate();
            factionGenerator.SetupEyrieDynasties();
            factionGenerator.SetupWoodlandAlliance();
            factionGenerator.SetupDenizens();
            factionGenerator.Reset();
        }
    }
}
