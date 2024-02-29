using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PresenceRenderer : MonoBehaviour
{
    [SerializeField] private Sprite marquisateSprite;
    [SerializeField] private Sprite eyrieDynastiesSprite;
    [SerializeField] private Sprite woodlandAllianceSprite;
    [SerializeField] private Sprite lizardCultSprite;
    [SerializeField] private Sprite riverfolkCompanySprite;
    [SerializeField] private Sprite grandDuchySprite;
    [SerializeField] private Sprite corvidConspiracySprite;

    private const float presenceDistance = 10;
    private List<GameObject> presenceObjects = new List<GameObject>();
    private Dictionary<FactionType, int> factionToPresenceIndex = new Dictionary<FactionType, int>();

    public void AddFactionPresence(FactionType faction)
    {
        if (!factionToPresenceIndex.ContainsKey(faction) && TryCreatePresenceObject(faction, out GameObject presenceObject))
        {
            presenceObjects.Add(presenceObject);
            int presenceIndex = presenceObjects.Count -1;
            factionToPresenceIndex.Add(faction, presenceIndex);
            presenceObject.transform.localPosition = presenceIndex * presenceDistance * Vector3.right;
        }
    }

    public void RemoveFactionPresence(FactionType faction)
    {
        if (factionToPresenceIndex.TryGetValue(faction, out int presenceIndex))
        {
            Destroy(presenceObjects[presenceIndex]);
            presenceObjects.RemoveAt(presenceIndex);
            factionToPresenceIndex.Remove(faction);
            UpdateObjectPositions(presenceIndex);
            UpdatePresenceDictionary(presenceIndex);
        }
    }

    public void RemoveAllFactionPresence()
    {
        FactionType[] presentFactions = GetFactionsWithPresence();
        
        foreach (FactionType faction in presentFactions)
        {
            RemoveFactionPresence(faction);
        }
    }

    public bool FactionHasPresence(FactionType faction)
    {
        return factionToPresenceIndex.ContainsKey(faction);
    }

    public FactionType[] GetFactionsWithPresence()
    {
        return factionToPresenceIndex.Keys.ToArray();
    }

    private void UpdateObjectPositions(int indexToUpdateFrom = 0)
    {
        for (int i = indexToUpdateFrom; i < presenceObjects.Count; i++)
        {
            presenceObjects[i].transform.localPosition = i * presenceDistance * Vector3.right;
        }
    }
    
    private void UpdatePresenceDictionary(int indexToUpdateFrom)
    {
        FactionType[] presentFactions = GetFactionsWithPresence();
        for(int i = 0; i < presentFactions.Length; i++)
        {
            FactionType currentFaction = presentFactions[i];
            if (factionToPresenceIndex.TryGetValue(currentFaction, out int index))
            {
                if (index > indexToUpdateFrom)
                {
                    factionToPresenceIndex[currentFaction]--;
                }
            }
        }
    }

    private bool TryCreatePresenceObject(FactionType faction, out GameObject presenceObject)
    {
        presenceObject = null;
        bool hasCreatedObject = false;
        
        if (TryGetFactionString(faction, out string factionString) && TryGetFactionSprite(faction, out Sprite factionSprite))
        {
            presenceObject = new GameObject(factionString + "Presence");
            presenceObject.gameObject.transform.SetParent(transform, false);
            SpriteRenderer spriteRenderer = presenceObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = factionSprite;
            hasCreatedObject = true;
        }

        return hasCreatedObject;
    }
    
    private bool TryGetFactionString(FactionType faction, out string factionString)
    {
        switch (faction)
        {
            case FactionType.Marquisate:
                factionString = "Marquisate";
                return true;
            case FactionType.EyrieDynasties:
                factionString = "EyrieDynasties";
                return true;
            case FactionType.WoodlandAlliance:
                factionString = "WoodlandAlliance";
                return true;
            case FactionType.LizardCult:
                factionString = "LizardCult";
                return true;
            case FactionType.RiverfolkCompany:
                factionString = "RiverfolkCompany";
                return true;
            case FactionType.GrandDuchy:
                factionString = "GrandDuchy";
                return true;
            case FactionType.CorvidConspiracy:
                factionString = "CorvidConspiracy";
                return true;
        }
        
        factionString = "";
        return false;
    }
    
    private bool TryGetFactionSprite(FactionType faction, out Sprite factionSprite)
    {
        switch (faction)
        {
            case FactionType.Marquisate:
                factionSprite = marquisateSprite;
                return true;
            case FactionType.EyrieDynasties:
                factionSprite = eyrieDynastiesSprite;
                return true;
            case FactionType.WoodlandAlliance:
                factionSprite = woodlandAllianceSprite;
                return true;
            case FactionType.LizardCult:
                factionSprite = lizardCultSprite;
                return true;
            case FactionType.RiverfolkCompany:
                factionSprite = riverfolkCompanySprite;
                return true;
            case FactionType.GrandDuchy:
                factionSprite = grandDuchySprite;
                return true;
            case FactionType.CorvidConspiracy:
                factionSprite = corvidConspiracySprite;
                return true;
        }

        factionSprite = null;
        return false;
    }
}
