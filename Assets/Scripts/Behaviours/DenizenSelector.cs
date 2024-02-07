using UnityEngine;

public class DenizenSelector : MonoBehaviour
{
    public DenizenType selectedDenizen { get; private set; }

    void Awake()
    {
        selectedDenizen = DenizenType.Fox;
    }

    public void SelectFox()
    {
        SelectDenizen(DenizenType.Fox);
    }

    public void SelectMouse()
    {
        SelectDenizen(DenizenType.Mouse);
    }

    public void SelectRabbit()
    {
        SelectDenizen(DenizenType.Rabbit);
    }

    public void SelectDenizen(DenizenType denizen)
    {
        selectedDenizen = denizen;
    }
}
