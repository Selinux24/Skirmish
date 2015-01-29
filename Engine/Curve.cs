using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    public class Curve
    {
        private List<Vector3> controlPoints = new List<Vector3>();

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
                }
            }
        }

        private Vector3 startPoint
        {
            get
            {
                Vector3 v = this.controlPoints[0] - this.controlPoints[1];

                return this.controlPoints[0] + v;
            }
        }
        private Vector3 endPoint
        {
            get
            {
                Vector3 v = this.controlPoints[this.controlPoints.Count - 1] - this.controlPoints[this.controlPoints.Count - 2];

                return this.controlPoints[this.controlPoints.Count - 1] + v;
            }
        }

        public float Length { get; set; }

        public Curve(Vector3[] points)
        {
            this.Points = points;

            for (int i = 0; i < points.Length - 1; i++)
            {
                this.Length += Vector3.Distance(points[i + 0], points[i + 1]);
            }
        }

        public Curve(Vector3[] points, float length)
        {
            this.Points = points;
            this.Length = length;
        }

        public Vector3 GetPosition(float distance, CurveInterpolations interpolation)
        {
            //Segment length
            float segLen = this.Length / (this.controlPoints.Count - 1);

            //Segment index
            int segmentNum = (int)(distance / segLen);

            //Relative position in segment
            float pos = (distance - (segmentNum * segLen)) / segLen;

            Vector3[] p = new Vector3[4];
            p[0] = segmentNum == 0 ? this.startPoint : this.controlPoints[segmentNum - 1];
            p[1] = this.controlPoints[segmentNum + 0];
            p[2] = this.controlPoints[segmentNum + 1];
            p[3] = segmentNum >= this.controlPoints.Count - 2 ? this.endPoint : this.controlPoints[segmentNum + 2];

            if (interpolation == CurveInterpolations.Linear)
            {
                return Vector3.Lerp(p[1], p[2], pos);
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
    }

    public enum CurveInterpolations
    {
        Linear,
        CatmullRom,
    }
}
