using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditClearingPanel : MonoBehaviour
{
    private Clearing editingClearing;

    [SerializeField] private Button denizenControlButton;
    [SerializeField] private Button marquisateControlButton;
    [SerializeField] private Button eyrieControlButton;
    [SerializeField] private Button woodlandAllianceControlButton;
    [SerializeField] private Button lizardControlButton;
    [SerializeField] private Button riverfolkControlButton;
    [SerializeField] private Button duchyControlButton;
    [SerializeField] private Button corvidControlButton;

    [SerializeField] private GameObject controlSelector;
    
    [SerializeField] private Toggle marquisatePresenceToggle;
    [SerializeField] private Toggle eyriePresenceToggle;
    [SerializeField] private Toggle woodlandAlliancePresenceToggle;
    [SerializeField] private Toggle lizardPresenceToggle;
    [SerializeField] private Toggle riverfolkPresenceToggle;
    [SerializeField] private Toggle duchyPresenceToggle;
    [SerializeField] private Toggle corvidPresenceToggle;

    [SerializeField] private Toggle hasBuildingToggle;

    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private Button foxDenizenButton;
    [SerializeField] private Button mouseDenizenButton;
    [SerializeField] private Button rabbitDenizenButton;
    
    [SerializeField] private GameObject denizenSelector;

    public bool isEditingClearing { get; private set; }

    public void UpdateEditingClearing(Clearing clearing)
    {
        this.editingClearing = clearing;
    }
    
    public void OpenEditClearingPanel()
    {
        SetFactionControl(editingClearing.clearingControl);

        marquisatePresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.Marquisate);
        eyriePresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.EyrieDynasties);
        woodlandAlliancePresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.WoodlandAlliance);
        lizardPresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.LizardCult);
        riverfolkPresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.RiverfolkCompany);
        duchyPresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.GrandDuchy);
        corvidPresenceToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.CorvidConspiracy);

        hasBuildingToggle.isOn = editingClearing.hasBuilding;

        nameInputField.text = editingClearing.clearingName;
        
        SetDenizen(editingClearing.majorDenizen);

        isEditingClearing = true;
        
        gameObject.SetActive(true);
    }

    public void CloseEditClearingPanel()
    {
        isEditingClearing = false;
        
        gameObject.SetActive(false);
    }

    public void SetDenizenControl()
    {
        SetFactionControl(FactionType.Denizens);
    }

    public void SetMarquisateControl()
    {
        SetFactionControl(FactionType.Marquisate);
    }
    
    public void SetEyrieControl()
    {
        SetFactionControl(FactionType.EyrieDynasties);
    }
    
    public void SetWoodlandAllianceControl()
    {
        SetFactionControl(FactionType.WoodlandAlliance);
    }
    
    public void SetLizardControl()
    {
        SetFactionControl(FactionType.LizardCult);
    }
    
    public void SetRiverfolkControl()
    {
        SetFactionControl(FactionType.RiverfolkCompany);
    }
    
    public void SetDuchyControl()
    {
        SetFactionControl(FactionType.GrandDuchy);
    }
    
    public void SetCorvidControl()
    {
        SetFactionControl(FactionType.CorvidConspiracy);
    }

    public void ToggleMarquisatePresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.Marquisate, toggleState);
    }
    
    public void ToggleEyriePresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.EyrieDynasties, toggleState);
    }
    
    public void ToggleWoodlandAlliancePresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.WoodlandAlliance, toggleState);
    }
    
    public void ToggleLizardPresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.LizardCult, toggleState);
    }
    
    public void ToggleRiverfolkPresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.RiverfolkCompany, toggleState);
    }
    
    public void ToggleDuchyPresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.GrandDuchy, toggleState);
    }
    
    public void ToggleCorvidPresence(bool toggleState)
    {
        ToggleFactionPresence(FactionType.CorvidConspiracy, toggleState);
    }

    public void ToggleHasBuilding(bool toggleState)
    {
        editingClearing.SetHasBuilding(toggleState);
    }

    public void ChangeClearingName(string clearingName)
    {
        editingClearing.SetClearingName(clearingName);
    }

    public void SetFoxDenizen()
    {
        SetDenizen(DenizenType.Fox);
    }
    
    public void SetMouseDenizen()
    {
        SetDenizen(DenizenType.Mouse);
    }
    
    public void SetRabbitDenizen()
    {
        SetDenizen(DenizenType.Rabbit);
    }

    private void SetFactionControl(FactionType faction)
    {
        editingClearing.SetClearingControl(faction);
        
        Button controlButton = GetControlButton(faction);
        controlSelector.transform.localPosition = controlButton.transform.localPosition;
    }

    private void ToggleFactionPresence(FactionType faction, bool toggleState)
    {
        if (toggleState)
        {
            editingClearing.SetPresence(faction);
        }
        else
        {
            editingClearing.RemovePresence(faction);
        }
    }

    private void SetDenizen(DenizenType denizen)
    {
        editingClearing.SetMajorDenizen(denizen);

        Button denizenButton = GetDenizenButton(denizen);
        denizenSelector.transform.localPosition = denizenButton.transform.localPosition;
    }

    private Button GetControlButton(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Denizens:
                return denizenControlButton;
            case FactionType.Marquisate:
                return marquisateControlButton;
            case FactionType.EyrieDynasties:
                return eyrieControlButton;
            case FactionType.WoodlandAlliance:
                return woodlandAllianceControlButton;
            case FactionType.LizardCult:
                return lizardControlButton;
            case FactionType.RiverfolkCompany:
                return riverfolkControlButton;
            case FactionType.GrandDuchy:
                return duchyControlButton;
            case FactionType.CorvidConspiracy:
                return corvidControlButton;
            default:
                return marquisateControlButton;
        }
    }
    
    private Button GetDenizenButton(DenizenType denizen)
    {
        switch (denizen)
        {
            case DenizenType.Fox:
                return foxDenizenButton;
            case DenizenType.Mouse:
                return mouseDenizenButton;
            case DenizenType.Rabbit:
                return rabbitDenizenButton;
            default:
                return foxDenizenButton;
        }
    }
}
