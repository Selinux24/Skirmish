using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Path of bezier curves
    /// </summary>
    public class BezierPath : IControllerPath
    {
        private List<Vector3> originalControlPoints = new List<Vector3>();
        private float? originalMinSqrDistance = null;
        private float? originalMaxSqrDistance = null;
        private float originalScale = 1f;

        /// <summary>
        /// Control points
        /// </summary>
        private List<Vector3> controlPoints = new List<Vector3>();
        /// <summary>
        /// Curve times dictionary
        /// </summary>
        private Dictionary<int, float> curveTimes = new Dictionary<int, float>();

        /// <summary>
        /// Control points
        /// </summary>
        public Vector3[] Points
        {
            get
            {
                List<Vector3> points = new List<Vector3>();

                if (this.Count > 0)
                {
                    points.Add(this.controlPoints[0]);

                    for (int i = 0; i < this.Count; i++)
                    {
                        int index = (i * 3) + 3;

                        points.Add(this.controlPoints[index]);
                    }
                }

                return points.ToArray();
            }
        }
        /// <summary>
        /// Number of segments in the path
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Total length of path
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BezierPath()
        {

        }

        /// <summary>
        /// Sets control points for the path
        /// </summary>
        /// <param name="points">Points</param>
        /// <remarks>
        /// Curves were defined in four by four components
        /// </remarks>
        public void SetControlPoints(Vector3[] points)
        {
            this.SetControlPoints(points, 1f);
        }
        /// <summary>
        /// Sets control points
        /// </summary>
        /// <param name="points">Points</param>
        /// <param name="scale">Scale of points</param>
        /// <remarks>
        /// Calculates a Bezier interpolated path for the given points and scale
        /// </remarks>
        public void SetControlPoints(Vector3[] points, float scale)
        {
            this.originalControlPoints.Clear();
            this.originalMinSqrDistance = null;
            this.originalMaxSqrDistance = null;
            this.originalScale = scale;

            if (points.Length > 0)
            {
                this.originalControlPoints.AddRange(points);
            }

            this.controlPoints.Clear();

            if (points.Length < 2)
            {
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                {
                    // is first
                    Vector3 p1 = points[i];
                    Vector3 p2 = points[i + 1];

                    Vector3 tangent = (p2 - p1);
                    Vector3 q1 = p1 + scale * tangent;

                    this.controlPoints.Add(p1);
                    this.controlPoints.Add(q1);
                }
                else if (i == points.Length - 1)
                {
                    // last
                    Vector3 p0 = points[i - 1];
                    Vector3 p1 = points[i];
                    Vector3 tangent = (p1 - p0);
                    Vector3 q0 = p1 - scale * tangent;

                    this.controlPoints.Add(q0);
                    this.controlPoints.Add(p1);
                }
                else
                {
                    Vector3 p0 = points[i - 1];
                    Vector3 p1 = points[i];
                    Vector3 p2 = points[i + 1];
                    Vector3 tangent = Vector3.Normalize(p2 - p0);
                    Vector3 q0 = p1 - scale * tangent * (p1 - p0).Length();
                    Vector3 q1 = p1 + scale * tangent * (p2 - p1).Length();

                    this.controlPoints.Add(q0);
                    this.controlPoints.Add(p1);
                    this.controlPoints.Add(q1);
                }
            }

            this.UpdateCurveInfo();
        }
        /// <summary>
        /// Sets control points
        /// </summary>
        /// <param name="points">Points</param>
        /// <param name="minSqrDistance">Minimum squared distances between sampled points</param>
        /// <param name="maxSqrDistance">Maximum squared distances between sampled points</param>
        /// <param name="scale">Scale</param>
        /// <remarks>
        /// Sample the given points as a Bezier path
        /// </remarks>
        public void SetControlPoints(Vector3[] points, float minSqrDistance, float maxSqrDistance, float scale)
        {
            if (points.Length < 2)
            {
                return;
            }

            Stack<Vector3> samplePoints = new Stack<Vector3>();

            samplePoints.Push(points[0]);

            Vector3 potentialSamplePoint = points[1];

            for (int i = 2; i < points.Length; i++)
            {
                if (((potentialSamplePoint - points[i]).LengthSquared() > minSqrDistance) &&
                    ((samplePoints.Peek() - points[i]).LengthSquared() > maxSqrDistance))
                {
                    samplePoints.Push(potentialSamplePoint);
                }

                potentialSamplePoint = points[i];
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

            Vector3[] sampledPoints = samplePoints.ToArray();

            Array.Reverse(sampledPoints);

            this.SetControlPoints(sampledPoints, scale);

            this.originalMinSqrDistance = minSqrDistance;
            this.originalMaxSqrDistance = maxSqrDistance;
        }
        /// <summary>
        /// Updates curve internal information
        /// </summary>
        private void UpdateCurveInfo()
        {
            this.Count = (this.controlPoints.Count - 1) / 3;
            this.curveTimes.Clear();
            this.Length = 0;

            List<Vector3> curvePoint = new List<Vector3>();

            for (int i = 0; i < this.Count; i++)
            {
                curvePoint.Clear();

                if (i == 0)
                {
                    //Only do this for the first end point. 
                    //When i != 0, this coincides with the end point of the previous segment,
                    curvePoint.Add(this.CalculateBezierPoint(i, 0));
                }

                for (int j = 1; j <= 50; j++)
                {
                    float t = (float)j / (float)50;

                    curvePoint.Add(this.CalculateBezierPoint(i, t));
                }

                float length = Helper.Distance(curvePoint.ToArray());

                this.Length += length;

                this.curveTimes.Add(i, length);
            }
        }

        /// <summary>
        /// Adds new control point to curve
        /// </summary>
        /// <param name="point">Point</param>
        public void AddPoint(Vector3 point)
        {
            this.originalControlPoints.Add(point);

            if (this.originalMinSqrDistance.HasValue && this.originalMaxSqrDistance.HasValue)
            {
                this.SetControlPoints(
                    this.originalControlPoints.ToArray(), 
                    this.originalMinSqrDistance.Value,
                    this.originalMaxSqrDistance.Value,
                    this.originalScale);
            }
            else
            {
                this.SetControlPoints(this.originalControlPoints.ToArray(), this.originalScale);
            }
        }

        /// <summary>
        /// Finds segment 
        /// </summary>
        /// <param name="time">Global time value</param>
        /// <param name="curve">Curve index</param>
        /// <param name="segmentTime">Relative time value for curve</param>
        public void FindCurve(float time, out int curve, out float segmentTime)
        {
            curve = -1;
            segmentTime = 0;

            if (time == 0)
            {
                curve = 0;
                segmentTime = 0;
            }
            else
            {
                float distance = 0;

                for (int i = 0; i < this.Count; i++)
                {
                    distance += this.curveTimes[i];

                    if (time <= distance)
                    {
                        curve = i;
                        segmentTime = 1f - ((distance - time) / this.curveTimes[i]);

                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Gets curve position in specified time
        /// </summary>
        /// <param name="time">Global time value</param>
        /// <returns>Returns curve position in specified time</returns>
        public Vector3 GetPosition(float time)
        {
            int segment;
            float segmentTime;
            this.FindCurve(time, out segment, out segmentTime);

            return CalculateBezierPoint(segment, segmentTime);
        }
        /// <summary>
        /// Gets path normal in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns path normal</returns>
        public Vector3 GetNormal(float time)
        {
            return Vector3.Up;
        }
        /// <summary>
        /// Gets the next control point in the specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns the next control point in time</returns>
        public Vector3 GetNextControlPoint(float time)
        {
            int segment;
            float segmentTime;
            this.FindCurve(time, out segment, out segmentTime);

            return this.controlPoints[segment];
        }
        /// <summary>
        /// Calculates a point on the path.
        /// </summary>
        /// <param name="curve">The index of the curve that the point is on</param>
        /// <param name="t">Relative time value for curve</param>
        /// <returns>Returns curve position in specified time</returns>
        private Vector3 CalculateBezierPoint(int curve, float t)
        {
            int nodeIndex = curve * 3;

            Vector3 p0 = this.controlPoints[nodeIndex + 0];
            Vector3 p1 = this.controlPoints[nodeIndex + 1];
            Vector3 p2 = this.controlPoints[nodeIndex + 2];
            Vector3 p3 = this.controlPoints[nodeIndex + 3];

            return this.CalculateBezierPoint(p0, p1, p2, p3, t);
        }
        /// <summary>
        /// Calculates a point on the path.
        /// </summary>
        /// <param name="p0">Control point 0</param>
        /// <param name="p1">Control point 1</param>
        /// <param name="p2">Control point 2</param>
        /// <param name="p3">Control point 3</param>
        /// <param name="t">Relative time value for curve</param>
        /// <returns>Returns curve position in specified time</returns>
        private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            float uu = u * u;
            float uuu = uu * u;

            float tt = t * t;
            float ttt = tt * t;

            Vector3 p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term

            return p;
        }
    }
}