using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RiverGenerator
{
    private WorldState worldState;
    
    private float xRange = 9.5f;
    private float yRange = 4.0f;

    private int pointCount = 5;
    
    private HashSet<int> riverClearingIDs = new HashSet<int>();

    public RiverGenerator(WorldState worldState)
    {
        this.worldState = worldState;
    }

    public void GenerateRiver()
    {
        riverClearingIDs.Clear();
        worldState.river.RegisterNewRiver(GetRiverClearings());
    }

    private List<Clearing> GetRiverClearings()
    {
        if (worldState.clearings.Count > 0)
        {
            Clearing startClearing = ChooseEdgeClearing();
            riverClearingIDs.Add(startClearing.clearingID);
            Clearing endClearing = ChooseEdgeClearing();
            riverClearingIDs.Add(endClearing.clearingID);
        
            List<Clearing> centreClearings = new List<Clearing>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                float xCoord = Random.Range(-xRange, xRange);
                float yCoord = Random.Range(-yRange, yRange);

                Vector3 point = new Vector3(xCoord, yCoord, 0);

                Clearing closestClearing = GetClearingClosestToPoint(point);
            
                if (!riverClearingIDs.Contains(closestClearing.clearingID))
                {
                    centreClearings.Add(closestClearing);
                    riverClearingIDs.Add(closestClearing.clearingID);
                }
            }

            List<Clearing> riverWithCentreClearings =
                AddCentreClearingsToRiver(startClearing, endClearing, centreClearings, out List<LineSegment> riverSegments);

            List<Clearing> riverWithOverlappingClearings = AddOverlappingClearings(riverWithCentreClearings, riverSegments);

            return riverWithOverlappingClearings;
        }
        else
        {
            return new List<Clearing>();
        }
    }

    private List<Clearing> AddOverlappingClearings(List<Clearing> riverWithCentreClearings, List<LineSegment> riverSegments)
    {
        List<Clearing> clearings = worldState.clearings;

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            
            if (!riverClearingIDs.Contains(currentClearing.clearingID))
            {
                for (int j = 0; j < riverSegments.Count; j++)
                {
                    float sqrDistanceToSegment = GetSqrDistanceToLineSegment(currentClearing.GetPosition(), riverSegments[j]);

                    if (sqrDistanceToSegment < GlobalConstants.clearingRadius * GlobalConstants.clearingRadius)
                    {
                        riverClearingIDs.Add(currentClearing.clearingID);
                        riverWithCentreClearings.Insert(j + 1, currentClearing);
                        
                        Vector3 beforePosition = riverWithCentreClearings[j].GetPosition();
                        Vector3 insertedPosition = riverWithCentreClearings[j + 1].GetPosition();
                        Vector3 afterPosition = riverWithCentreClearings[j + 2].GetPosition();
                        
                        riverSegments.RemoveAt(j);
                        riverSegments.Insert(j, new LineSegment(insertedPosition, afterPosition));
                        riverSegments.Insert(j, new LineSegment(beforePosition, insertedPosition));
                        break;
                    }
                }
            }
        }

        return riverWithCentreClearings;
    }

    private List<Clearing> AddCentreClearingsToRiver(Clearing startClearing, Clearing endClearing,
        List<Clearing> centreClearings, out List<LineSegment> riverSegments)
    {
        List<Clearing> riverWithCentreClearings = new List<Clearing>(centreClearings.Count + 2);
        riverWithCentreClearings.Add(startClearing);
        riverWithCentreClearings.Add(endClearing);
        
        List<LineSegment> lineSegments = new List<LineSegment>();
        lineSegments.Add(new LineSegment(startClearing.GetPosition(), endClearing.GetPosition()));

        for (int i = 0; i < centreClearings.Count; i++)
        {
            Clearing currentClearing = centreClearings[i];
            Vector3 clearingPosition = currentClearing.GetPosition();

            int closestLineSegmentIndex = 0;
            float minDistanceToLineSegment = float.MaxValue;

            for (int j = 0; j < lineSegments.Count; j++)
            {
                float distanceToLineSegment = GetSqrDistanceToLineSegment(clearingPosition, lineSegments[j]);

                if (distanceToLineSegment < minDistanceToLineSegment)
                {
                    closestLineSegmentIndex = j;
                    minDistanceToLineSegment = distanceToLineSegment;
                }
            }
            
            riverWithCentreClearings.Insert(closestLineSegmentIndex + 1, currentClearing);

            Vector3 beforePosition = riverWithCentreClearings[closestLineSegmentIndex].GetPosition();
            Vector3 insertedPosition = riverWithCentreClearings[closestLineSegmentIndex + 1].GetPosition();
            Vector3 afterPosition = riverWithCentreClearings[closestLineSegmentIndex + 2].GetPosition();

            lineSegments.RemoveAt(closestLineSegmentIndex);

            lineSegments.Insert(closestLineSegmentIndex, new LineSegment(insertedPosition, afterPosition));
            lineSegments.Insert(closestLineSegmentIndex, new LineSegment(beforePosition, insertedPosition));
        }

        riverSegments = lineSegments;
        return riverWithCentreClearings;
    }

    private float GetSqrDistanceToLineSegment(Vector3 point, LineSegment lineSegment)
    {
        Vector3 endMinusStart = lineSegment.lineEnd - lineSegment.lineStart;
        Vector3 startMinusPoint = lineSegment.lineStart - point;
        float t = -Vector3.Dot(endMinusStart, startMinusPoint) / Vector3.Dot(endMinusStart, endMinusStart);
        
        if (t is >= 0 and <= 1)
        {
            return Vector3.SqrMagnitude(lineSegment.GetPoint(t) - point);
        }
        else
        {
            float sqrDistanceToEnd = Vector3.SqrMagnitude(lineSegment.lineEnd - point);
            float sqrDistanceToStart = Vector3.SqrMagnitude(lineSegment.lineStart - point);

            return Math.Min(sqrDistanceToStart, sqrDistanceToEnd);
        }
    }

    private Clearing GetClearingClosestToPoint(Vector3 point)
    {
        List<Clearing> clearings = worldState.clearings;

        Clearing currentClosestClearing = clearings[0];
        float currentMinSqrDistance = float.MaxValue;

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            float sqrDistanceToPoint = Vector3.SqrMagnitude(point - currentClearing.GetPosition());

            if (sqrDistanceToPoint < currentMinSqrDistance)
            {
                currentClosestClearing = currentClearing;
                currentMinSqrDistance = sqrDistanceToPoint;
            }
        }

        return currentClosestClearing;
    }

    private Clearing ChooseEdgeClearing()
    {
        bool isXWall = Convert.ToBoolean(Random.Range(0,2));
        bool isPositive = Convert.ToBoolean(Random.Range(0,2));

        List<Clearing> clearings = worldState.clearings;

        Clearing currentClosestClearing = clearings[0];
        float currentDistance = float.MaxValue;

        for (int i = 0; i < clearings.Count; i++)
        {
            Clearing currentClearing = clearings[i];
            Vector3 clearingPosition = currentClearing.GetPosition();
            float wallTypeCoordinate = (isXWall) ? clearingPosition.x : clearingPosition.y;
            float wallPosition = (isPositive) ? xRange : -xRange;

            float distanceToWall = Math.Abs(wallPosition - wallTypeCoordinate);
            if (distanceToWall < currentDistance && !riverClearingIDs.Contains(currentClearing.clearingID))
            {
                currentClosestClearing = currentClearing;
                currentDistance = distanceToWall;
            }
        }

        return currentClosestClearing;
    }

    private struct LineSegment
    {
        public Vector3 lineStart { get; private set; }
        public Vector3 lineEnd { get; private set; }

        public LineSegment(Vector3 lineStart, Vector3 lineEnd)
        {
            this.lineStart = lineStart;
            this.lineEnd = lineEnd;
        }

        public Vector3 GetPoint(float t)
        {
            if (t is >= 0 and <= 1)
            {
                return lineStart + t * (lineEnd - lineStart);
            }
            else if (t < 0)
            {
                return lineStart;
            }
            else
            {
                return lineEnd;
            }
        }
    }
}
