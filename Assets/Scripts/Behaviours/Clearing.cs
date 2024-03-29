using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Clearing : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private MeshFilter meshFilter;
    
    [SerializeField] private SpriteRenderer denizenRenderer;
    [SerializeField] private FactionSpriteSwitcher controlSpriteSwitcher;
    [SerializeField] private FactionSpriteSwitcher buildingSpriteSwitcher;
    [SerializeField] private PresenceRenderer presenceRenderer;
    
    [SerializeField] private Sprite foxSprite;
    [SerializeField] private Sprite mouseSprite;
    [SerializeField] private Sprite rabbitSprite;

    public int clearingID {get; private set;}
    public string clearingName { get; private set; }
    public DenizenType majorDenizen { get; private set; }
    
    public FactionType clearingControl { get; private set; }
    public bool hasBuilding { get; private set; }
    
    private int circleSteps = 15;

    public delegate void PositionEventHandler();

    public event PositionEventHandler OnClearingPositionChanged;

    public Vector3 GetClosestSide()
    {
        Vector3 clearingPosition = transform.position;

        int xSign = Math.Sign(clearingPosition.x);
        int ySign = Math.Sign(clearingPosition.y);

        float xSideDistance = GlobalConstants.xRange - Math.Abs(clearingPosition.x);
        float ySideDistance = GlobalConstants.yRange - Math.Abs(clearingPosition.y);

        if (xSideDistance < ySideDistance)
        {
            return new Vector3(xSign * (GlobalConstants.xRange + 5), clearingPosition.y, 0);
        }
        
        return new Vector3(clearingPosition.x, ySign * (GlobalConstants.yRange + 5), 0);
    }

    public void Init(int id)
    {
        this.clearingID = id;
        SetClearingName("New Clearing");
        SetMajorDenizen(DenizenType.Fox);
        SetClearingControl(FactionType.Denizens);
        hasBuilding = false;
    }

    void Start()
    {
        DrawOutline();
    }
    
    public Vector3 GetPathStart(Vector3 normDirection)
    {
        return transform.localPosition + normDirection * GlobalConstants.clearingRadius;
    }

    public Vector3 GetPosition()
    {
        return transform.localPosition;
    }

    public bool GetHasFactionPresence(FactionType faction)
    {
        return presenceRenderer.FactionHasPresence(faction);
    }

    public FactionType[] GetPresentFactions()
    {
        return presenceRenderer.GetFactionsWithPresence();
    }

    public void SetPosition(Vector3 position)
    {
        float xBounded = Math.Clamp(position.x, -GlobalConstants.xRange, GlobalConstants.xRange);
        float yBounded = Math.Clamp(position.y, -GlobalConstants.yRange, GlobalConstants.yRange);
        transform.localPosition = new Vector3(xBounded, yBounded, 0);

        OnClearingPositionChanged?.Invoke();
    }

    private void DrawOutline()
    {
        lineRenderer.positionCount = circleSteps;
        lineRenderer.startWidth = GlobalConstants.clearingOutlineWidth;

        Vector3[] vertices = new Vector3[circleSteps + 1];
        int[] triangles = new int[3 * circleSteps];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < circleSteps; i++)
        {
            double currentAngle = (2 * Math.PI) * (i / (float)circleSteps);
            float x = (float)(GlobalConstants.clearingRadius * Math.Cos(currentAngle));
            float y = (float) (GlobalConstants.clearingRadius * Math.Sin(currentAngle));
                
            lineRenderer.SetPosition(i, new Vector3(x,y,0));
            vertices[i + 1] = new Vector3(x, y, 0);
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = (i + 1) % circleSteps + 1;
            triangles[3 * i + 2] = i + 1;
        }
        
        Mesh mesh = meshFilter.mesh;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
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
        
        SetHasBuilding(hasBuilding);
    }

    public void SetHasBuilding(bool building)
    {
        hasBuilding = building;
        buildingSpriteSwitcher.SetFaction(hasBuilding ? clearingControl : FactionType.Denizens);
    }

    public void SetPresence(FactionType faction)
    {
        presenceRenderer.AddFactionPresence(faction);
    }
    
    public void RemovePresence(FactionType faction)
    {
        presenceRenderer.RemoveFactionPresence(faction);
    }

    public void RemoveAllPresence()
    {
        presenceRenderer.RemoveAllFactionPresence();
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
