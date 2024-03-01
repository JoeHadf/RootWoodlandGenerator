using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearing;
    [SerializeField] private GameObject path;

    [SerializeField] private SaveLoadPanel saveLoadPanel;
    [SerializeField] private EditClearingPanel editClearingPanel;
    [SerializeField] private MouseBehaviour mouseBehaviour;

    [SerializeField] private EditModePanel editModePanel;
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
        
        saveLoadPanel.Init(worldState);
        mouseBehaviour.Init(worldState, editClearingPanel);
        editModePanel.Init(worldState);
        fileScrollList.Init(fileGenerator);
        fileSaveMenu.Init(fileGenerator);
        rerollPanel.Init(worldState, mapGenerator, clearingInfoGenerator, riverGenerator);
        factionRerollMenu.Init(worldState, factionGenerator);
        editClearingPanel.Init(worldState);

        float yRange = Camera.main.orthographicSize;
        float xRange = yRange * ((float)Screen.width / Screen.height);

        GlobalConstants.SetScreenRange(xRange, yRange);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (worldState.menuState == MenuState.Escape)
            {
                worldState.TryEnterMenuState(MenuState.Default);
            }
            else if (worldState.menuState == MenuState.Default)
            {
                worldState.TryEnterMenuState(MenuState.Escape);
            }
        }
    }
}
