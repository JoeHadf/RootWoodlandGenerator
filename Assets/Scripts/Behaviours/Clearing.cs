using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Clearing : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private SpriteRenderer denizenRenderer;
    [SerializeField] private SpriteRenderer presenceRenderer;
    [SerializeField] private FactionSpriteSwitcher controlSpriteSwitcher;
    [SerializeField] private FactionSpriteSwitcher buildingSpriteSwitcher;

    [SerializeField] private Sprite sympathySprite;
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite mouseSprite;
    [SerializeField] private Sprite rabbitSprite;

    public int clearingID {get; private set;}
    public string clearingName { get; private set; }
    public DenizenType majorDenizen { get; private set; }
    
    public FactionType clearingControl { get; private set; }
    public bool hasBuilding { get; private set; }
    public HashSet<FactionType> clearingPresence { get; private set; }

    private List<Path> paths = new List<Path>();
    
    private float clearingRadius = 1.0f;
    private int circleSteps = 15;
    private float lineWidth = 0.2f;
    
    private float xRange = 9.5f;
    private float yRange = 4.0f;

    public void Init(int id)
    {
        this.clearingID = id;
        SetClearingName(id.ToString());
        SetMajorDenizen(DenizenType.Fox);
        SetClearingControl(FactionType.Denizens);
        clearingPresence = new HashSet<FactionType>();
    }

    void Start()
    {
        DrawCircle();
    }
    
    public Vector3 GetPathStart(Vector3 normDirection)
    {
        return transform.localPosition + normDirection * clearingRadius;
    }

    public void RegisterPath(Path adjacentPath)
    {
        paths.Add(adjacentPath);
    }

    public void DeregisterPath(Path nonAdjacentPath)
    {
        paths.Remove(nonAdjacentPath);
    }

    public void DeregisterAllPaths()
    {
        paths.Clear();
    }

    public Vector3 GetPosition()
    {
        return transform.localPosition;
    }

    public void SetPosition(Vector3 position)
    {
        float xBounded = Math.Clamp(position.x, -xRange, xRange);
        float yBounded = Math.Clamp(position.y, -yRange, yRange);
        transform.localPosition = new Vector3(xBounded, yBounded, 0);
    }

    private void DrawCircle()
    {
        lineRenderer.positionCount = circleSteps;
        lineRenderer.startWidth = lineWidth;

        for (int j = 0; j < circleSteps; j++)
        {
            double currentAngle = (2 * Math.PI) * (j / (float)circleSteps);
            float x = (float) (clearingRadius * Math.Cos(currentAngle));
            float y = (float) (clearingRadius * Math.Sin(currentAngle));
                
            lineRenderer.SetPosition(j, new Vector3(x,y,0));
        }
    }

    public void SetClearingName(string name)
    {
        this.clearingName = name;
        textMesh.text = name;
    }

    public void SetMajorDenizen(DenizenType denizen)
    {
        this.majorDenizen = denizen;
        denizenRenderer.sprite = GetDenizenSprite(denizen);
    }

    public void SetClearingControl(FactionType faction)
    {
        this.clearingControl = faction;
        controlSpriteSwitcher.SetFaction(faction);
    }

    public void SetHasBuilding(bool building)
    {
        hasBuilding = building;
        buildingSpriteSwitcher.SetFaction(hasBuilding ? clearingControl : FactionType.Denizens);
    }

    public void SetClearingPresence(FactionType faction)
    {
        clearingPresence.Add(faction);
        if (faction == FactionType.WoodlandAlliance)
        {
            presenceRenderer.enabled = true;
            presenceRenderer.sprite = sympathySprite;
        }
    }

    public void RemoveClearingPresence(FactionType faction)
    {
        clearingPresence.Remove(faction);
        if (faction == FactionType.WoodlandAlliance)
        {
            presenceRenderer.enabled = false;
        }
    }

    public void RemoveAllPresence()
    {
        clearingPresence.Clear();
        presenceRenderer.enabled = false;
    }

    private Sprite GetDenizenSprite(DenizenType denizen)
    {
        switch (denizen)
        {
            case DenizenType.Fox:
                return foxSprite;
            case DenizenType.Mouse:
                return mouseSprite;
            case DenizenType.Rabbit:
                return rabbitSprite;
        }

        return foxSprite;
    }
}

public enum DenizenType
{
    Fox = 0,
    Mouse = 1,
    Rabbit = 2,
    Bird = 3
}
