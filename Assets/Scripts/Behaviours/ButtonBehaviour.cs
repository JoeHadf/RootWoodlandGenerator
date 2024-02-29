using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject editModeButton;
    [SerializeField] private TextMeshProUGUI editModeText;
    [SerializeField] private GameObject changeNameButton;
    [SerializeField] private GameObject changeNameInputFieldObject;
    [SerializeField] private TMP_InputField changeNameInputField;
    [SerializeField] private GameObject endModifyModeButton;

    [SerializeField] private GameObject changeDenizenButton;
    [SerializeField] private GameObject denizenSelectorObject;
    [SerializeField] private DenizenSelector denizenSelector;

    [SerializeField] private GameObject changeFactionButton;
    [SerializeField] private GameObject factionMenuObject;
    [SerializeField] private FactionSelector factionSelector;
    [SerializeField] private Toggle hasBuildingToggle;
    [SerializeField] private Toggle hasSympathyToggle;

    [SerializeField] private GameObject fileScrollListObject;
    [SerializeField] private FileScrollList fileScrollList;

    [SerializeField] private GameObject fileSaveMenuObject;

    private WorldState worldState;
    
    public Clearing editingClearing { get; private set; }
    
    private string modifyText = "Modify";
    private string createText = "Create";
    private string destroyText = "Destroy";

    public bool changingName { get; private set; }
    public bool changingDenizen { get; private set; }
    public bool changingFaction { get; private set; }
    
    public bool loadingWoodland { get; private set; }
    
    public bool savingWoodland { get; private set; }

    private EditMode storedEditMode = EditMode.Modify;

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
        
        changingName = false;
        changingDenizen = false;
        changingFaction = false;
        loadingWoodland = false;
        savingWoodland = false;
    }

    private void Update()
    {
        if (changingName)
        {
            editingClearing.SetClearingName(changeNameInputField.text);
        }

        if (changingDenizen)
        {
            editingClearing.SetMajorDenizen(denizenSelector.selectedDenizen);
        }

        if (changingFaction)
        {
            editingClearing.SetClearingControl(factionSelector.selectedFaction);
        }
    }

    public bool IsDoingAction()
    {
        return changingName || changingDenizen || changingFaction || loadingWoodland || savingWoodland;
    }

    public void NextEditMode()
    {
        if (!IsDoingAction() && worldState.editMode != EditMode.EditRiver)
        {
            switch (worldState.editMode)
            {
                case EditMode.Modify:
                    ChangeEditMode(EditMode.Create);
                    break;
                case EditMode.Create:
                    ChangeEditMode(EditMode.Destroy);
                    break;
                case EditMode.Destroy:
                    ChangeEditMode(EditMode.Modify);
                    break;
            }
        }
    }

    public void OpenScrollList()
    {
        if (worldState.editMode != EditMode.EditRiver)
        {
            fileScrollListObject.SetActive(true);
            List<string> savedWoodlands = FileHelper.GetAllSavedWoodlands();
            fileScrollList.StartScrollList(savedWoodlands);

            loadingWoodland = true;
        }
    }

    public void CloseScrollList()
    {
        fileScrollListObject.SetActive(false);
        loadingWoodland = false;
    }

    public void OpenSaveMenu()
    {
        if (worldState.editMode != EditMode.EditRiver)
        {
            fileSaveMenuObject.SetActive(true);
            savingWoodland = true;
        }
    }
    
    public void CloseSaveMenu()
    {
        fileSaveMenuObject.SetActive(false);
        savingWoodland = false;
    }

    public void StartModifyingClearing(Clearing clearing)
    {
        Vector3 clearingWorldPosition = clearing.GetPosition();
        Vector3 changeNameButtonPosition = Camera.main.WorldToScreenPoint( clearingWorldPosition + new Vector3(1,0,0));
        Vector3 changeDenizenButtonPosition = Camera.main.WorldToScreenPoint(clearingWorldPosition + new Vector3(-1, -1, 0));
        Vector3 changeFactionButtonPosition = Camera.main.WorldToScreenPoint(clearingWorldPosition + new Vector3(-1, 1, 0));
        
        changeNameButton.transform.position = changeNameButtonPosition;
        changeNameButton.SetActive(true);

        changeDenizenButton.transform.position = changeDenizenButtonPosition;
        changeDenizenButton.SetActive(true);

        changeFactionButton.transform.position = changeFactionButtonPosition;
        changeFactionButton.SetActive(true);
        
        ChangeEditingClearing(clearing);
    }

    public void EndModifyingClearing()
    {
        changeNameButton.SetActive(false);
        changeDenizenButton.SetActive(false);
        changeFactionButton.SetActive(false);
    }

    public void StartChangingName()
    {
        changingName = true;
        changeNameInputFieldObject.SetActive(true);
        endModifyModeButton.SetActive(true);
        changeNameInputField.text = editingClearing.clearingName;
        
        EndModifyingClearing();
    }

    private void EndChangingName()
    {
        changingName = false;
        changeNameInputFieldObject.SetActive(false);
        endModifyModeButton.SetActive(false);
        changeNameInputField.text = "";
    }

    public void StartChangingDenizen()
    {
        changingDenizen = true;
        denizenSelectorObject.transform.position = Camera.main.WorldToScreenPoint(editingClearing.GetPosition() - new Vector3(0, 1.2f, 0));
        denizenSelectorObject.SetActive(true);
        endModifyModeButton.SetActive(true);
        denizenSelector.SelectDenizen(editingClearing.majorDenizen);
        
        EndModifyingClearing();
    }

    private void EndChangingDenizen()
    {
        changingDenizen = false;
        denizenSelectorObject.SetActive(false);
        endModifyModeButton.SetActive(false);
    }

    public void StartChangingFaction()
    {
        changingFaction = true;
        factionMenuObject.transform.position = Camera.main.WorldToScreenPoint(editingClearing.GetPosition() - new Vector3(0, 1.2f, 0));
        factionMenuObject.SetActive(true);
        endModifyModeButton.SetActive(true);
        factionSelector.SelectFaction(editingClearing.clearingControl);
        hasBuildingToggle.isOn = editingClearing.hasBuilding;
        hasSympathyToggle.isOn = editingClearing.GetHasFactionPresence(FactionType.WoodlandAlliance);
        EndModifyingClearing();
    }

    private void EndChangingFaction()
    {
        changingFaction = false;
        factionMenuObject.SetActive(false);
        endModifyModeButton.SetActive(false);
    }

    public void ToggleEditRiver()
    {
        if (worldState.editMode != EditMode.EditRiver)
        {
            worldState.river.ClearRiver();
            storedEditMode = worldState.editMode;
            worldState.ChangeEditMode(EditMode.EditRiver);
        }
        else
        {
            worldState.ChangeEditMode(storedEditMode);
        }
    }

    public void ToggleHasBuilding(bool toggleState)
    {
        editingClearing.SetHasBuilding(toggleState);
    }

    public void ToggleHasSympathy(bool toggleState)
    {
        if (toggleState)
        {
            editingClearing.SetPresence(FactionType.WoodlandAlliance);
        }
        else
        {
            editingClearing.RemovePresence(FactionType.WoodlandAlliance);
        }
    }
    
    private void ChangeEditingClearing(Clearing clearing)
    {
        this.editingClearing = clearing;
    }

    public void EndCurrentModifyMode()
    {
        EndChangingName();
        EndChangingDenizen();
        EndChangingFaction();
    }

    private void ChangeEditMode(EditMode newMode)
    {
        worldState.ChangeEditMode(newMode);
        editModeText.text = GetEditModeButtonText(newMode);
    }

    private string GetEditModeButtonText(EditMode editMode)
    {
        switch (editMode)
        {
            case EditMode.Modify:
                return modifyText;
            case EditMode.Create:
                return createText;
            case EditMode.Destroy:
                return destroyText;
            default:
                return "";
        }
    }
}
