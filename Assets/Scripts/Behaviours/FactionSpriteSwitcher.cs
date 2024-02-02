using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionSpriteSwitcher : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [SerializeField] private Sprite marquisateSprite;
    [SerializeField] private Sprite eyrieDynastiesSprite;
    [SerializeField] private Sprite woodlandAllianceSprite;

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
        }

        factionSprite = null;
        return false;
    }
}
