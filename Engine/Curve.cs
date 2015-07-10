using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 3D curve
    /// </summary>
    public class Curve : IPath
    {
        /// <summary>
        /// Control points
        /// </summary>
        private List<Tuple<float, Vector3>> controlPoints = new List<Tuple<float, Vector3>>();
        /// <summary>
        /// Start point
        /// </summary>
        private Vector3 startPoint
        {
            get
            {
                if (this.controlPoints.Count >= 2)
                {
                    Vector3 v = this.controlPoints[0].Item2 - this.controlPoints[1].Item2;

                    return this.controlPoints[0].Item2 + v;
                }

                return Vector3.Zero;
            }
        }
        /// <summary>
        /// End point
        /// </summary>
        private Vector3 endPoint
        {
            get
            {
                if (this.controlPoints.Count >= 2)
                {
                    Vector3 v = this.controlPoints[this.controlPoints.Count - 1].Item2 - this.controlPoints[this.controlPoints.Count - 2].Item2;

                    return this.controlPoints[this.controlPoints.Count - 1].Item2 + v;
                }

                return Vector3.Zero;
            }
        }

        /// <summary>
        /// Point list
        /// </summary>
        public Vector3[] Points
        {
            get
            {
                List<Vector3> points = new List<Vector3>();

                if (this.controlPoints.Count > 0)
                {
                    this.controlPoints.ForEach((c) => { points.Add(c.Item2); });

                }

                return points.ToArray();
            }
            set
            {
                this.controlPoints.Clear();

                if (value != null && value.Length > 0)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        this.AddPosition(value[i]);
                    }
                }
            }
        }
        /// <summary>
        /// Total length
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Curve()
        {
            this.Length = 0;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Point list</param>
        public Curve(Vector3[] points)
        {
            if (points != null && points.Length > 0)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    this.AddPosition(points[i]);
                }
            }
        }

        /// <summary>
        /// Add control point
        /// </summary>
        /// <param name="point">Point</param>
        public void AddPosition(Vector3 point)
        {
            if (this.controlPoints.Count > 0)
            {
                Tuple<float, Vector3> p = this.controlPoints[this.controlPoints.Count - 1];

                //TODO: For Catmull-Rom, distance and time is not the same
                float d = Vector3.Distance(p.Item2, point);

                this.controlPoints.Add(new Tuple<float, Vector3>(p.Item1 + d, point));

                this.Length += d;
            }
            else
            {
                this.controlPoints.Add(new Tuple<float, Vector3>(0, point));
            }
        }
        /// <summary>
        /// Add control point with specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="point">Point</param>
        public void AddPosition(float time, Vector3 point)
        {
            if (this.controlPoints.Count > 0)
            {
                Tuple<float, Vector3> p = this.controlPoints[this.controlPoints.Count - 1];

                this.controlPoints.Add(new Tuple<float, Vector3>(p.Item1 + time, point));

                this.Length += time;
            }
            else
            {
                this.controlPoints.Add(new Tuple<float, Vector3>(0, point));
            }
        }
        /// <summary>
        /// Get curve position in time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns position in time</returns>
        public Vector3 GetPosition(float time)
        {
            return this.GetPosition(time, CurveInterpolations.Linear);
        }
        /// <summary>
        /// Get curve position in time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="interpolation">Interpolation type</param>
        /// <returns>Returns position in time</returns>
        public Vector3 GetPosition(float time, CurveInterpolations interpolation)
        {
            int segmentNum;
            float pos;
            this.FindSegment(time, out segmentNum, out pos);

            Vector3[] p = new Vector3[4];
            p[0] = segmentNum == 0 ? this.startPoint : this.controlPoints[segmentNum - 1].Item2;
            p[1] = this.controlPoints[segmentNum + 0].Item2;
            p[2] = this.controlPoints[segmentNum + 1].Item2;
            p[3] = segmentNum >= this.controlPoints.Count - 2 ? this.endPoint : this.controlPoints[segmentNum + 2].Item2;

            if (interpolation == CurveInterpolations.Linear)
            {
                return Vector3.Lerp(p[1], p[2], pos);
            }
            else if (interpolation == CurveInterpolations.SmoothStep)
            {
                return Vector3.SmoothStep(p[1], p[2], pos);
            }
            else if (interpolation == CurveInterpolations.CatmullRom)
            {
                return Vector3.CatmullRom(p[0], p[1], p[2], p[3], pos);
            }
            else
            {
                throw new Exception(string.Format("Bad interpolation mode: {0}", interpolation));
            }
        }
        /// <summary>
        /// Find segment and relative segment time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="segment">Segment</param>
        /// <param name="segmentDistance">Relative segment time</param>
        public void FindSegment(float time, out int segment, out float segmentDistance)
        {
            segment = -1;
            segmentDistance = 0;

            if (time == 0)
            {
                segment = 0;
                segmentDistance = 0;
            }
            else
            {
                for (int i = 0; i < this.controlPoints.Count; i++)
                {
                    if (time <= this.controlPoints[i].Item1)
                    {
                        segment = i - 1;

                        float d = this.controlPoints[i].Item1 - this.controlPoints[i - 1].Item1;

                        segmentDistance = (time - this.controlPoints[i - 1].Item1) / d;

                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Interpolation modes
    /// </summary>
    public enum CurveInterpolations
    {
        /// <summary>
        /// Linear
        /// </summary>
        Linear,
        /// <summary>
        /// Smooth step
        /// </summary>
        SmoothStep,
        /// <summary>
        /// Catmull-Rom
        /// </summary>
        CatmullRom,
    }
}
