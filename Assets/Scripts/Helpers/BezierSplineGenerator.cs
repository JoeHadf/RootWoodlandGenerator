using System.Collections.Generic;
using UnityEngine;


public static class BezierSplineHelper
{
    private const int splineSegmentSubdivisions = 13;

    public static List<Vector3> GetRiverSpline(List<Clearing> clearings)
    {
        if (clearings.Count >= 2)
        {
            Vector3[] clearingCentres = new Vector3[clearings.Count + 2];

            for (int i = 0; i < clearings.Count; i++)
            {
                clearingCentres[i + 1] = clearings[i].GetPosition();
            }

            clearingCentres[0] = clearings[0].GetClosestSide();
            clearingCentres[^1] = clearings[^1].GetClosestSide();

            BezierSplineSegment[] splineSegments = GetBezierSplineSegments(clearingCentres);

            List<Vector3> spline = new List<Vector3>(splineSegments.Length * splineSegmentSubdivisions + 1);

            for (int i = 0; i < splineSegments.Length; i++)
            {
                spline.AddRange(GetSubdividedSegment(splineSegments[i], i == splineSegments.Length - 1));
            }

            return spline;
        }

        return new List<Vector3>();
    }
    
    private static BezierSplineSegment[] GetBezierSplineSegments(Vector3[] joins)
    {
        Vector3[] secondPoints = new Vector3[joins.Length - 1];
        secondPoints[0] = Vector3.Lerp(joins[0], joins[1], 0.25f);

        BezierSplineSegment[] segments = new BezierSplineSegment[joins.Length - 1];
        for (int i = 0; i < joins.Length - 1; i++)
        {
            Vector3 firstPoint = joins[i];
            Vector3 lastPoint = joins[i + 1];

            Vector3 secondPoint = secondPoints[i];

            bool isLastPoint = i == joins.Length - 2;

            Vector3 thirdPoint;
            if (isLastPoint)
            {
                thirdPoint = Vector3.Lerp(firstPoint, lastPoint, 0.75f);
            }
            else
            {
                 Vector3 nextSecondPoint = Vector3.Lerp(joins[i+1], joins[i+2], 0.25f);
                 secondPoints[i + 1] = nextSecondPoint;
                 Vector3 nextSecondToLast = lastPoint - nextSecondPoint;
                 thirdPoint = lastPoint + nextSecondToLast;
            }

            BezierSplineSegment currentSegment =
                new BezierSplineSegment(firstPoint, secondPoint, thirdPoint, lastPoint);
            segments[i] = currentSegment;

        }

        return segments;
    }

    private static Vector3[] GetSubdividedSegment(BezierSplineSegment bezierSplineSegment, bool includeEndPoint)
    {
        int segmentLength = (includeEndPoint) ? splineSegmentSubdivisions + 1 : splineSegmentSubdivisions;
        Vector3[] subdividedSegment = new Vector3[segmentLength];
        float subdivisionIncrement = 1 / (float)(splineSegmentSubdivisions + 1);
        float currentT = 0;

        for (int i = 0; i < subdividedSegment.Length; i++)
        {
            subdividedSegment[i] = bezierSplineSegment.GetPoint(currentT);
            currentT += subdivisionIncrement;
        }

        return subdividedSegment;
    }

    private struct BezierSplineSegment
    {
        private Vector3 controlPoint0;
        private Vector3 controlPoint1;
        private Vector3 controlPoint2;
        private Vector3 controlPoint3;

        public BezierSplineSegment(Vector3 controlPoint0, Vector3 controlPoint1, Vector3 controlPoint2,
            Vector3 controlPoint3)
        {
            this.controlPoint0 = controlPoint0;
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            this.controlPoint3 = controlPoint3;
        }

        public Vector3 GetPoint(float t)
        {
            float oneMinusT = 1 - t;
            float oneMinusTSquared = oneMinusT * oneMinusT;
            float oneMinusTCubed = oneMinusTSquared * oneMinusT;
            float tSquared = t * t;
            float tCubed = tSquared * t;

            float coefficient0 = oneMinusTCubed;
            float coefficient1 = 3 * oneMinusTSquared * t;
            float coefficient2 = 3 * tSquared * oneMinusT;
            float coefficient3 = tCubed;

            return coefficient0 * controlPoint0 + 
                   coefficient1 * controlPoint1 + 
                   coefficient2 * controlPoint2 +
                   coefficient3 * controlPoint3;
        }
    }
}
