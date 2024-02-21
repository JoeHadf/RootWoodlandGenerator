using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D edgeCollider;
    
    public PathID pathID { get; private set; }
    
    private Clearing startClearing;
    private Clearing endClearing;

    private bool isTemporary;

    public void Init(Clearing start, Clearing end)
    {
        isTemporary = false;
        
        this.startClearing = start;
        this.endClearing = end;
        this.pathID = new PathID(startClearing.clearingID, endClearing.clearingID);
        
        startClearing.RegisterPath(this);
        endClearing.RegisterPath(this);

        startClearing.OnClearingPositionChanged += UpdatePath;
        endClearing.OnClearingPositionChanged += UpdatePath;
        
        UpdatePath();
    }

    public void Init(Clearing start)
    {
        isTemporary = true;

        this.startClearing = start;
        this.pathID = new PathID(startClearing.clearingID, -1);
    }

    public void OnDestroy()
    {
        startClearing.OnClearingPositionChanged -= UpdatePath;
        if (!isTemporary)
        {
            endClearing.OnClearingPositionChanged -= UpdatePath;
        }
    }

    private void Start()
    {
        lineRenderer.startWidth = GlobalConstants.pathWidth;
        edgeCollider.edgeRadius = 0.5f * lineRenderer.startWidth;
    }

    void Update()
    {
        if (isTemporary)
        {
            UpdateTemporaryPath();
        }
    }

    public bool IsEndPoint(int clearingID)
    {
        return clearingID == pathID.startID || clearingID == pathID.endID;
    }

    private void UpdateCollider()
    {
        Vector2[] newPoints = new Vector2[2] { lineRenderer.GetPosition(0), lineRenderer.GetPosition(1) };
        edgeCollider.points = newPoints;
    }

    private void UpdatePath()
    {
        Vector3 direction = endClearing.transform.localPosition - startClearing.transform.localPosition;
        direction.Normalize();
        Vector3 start = startClearing.GetPathStart(direction);
        Vector3 end = endClearing.GetPathStart(-direction);
        
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        UpdateCollider();
    }
    
    private void UpdateTemporaryPath()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        
        Vector3 direction = mouseWorldPosition - startClearing.transform.localPosition;
        direction.Normalize();
        Vector3 start = startClearing.GetPathStart(direction);
        Vector3 end = mouseWorldPosition;
        
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        UpdateCollider();
    }

    public (Vector3 start, Vector3 end) GetEndPoints()
    {
        return (lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
    }

    public void DeregisterAdjacentClearings()
    {
        startClearing.DeregisterPath(this);
        endClearing.DeregisterPath(this);
    }
}
