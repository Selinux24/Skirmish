using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.PathFinding
{
    using Engine.Common;

    /// <summary>
    /// PathFinder path
    /// </summary>
    public class Path
    {
        /// <summary>
        /// Path identifier
        /// </summary>
        public readonly Guid Id = Guid.NewGuid();
        /// <summary>
        /// Path nodes
        /// </summary>
        public readonly List<GridNode> ReturnPath = new List<GridNode>();
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
                    distance += Vector3.Distance(this.ReturnPath[i].Center, this.ReturnPath[i + 1].Center);
                }

                return distance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="returnPath">Node list</param>
        public Path(GridNode[] returnPath)
        {
            if (returnPath != null && returnPath.Length > 0)
            {
                this.ReturnPath.AddRange(returnPath);
            }
        }
        /// <summary>
        /// Generates a curve that goes through the nodes of the way
        /// </summary>
        /// <returns>Returns the 3D curve generated</returns>
        public Curve GenerateCurve()
        {
            Curve curve = new Curve();

            float distanceAcum = 0;

            for (int i = 0; i < this.ReturnPath.Count; i++)
            {
                Vector3 position = this.ReturnPath[i].Center;

                if (i > 0)
                {
                    Vector3 previousPosition = this.ReturnPath[i - 1].Center;

                    distanceAcum += Vector3.Distance(position, previousPosition);
                }

                curve.AddPosition(this.ReturnPath[i].Center);
            }

            return curve;
        }
        /// <summary>
        /// Generates a curve that goes through the nodes of the way to the maximum distance indicated
        /// </summary>
        /// <param name="maximumDistance">Maximum distance</param>
        /// <param name="distance">Total result distance</param>
        /// <returns>Returns the 3D curve generated</returns>
        public Curve GenerateCurve(float maximumDistance, out float distance)
        {
            distance = 0;

            Curve curve = new Curve();

            float distanceAcum = 0;

            for (int i = 0; i < this.ReturnPath.Count; i++)
            {
                Vector3 position = this.ReturnPath[i].Center;

                if (i > 0)
                {
                    Vector3 previousPosition = this.ReturnPath[i - 1].Center;

                    distanceAcum += Vector3.Distance(position, previousPosition);
                }

                if (distanceAcum > maximumDistance)
                {
                    //Reached the maximum distance
                    distance = maximumDistance;
                    break;
                }

                curve.AddPosition(this.ReturnPath[i].Center);
            }

            distance = distanceAcum;

            return curve;
        }
    }
}
