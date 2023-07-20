using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding
{
    /// <summary>
    /// PathFinder path
    /// </summary>
    public class PathFindingPath
    {
        /// <summary>
        /// Position list
        /// </summary>
        private readonly List<Vector3> positions = new();
        /// <summary>
        /// Normal list
        /// </summary>
        private readonly List<Vector3> normals = new();

        /// <summary>
        /// Gets the position control points
        /// </summary>
        public IEnumerable<Vector3> Positions
        {
            get
            {
                return positions.ToArray();
            }
        }
        /// <summary>
        /// Gets the normal control points
        /// </summary>
        public IEnumerable<Vector3> Normals
        {
            get
            {
                return normals.ToArray();
            }
        }
        /// <summary>
        /// Gets the control point count
        /// </summary>
        public int Count
        {
            get
            {
                return positions.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PathFindingPath()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="positions">Position list</param>
        /// <param name="normals">Normal list</param>
        public PathFindingPath(IEnumerable<Vector3> positions, IEnumerable<Vector3> normals)
        {
            if (positions?.Count() < 2)
            {
                throw new ArgumentException("A path must have two control points at least.", nameof(positions));
            }

            this.positions.AddRange(positions);

            if (normals?.Any() == true)
            {
                if (normals.Count() != positions.Count())
                {
                    throw new ArgumentException("Normals and positions must have the same number of elements.", nameof(normals));
                }

                this.normals.AddRange(normals);
            }
            else
            {
                this.normals.AddRange(Helper.CreateArray(positions.Count(), Vector3.Up));
            }
        }

        /// <summary>
        /// Adds a new control point
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        public void AddControlPoint(Vector3 position, Vector3 normal)
        {
            positions.Add(position);
            normals.Add(normal);
        }
        /// <summary>
        /// Inserts a new control point into the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        public void InsertControlPoint(int index, Vector3 position, Vector3 normal)
        {
            positions.Insert(index, position);
            normals.Insert(index, normal);
        }
        /// <summary>
        /// Removes the control point at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveControlPoint(int index)
        {
            positions.RemoveAt(index);
            normals.RemoveAt(index);
        }
        /// <summary>
        /// Clears the path
        /// </summary>
        public void Clear()
        {
            positions.Clear();
            normals.Clear();
        }

        /// <summary>
        /// Refines the path interpolating the existing control points by the given delta distance
        /// </summary>
        /// <param name="delta">Control point path deltas</param>
        public void RefinePath(float delta)
        {
            if (delta <= 0)
            {
                return;
            }

            if (positions.Count < 2)
            {
                return;
            }

            Logger.WriteTrace(this, $"PathFindingPath.RefinePath delta {delta}.");

            var lPositions = new List<Vector3>();
            var lNormals = new List<Vector3>();

            // Copy path and normals
            var positionArray = positions.ToArray();
            var normalArray = normals.ToArray();

            lPositions.Add(positionArray[0]);
            lNormals.Add(normalArray[0]);

            var p0 = positionArray[0];
            var p1 = positionArray[1];

            var n0 = normalArray[0];
            var n1 = normalArray[1];

            int index = 0;
            while (index < positionArray.Length - 1)
            {
                var s = p1 - p0;
                var v = Vector3.Normalize(s) * delta;
                var l = delta - s.Length();

                if (l <= 0f)
                {
                    //Into de segment
                    p0 += v;
                    n0 = Vector3.Normalize(Vector3.Lerp(n0, n1, l));
                }
                else if (index < positionArray.Length - 2)
                {
                    //Next segment
                    var p2 = positionArray[index + 2];
                    p0 = p1 + ((p2 - p1) * l);
                    p1 = p2;

                    var n2 = normalArray[index + 2];
                    n0 = Vector3.Normalize(Vector3.Lerp(n1, n2, l));

                    index++;
                }
                else
                {
                    //End
                    p0 = positionArray[index + 1];
                    n0 = normalArray[index + 1];

                    index++;
                }

                lPositions.Add(p0);
                lNormals.Add(n0);
            }

            positions.Clear();
            positions.AddRange(lPositions);

            normals.Clear();
            normals.AddRange(lNormals);
        }
    }
}
