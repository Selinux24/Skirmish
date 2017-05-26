using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A point in a navigation mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct PathPoint
    {
        /// <summary>
        /// A null point that isn't associated with any polygon.
        /// </summary>
        public static readonly PathPoint Null = new PathPoint(0, Vector3.Zero);

        /// <summary>
        /// A reference to the polygon this point is on.
        /// </summary>
        public int Polygon;
        /// <summary>
        /// The 3d position of the point.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathPoint"/> struct.
        /// </summary>
        /// <param name="poly">The polygon that the point is on.</param>
        /// <param name="pos">The 3d position of the point.</param>
        public PathPoint(int poly, Vector3 pos)
        {
            this.Polygon = poly;
            this.Position = pos;
        }

        /// <summary>
        /// Gets the string representation of the instance
        /// </summary>
        public override string ToString()
        {
            return string.Format("Polygon: {0}; Position: {1}", this.Polygon, this.Position);
        }
    }
}
