using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject editModeButton;
    [SerializeField] private TextMeshProUGUI editModeText;
    [SerializeField] private GameObject changeNameButton;
    [SerializeField] private GameObject changeNameInputFieldObject;
    [SerializeField] private TMP_InputField changeNameInputField;
    [SerializeField] private GameObject endChangeNameButton;

    private WorldState worldState;
    
    public Clearing editingClearing { get; private set; }
    
    private string modifyText = "Modify";
    private string createText = "Create";
    private string destroyText = "Destroy";

    public bool changingName { get; private set; }

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
        changingName = false;
    }

    private void Update()
    {
        if (changingName)
        {
            editingClearing.SetClearingName(changeNameInputField.text);
        }
    }

    public void NextEditMode()
    {
        changeNameButton.SetActive(false);
        EndChangingName();
        
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

    public void StartChangingName()
    {
        changingName = true;
        changeNameInputFieldObject.SetActive(true);
        endChangeNameButton.SetActive(true);
        changeNameInputField.text = editingClearing.clearingName;
    }

    public void ChangeEditingClearing(Clearing clearing)
    {
        EndChangingName();
        this.editingClearing = clearing;
    }

    public void EndChangingName()
    {
        changingName = false;
        changeNameInputFieldObject.SetActive(false);
        endChangeNameButton.SetActive(false);
        changeNameInputField.text = "";
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
