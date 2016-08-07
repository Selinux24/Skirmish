using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// PathFinder path
    /// </summary>
    public class PathFindingPath
    {
        /// <summary>
        /// Path identifier
        /// </summary>
        public readonly Guid Id = Guid.NewGuid();
        /// <summary>
        /// Path nodes
        /// </summary>
        public readonly List<Vector3> ReturnPath = new List<Vector3>();
        /// <summary>
        /// Start position
        /// </summary>
        public Vector3 StartPosition;
        /// <summary>
        /// End position
        /// </summary>
        public Vector3 EndPosition;
        /// <summary>
        /// Total distance
        /// </summary>
        public float Distance
        {
            get
            {
                float distance = 0f;

                for (int i = 0; i < this.ReturnPath.Count - 1; i++)
                {
                    distance += Vector3.Distance(this.ReturnPath[i], this.ReturnPath[i + 1]);
                }

                return distance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="returnPath">Node list</param>
        public PathFindingPath(Vector3 startPosition, Vector3 endPosition, Vector3[] returnPath)
        {
            this.StartPosition = startPosition;
            this.EndPosition = endPosition;

            if (returnPath != null && returnPath.Length > 0)
            {
                this.ReturnPath.AddRange(returnPath);
            }
        }
        /// <summary>
        /// Generates a curve that goes through the nodes of the way
        /// </summary>
        /// <returns>Returns the 3D curve generated</returns>
        public Curve3D GenerateCurve()
        {
            Curve3D curve = new Curve3D();

            curve.AddPosition(0, this.StartPosition);

            float distanceAcum = 0;

            for (int i = 1; i < this.ReturnPath.Count - 1; i++)
            {
                Vector3 position = this.ReturnPath[i];

                if (i > 0)
                {
                    Vector3 previousPosition = this.ReturnPath[i - 1];

                    distanceAcum += Vector3.Distance(position, previousPosition);
                }

                curve.AddPosition(distanceAcum, this.ReturnPath[i]);
            }

            distanceAcum += Vector3.Distance(this.EndPosition, curve.GetKey(curve.KeyCount - 1));

            curve.AddPosition(distanceAcum, this.EndPosition);

            curve.SetTangents();

            return curve;
        }
        /// <summary>
        /// Generates a curve that goes through the nodes of the way to the maximum distance indicated
        /// </summary>
        /// <param name="maximumDistance">Maximum distance</param>
        /// <param name="distance">Total result distance</param>
        /// <returns>Returns the 3D curve generated</returns>
        public Curve3D GenerateCurve(float maximumDistance, out float distance)
        {
            distance = 0;

            Curve3D curve = new Curve3D();

            float distanceAcum = 0;

            for (int i = 0; i < this.ReturnPath.Count; i++)
            {
                Vector3 position = this.ReturnPath[i];

                if (i > 0)
                {
                    Vector3 previousPosition = this.ReturnPath[i - 1];

                    distanceAcum += Vector3.Distance(position, previousPosition);
                }

                if (distanceAcum > maximumDistance)
                {
                    //Reached the maximum distance
                    distanceAcum = maximumDistance;
                    break;
                }

                curve.AddPosition(distanceAcum, this.ReturnPath[i]);
            }

            distance = distanceAcum;

            return curve;
        }
        /// <summary>
        /// Generates a bezier path that goes through the nodes of the way
        /// </summary>
        /// <returns>Returns the bezier path generated</returns>
        public BezierPath GenerateBezierPath()
        {
            BezierPath curve = new BezierPath();

            List<Vector3> positions = new List<Vector3>();

            positions.Add(this.StartPosition);

            for (int i = 1; i < this.ReturnPath.Count - 1; i++)
            {
                Vector3 position = this.ReturnPath[i];

                if (i > 0)
                {
                    Vector3 previousPosition = this.ReturnPath[i - 1];
                }

                positions.Add(this.ReturnPath[i]);
            }

            positions.Add(this.EndPosition);

            curve.SetControlPoints(positions.ToArray(), 0.25f);

            return curve;
        }
    }
}
