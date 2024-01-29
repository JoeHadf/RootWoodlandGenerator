using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    public PathID pathID { get; private set; }
    
    private Clearing startClearing;
    private Clearing endClearing;

    public void Init(Clearing start, Clearing end)
    {
        this.startClearing = start;
        this.endClearing = end;
        this.pathID = new PathID(startClearing.clearingID, endClearing.clearingID);
        
        startClearing.RegisterPath(this);
        endClearing.RegisterPath(this);
        
        UpdatePath();
    }
    
    void Update()
    {
        UpdatePath();
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
    }

    public (Vector3 start, Vector3 end) GetEndPoints()
    {
        return (lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
    }
}
