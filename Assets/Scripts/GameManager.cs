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

    [SerializeField] private LineRenderer river;

    private MapGenerator mapGenerator;
    private ClearingInfoGenerator clearingInfoGenerator;
    private FactionGenerator factionGenerator;
    private FileGenerator fileGenerator;
    private BezierSplineGenerator bezierSplineGenerator;

    private WorldState worldState;

    void Awake()
    {
        worldState = new WorldState(clearing, path);
        
        mapGenerator = new MapGenerator(worldState);
        clearingInfoGenerator = new ClearingInfoGenerator(worldState);
        factionGenerator = new FactionGenerator(worldState);
        fileGenerator = new FileGenerator(worldState);
        bezierSplineGenerator = new BezierSplineGenerator();
        
        buttonBehaviour.Init(worldState);
        mouseBehaviour.Init(worldState, buttonBehaviour);
        fileScrollList.Init(fileGenerator);
        fileSaveMenu.Init(fileGenerator);
    }
    
    void Start()
    {
        fileGenerator.ReadFileWithName("littleWoodland");

        List<Clearing> predefinedRiver = new List<Clearing>()
            { worldState.clearingsByID[2], worldState.clearingsByID[5], worldState.clearingsByID[4] };
        worldState.riverClearings = predefinedRiver;
        UpdateRiver();

        for (int i = 0; i < predefinedRiver.Count; i++)
        {
            predefinedRiver[i].OnClearingPositionChanged += UpdateRiver;
        }
    }

    void UpdateRiver()
    {
        List<Vector3> riverSpline = bezierSplineGenerator.GetRiverSpline(worldState.riverClearings);
        river.positionCount = riverSpline.Count;

        for (int i = 0; i < riverSpline.Count; i++)
        {
            river.SetPosition(i, riverSpline[i]);
        }
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
    }
}
