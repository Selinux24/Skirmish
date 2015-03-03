using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    public class BezierPath
    {
        private const int SEGMENTS_PER_CURVE = 10;
        private const float MINIMUM_SQR_DISTANCE = 0.01f;
        private const float DIVISION_THRESHOLD = -0.99f;

        private List<Vector3> controlPoints = new List<Vector3>();
        private int curveCount; //how many bezier curves in this path?

        public Vector3[] Points
        {
            get
            {
                return this.controlPoints.ToArray();
            }
            set
            {
                this.controlPoints.Clear();

                if (value != null && value.Length > 0)
                {
                    this.controlPoints.AddRange(value);
                    this.curveCount = (controlPoints.Count - 1) / 3;
                }
            }
        }

        public BezierPath()
        {

        }

        public void Interpolate(Vector3[] segmentPoints, float scale)
        {
            this.controlPoints.Clear();

            if (segmentPoints.Length < 2)
            {
                return;
            }

            for (int i = 0; i < segmentPoints.Length; i++)
            {
                if (i == 0) // is first
                {
                    Vector3 p1 = segmentPoints[i];
                    Vector3 p2 = segmentPoints[i + 1];

                    Vector3 tangent = (p2 - p1);
                    Vector3 q1 = p1 + scale * tangent;

                    this.controlPoints.Add(p1);
                    this.controlPoints.Add(q1);
                }
                else if (i == segmentPoints.Length - 1) //last
                {
                    Vector3 p0 = segmentPoints[i - 1];
                    Vector3 p1 = segmentPoints[i];
                    Vector3 tangent = (p1 - p0);
                    Vector3 q0 = p1 - scale * tangent;

                    this.controlPoints.Add(q0);
                    this.controlPoints.Add(p1);
                }
                else
                {
                    Vector3 p0 = segmentPoints[i - 1];
                    Vector3 p1 = segmentPoints[i];
                    Vector3 p2 = segmentPoints[i + 1];
                    Vector3 tangent = Vector3.Normalize(p2 - p0);
                    Vector3 q0 = p1 - scale * tangent * (p1 - p0).Length();
                    Vector3 q1 = p1 + scale * tangent * (p2 - p1).Length();

                    this.controlPoints.Add(q0);
                    this.controlPoints.Add(p1);
                    this.controlPoints.Add(q1);
                }
            }

            this.curveCount = (this.controlPoints.Count - 1) / 3;
        }
        public void SamplePoints(Vector3[] sourcePoints, float minSqrDistance, float maxSqrDistance, float scale)
        {
            if (sourcePoints.Length < 2)
            {
                return;
            }

            Stack<Vector3> samplePoints = new Stack<Vector3>();

            samplePoints.Push(sourcePoints[0]);

            Vector3 potentialSamplePoint = sourcePoints[1];

            int i = 2;

            for (i = 2; i < sourcePoints.Length; i++)
            {
                if (
                    ((potentialSamplePoint - sourcePoints[i]).LengthSquared() > minSqrDistance) &&
                    ((samplePoints.Peek() - sourcePoints[i]).LengthSquared() > maxSqrDistance))
                {
                    samplePoints.Push(potentialSamplePoint);
                }

                potentialSamplePoint = sourcePoints[i];
            }

            //now handle last bit of curve
            Vector3 p1 = samplePoints.Pop(); //last sample point
            Vector3 p0 = samplePoints.Peek(); //second last sample point
            Vector3 tangent = Vector3.Normalize(p0 - potentialSamplePoint);
            float d2 = (potentialSamplePoint - p1).Length();
            float d1 = (p1 - p0).Length();
            p1 = p1 + tangent * ((d1 - d2) / 2);

            samplePoints.Push(p1);
            samplePoints.Push(potentialSamplePoint);

            this.Interpolate(samplePoints.ToArray(), scale);
        }
        public Vector3 CalculateBezierPoint(int curveIndex, float t)
        {
            int nodeIndex = curveIndex * 3;

            Vector3 p0 = this.controlPoints[nodeIndex];
            Vector3 p1 = this.controlPoints[nodeIndex + 1];
            Vector3 p2 = this.controlPoints[nodeIndex + 2];
            Vector3 p3 = this.controlPoints[nodeIndex + 3];

            return this.CalculateBezierPoint(t, p0, p1, p2, p3);
        }
        public Vector3[] GetDrawingPoints0()
        {
            List<Vector3> drawingPoints = new List<Vector3>();

            for (int curveIndex = 0; curveIndex < this.curveCount; curveIndex++)
            {
                if (curveIndex == 0) 
                {
                    //Only do this for the first end point. 
                    //When i != 0, this coincides with the end point of the previous segment,
                    drawingPoints.Add(this.CalculateBezierPoint(curveIndex, 0));
                }

                for (int j = 1; j <= SEGMENTS_PER_CURVE; j++)
                {
                    float t = j / (float)SEGMENTS_PER_CURVE;
                    drawingPoints.Add(this.CalculateBezierPoint(curveIndex, t));
                }
            }

            return drawingPoints.ToArray();
        }
        public Vector3[] GetDrawingPoints1()
        {
            List<Vector3> drawingPoints = new List<Vector3>();

            for (int i = 0; i < this.controlPoints.Count - 3; i += 3)
            {
                Vector3 p0 = this.controlPoints[i];
                Vector3 p1 = this.controlPoints[i + 1];
                Vector3 p2 = this.controlPoints[i + 2];
                Vector3 p3 = this.controlPoints[i + 3];

                if (i == 0) //only do this for the first end point. When i != 0, this coincides with the end point of the previous segment,
                {
                    drawingPoints.Add(this.CalculateBezierPoint(0, p0, p1, p2, p3));
                }

                for (int j = 1; j <= SEGMENTS_PER_CURVE; j++)
                {
                    float t = j / (float)SEGMENTS_PER_CURVE;
                    drawingPoints.Add(this.CalculateBezierPoint(t, p0, p1, p2, p3));
                }
            }

            return drawingPoints.ToArray();
        }
        public Vector3[] GetDrawingPoints2()
        {
            List<Vector3> drawingPoints = new List<Vector3>();

            for (int curveIndex = 0; curveIndex < this.curveCount; curveIndex++)
            {
                List<Vector3> bezierCurveDrawingPoints = new List<Vector3>(this.FindDrawingPoints(curveIndex));

                if (curveIndex != 0)
                {
                    //remove the fist point, as it coincides with the last point of the previous Bezier curve.
                    bezierCurveDrawingPoints.RemoveAt(0);
                }

                drawingPoints.AddRange(bezierCurveDrawingPoints);
            }

            return drawingPoints.ToArray();
        }

        private Vector3[] FindDrawingPoints(int curveIndex)
        {
            List<Vector3> pointList = new List<Vector3>();

            Vector3 left = this.CalculateBezierPoint(curveIndex, 0);
            Vector3 right = this.CalculateBezierPoint(curveIndex, 1);

            pointList.Add(left);
            pointList.Add(right);

            this.FindDrawingPoints(curveIndex, 0, 1, pointList, 1);

            return pointList.ToArray();
        }
        private int FindDrawingPoints(int curveIndex, float t0, float t1, List<Vector3> pointList, int insertionIndex)
        {
            Vector3 left = CalculateBezierPoint(curveIndex, t0);
            Vector3 right = CalculateBezierPoint(curveIndex, t1);

            if ((left - right).LengthSquared() < MINIMUM_SQR_DISTANCE)
            {
                return 0;
            }

            float tMid = (t0 + t1) / 2;
            Vector3 mid = this.CalculateBezierPoint(curveIndex, tMid);

            Vector3 leftDirection = Vector3.Normalize(left - mid);
            Vector3 rightDirection = Vector3.Normalize(right - mid);

            if (Vector3.Dot(leftDirection, rightDirection) > DIVISION_THRESHOLD || Math.Abs(tMid - 0.5f) < 0.0001f)
            {
                int pointsAddedCount = 0;

                pointsAddedCount += this.FindDrawingPoints(curveIndex, t0, tMid, pointList, insertionIndex);
                pointList.Insert(insertionIndex + pointsAddedCount, mid);
                pointsAddedCount++;
                pointsAddedCount += this.FindDrawingPoints(curveIndex, tMid, t1, pointList, insertionIndex + pointsAddedCount);

                return pointsAddedCount;
            }

            return 0;
        }
        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0; //first term

            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term

            return p;
        }
    }
}