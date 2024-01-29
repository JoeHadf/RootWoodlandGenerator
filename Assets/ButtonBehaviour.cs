using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI editModeText;

    private WorldState worldState;

    private string modifyText = "Modify";
    private string createText = "Create";
    private string destroyText = "Destroy";

    public void Init(WorldState worldState)
    {
        this.worldState = worldState;
    }

    public void NextEditMode()
    {
        switch (WorldState.editMode)
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
