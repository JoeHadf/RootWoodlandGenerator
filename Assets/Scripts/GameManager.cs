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
    [SerializeField] private RerollPanel rerollPanel;
    [SerializeField] private FactionRerollMenu factionRerollMenu;

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
        rerollPanel.Init(worldState, mapGenerator, clearingInfoGenerator, riverGenerator);
        factionRerollMenu.Init(worldState, factionGenerator);
    }
    
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !mouseBehaviour.IsDoingAction() && !buttonBehaviour.IsDoingAction())
        {
            factionGenerator.SetupMarquisate();
            factionGenerator.SetupEyrieDynasties();
            factionGenerator.SetupWoodlandAlliance();
            factionGenerator.SetupLizardCult();
            factionGenerator.SetupRiverfolkCompany();
            factionGenerator.SetupGrandDuchy();
            factionGenerator.SetupCorvidConspiracy();
            factionGenerator.SetupDenizens();
            factionGenerator.Reset();
        }
    }
}
