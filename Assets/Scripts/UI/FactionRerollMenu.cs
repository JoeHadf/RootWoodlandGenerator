using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class FactionRerollMenu : MonoBehaviour
{

    private WorldState worldState;
    private FactionGenerator factionGenerator;
    
    [SerializeField] private Toggle marquisateToggle;
    [SerializeField] private Toggle eyrieToggle;
    [SerializeField] private Toggle woodlandAllianceToggle;
    [SerializeField] private Toggle lizardToggle;
    [SerializeField] private Toggle riverfolkToggle;
    [SerializeField] private Toggle duchyToggle;
    [SerializeField] private Toggle corvidToggle;

    public void Init(WorldState worldState, FactionGenerator factionGenerator)
    {
        this.worldState = worldState;
        this.factionGenerator = factionGenerator;

        this.worldState.OnEnterRerollFactionMenuState += OpenFactionRerollMenu;
        this.worldState.OnExitRerollFactionMenuState += CloseFactionRerollMenu;
    }

    public void EnterRerollFactionMenuState()
    { 
        worldState.TryEnterMenuState(MenuState.RerollFaction);
    }

    public void EnterDefaultMenuState()
    {
        worldState.TryEnterMenuState(MenuState.Default);
    }

    public void DoFactionReroll()
    {
        EnterDefaultMenuState();
        
        if (worldState.clearings.Count <= 0)
        {
            return;
        }
        
        factionGenerator.RemoveAllFactionInfo();
        
        if (marquisateToggle.isOn)
        {
            factionGenerator.SetupMarquisate();
        }
        
        if (eyrieToggle.isOn)
        {
            factionGenerator.SetupEyrieDynasties();
        }
        
        if (woodlandAllianceToggle.isOn)
        {
            factionGenerator.SetupWoodlandAlliance();
        }
        
        if (lizardToggle.isOn)
        {
            factionGenerator.SetupLizardCult();
        }
        
        if (riverfolkToggle.isOn)
        {
            factionGenerator.SetupRiverfolkCompany();
        }
        
        if (duchyToggle.isOn)
        {
            factionGenerator.SetupGrandDuchy();
        }
        
        if (corvidToggle.isOn)
        {
            factionGenerator.SetupCorvidConspiracy();
        }
        
        factionGenerator.SetupDenizens();
        factionGenerator.Reset();
    }

    private void OpenFactionRerollMenu()
    {
        marquisateToggle.isOn = false;
        eyrieToggle.isOn = false;
        woodlandAllianceToggle.isOn = false;
        lizardToggle.isOn = false;
        riverfolkToggle.isOn = false;
        duchyToggle.isOn = false;
        corvidToggle.isOn = false;

        gameObject.SetActive(true);
    }

    private void CloseFactionRerollMenu()
    {
        gameObject.SetActive(false);
    }
}
