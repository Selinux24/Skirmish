using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A point in a navigation mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PathPoint
    {
        /// <summary>
        /// A null point that isn't associated with any polygon.
        /// </summary>
        public static readonly PathPoint Null = new PathPoint(PolyId.Null, Vector3.Zero);

        /// <summary>
        /// A reference to the polygon this point is on.
        /// </summary>
        public PolyId Polygon;
        /// <summary>
        /// The 3d position of the point.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathPoint"/> struct.
        /// </summary>
        /// <param name="polygon">The polygon that the point is on.</param>
        /// <param name="position">The 3d position of the point.</param>
        public PathPoint(PolyId polygon, Vector3 position)
        {
            this.Polygon = polygon;
            this.Position = position;
        }
    }
}
