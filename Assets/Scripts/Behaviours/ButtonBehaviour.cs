using System;
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

    private WorldState worldState;
    
    public Clearing editingClearing { get; private set; }
    
    private string modifyText = "Modify";
    private string createText = "Create";
    private string destroyText = "Destroy";

    public bool changingName { get; private set; }
    public bool changingDenizen { get; private set; }

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
        changingName = false;
        changingDenizen = false;
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
    }

    public bool IsDoingAction()
    {
        return changingName || changingDenizen;
    }

    public void NextEditMode()
    {
        if (!IsDoingAction())
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

    public void StartModifyingClearing(Clearing clearing)
    {
        Vector3 clearingWorldPosition = clearing.GetPosition();
        Vector3 changeNameButtonPosition = Camera.main.WorldToScreenPoint( clearingWorldPosition + new Vector3(1,0,0));
        Vector3 changeDenizenButtonPosition = Camera.main.WorldToScreenPoint(clearingWorldPosition + new Vector3(-1, -1, 0));
        
        changeNameButton.transform.position = changeNameButtonPosition;
        changeNameButton.SetActive(true);

        changeDenizenButton.transform.position = changeDenizenButtonPosition;
        changeDenizenButton.SetActive(true);
        
        ChangeEditingClearing(clearing);
    }

    public void EndModifyingClearing()
    {
        changeNameButton.SetActive(false);
        changeDenizenButton.SetActive(false);
    }

    public void StartChangingName()
    {
        changingName = true;
        changeNameInputFieldObject.SetActive(true);
        endModifyModeButton.SetActive(true);
        changeNameInputField.text = editingClearing.clearingName;
        
        EndModifyingClearing();
    }

    public void EndChangingName()
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
    
    private void ChangeEditingClearing(Clearing clearing)
    {
        EndChangingName();
        this.editingClearing = clearing;
    }

    public void EndCurrentModifyMode()
    {
        EndChangingName();
        EndChangingDenizen();
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
