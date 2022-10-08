using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Path of bezier curves
    /// </summary>
    public class BezierPath : IControllerPath
    {
        /// <summary>
        /// Gets total segment's distance represented by a point list
        /// </summary>
        /// <param name="points">Point list</param>
        /// <returns>Returns total segment's distance represented by a point list</returns>
        public static float SegmentDistance(params Vector3[] points)
        {
            float length = 0;

            Vector3 p0 = points[0];

            for (int i = 1; i < points.Length; i++)
            {
                Vector3 p1 = points[i];

                length += Vector3.Distance(p0, p1);

                p0 = p1;
            }

            return length;
        }

        /// <summary>
        /// Initial control points
        /// </summary>
        private readonly List<Vector3> initialControlPoints = new List<Vector3>();
        /// <summary>
        /// Initial minimum distance squared
        /// </summary>
        private float? initialMinSqrDistance = null;
        /// <summary>
        /// Initial maximum distance squared
        /// </summary>
        private float? initialMaxSqrDistance = null;
        /// <summary>
        /// Initial scale
        /// </summary>
        private float initialScale = 1f;
        /// <summary>
        /// Control points
        /// </summary>
        private readonly List<Vector3> controlPoints = new List<Vector3>();
        /// <summary>
        /// Curve times dictionary
        /// </summary>
        private readonly Dictionary<int, float> curveTimes = new Dictionary<int, float>();

        /// <summary>
        /// Control points
        /// </summary>
        public Vector3[] Points
        {
            get
            {
                List<Vector3> points = new List<Vector3>();

                if (this.PositionCount > 0)
                {
                    points.Add(this.controlPoints[0]);

                    for (int i = 0; i < this.PositionCount; i++)
                    {
                        int index = (i * 3) + 3;

                        points.Add(this.controlPoints[index]);
                    }
                }

                return points.ToArray();
            }
        }
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 First
        {
            get
            {
                return this.Points[0];
            }
        }
        /// <summary>
        /// Last point
        /// </summary>
        public Vector3 Last
        {
            get
            {
                return this.Points[this.Points.Length - 1];
            }
        }
        /// <summary>
        /// Number of segments in the path
        /// </summary>
        public int PositionCount { get; private set; }
        /// <summary>
        /// Number of normals in the path
        /// </summary>
        public int NormalCount
        {
            get
            {
                return 0;
            }
        }
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
            this.initialControlPoints.Clear();
            this.initialMinSqrDistance = null;
            this.initialMaxSqrDistance = null;
            this.initialScale = scale;

            if (points.Length > 0)
            {
                this.initialControlPoints.AddRange(points);
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
            p1 += tangent * ((d1 - d2) / 2);

            samplePoints.Push(p1);
            samplePoints.Push(potentialSamplePoint);

            Vector3[] sampledPoints = samplePoints.ToArray();

            Array.Reverse(sampledPoints);

            this.SetControlPoints(sampledPoints, scale);

            this.initialMinSqrDistance = minSqrDistance;
            this.initialMaxSqrDistance = maxSqrDistance;
        }
        /// <summary>
        /// Updates curve internal information
        /// </summary>
        private void UpdateCurveInfo()
        {
            this.PositionCount = (this.controlPoints.Count - 1) / 3;
            this.curveTimes.Clear();
            this.Length = 0;

            List<Vector3> curvePoint = new List<Vector3>();

            for (int i = 0; i < this.PositionCount; i++)
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

                float length = SegmentDistance(curvePoint.ToArray());

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
            this.initialControlPoints.Add(point);

            if (this.initialMinSqrDistance.HasValue && this.initialMaxSqrDistance.HasValue)
            {
                this.SetControlPoints(
                    this.initialControlPoints.ToArray(),
                    this.initialMinSqrDistance.Value,
                    this.initialMaxSqrDistance.Value,
                    this.initialScale);
            }
            else
            {
                this.SetControlPoints(this.initialControlPoints.ToArray(), this.initialScale);
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

                for (int i = 0; i < this.PositionCount; i++)
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
            this.FindCurve(time, out int segment, out float segmentTime);

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
            this.FindCurve(time, out int segment, out _);

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
        /// <summary>
        /// Samples current path in a vector array
        /// </summary>
        /// <param name="sampleTime">Time delta</param>
        /// <returns>Returns a vector array</returns>
        public IEnumerable<Vector3> SamplePath(float sampleTime)
        {
            List<Vector3> returnPath = new List<Vector3>();

            float time = 0;
            while (time < this.Length)
            {
                returnPath.Add(this.GetPosition(time));

                time += sampleTime;
            }

            return returnPath.ToArray();
        }
    }
}