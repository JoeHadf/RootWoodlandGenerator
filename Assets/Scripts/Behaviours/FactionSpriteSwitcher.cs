using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionSpriteSwitcher : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [SerializeField] private Sprite marquisateSprite;
    [SerializeField] private Sprite eyrieDynastiesSprite;
    [SerializeField] private Sprite woodlandAllianceSprite;
    [SerializeField] private Sprite lizardCultSprite;
    [SerializeField] private Sprite riverfolkCompanySprite;
    [SerializeField] private Sprite grandDuchySprite;
    [SerializeField] private Sprite corvidConspiracySprite;

    public void SetFaction(FactionType faction)
    {
        if (TryGetFactionSprite(faction, out Sprite factionSprite))
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = factionSprite;
        }
        else
        {
            spriteRenderer.enabled = false;
        }
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
