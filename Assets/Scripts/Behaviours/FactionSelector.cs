using UnityEngine;

public class FactionSelector : MonoBehaviour
{
    public FactionType selectedFaction { get; private set; }

    void Awake()
    {
        selectedFaction = FactionType.Denizens;
    }

    public void SelectMarquisate()
    {
        SelectFaction(FactionType.Marquisate);
    }

    public void SelectEyrie()
    {
        SelectFaction(FactionType.EyrieDynasties);
    }

    public void SelectWoodlandAlliance()
    {
        SelectFaction(FactionType.WoodlandAlliance);
    }

    public void SelectFaction(FactionType faction)
    {
        selectedFaction = faction;
    }
}
