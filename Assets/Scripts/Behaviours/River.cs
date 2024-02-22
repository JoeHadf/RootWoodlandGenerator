using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private List<Clearing> riverClearings = new List<Clearing>();

    public void RegisterNewRiver(List<Clearing> newRiver)
    {
        gameObject.SetActive(true);
        for (int i = 0; i < riverClearings.Count; i++)
        {
            riverClearings[i].OnClearingPositionChanged -= UpdateRiver;
        }

        riverClearings = newRiver;

        for (int i = 0; i < riverClearings.Count; i++)
        {
            riverClearings[i].OnClearingPositionChanged += UpdateRiver;
        }
        
        UpdateRiver();
    }

    public void RemoveClearingFromRiver(Clearing clearing)
    {
        for (int i = 0; i < riverClearings.Count; i++)
        {
            Clearing currentClearing = riverClearings[i];

            if (currentClearing.clearingID == clearing.clearingID)
            {
                currentClearing.OnClearingPositionChanged -= UpdateRiver;
                riverClearings.RemoveAt(i);
                UpdateRiver();
                break;
            }
        }
    }
    
    private void UpdateRiver()
    {
        List<Vector3> riverSpline = BezierSplineHelper.GetRiverSpline(riverClearings);

        lineRenderer.positionCount = riverSpline.Count;

        for (int i = 0; i < riverSpline.Count; i++)
        {
            lineRenderer.SetPosition(i, riverSpline[i]);
        }
    }

    public bool PathIsOnRiver(Clearing startClearing, Clearing endClearing)
    {
        for (int i = 0; i < riverClearings.Count; i++)
        {
            Clearing currentClearing = riverClearings[i];

            if (currentClearing.clearingID == startClearing.clearingID)
            {
                if (i > 0 && riverClearings[i - 1].clearingID == endClearing.clearingID)
                {
                    return true;
                }
                else if (i < riverClearings.Count - 1 && riverClearings[i + 1].clearingID == endClearing.clearingID)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
