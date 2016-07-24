using SharpDX;
using System;

namespace Engine.Common
{
    /// <summary>
    /// An offmesh connection links two polygons, which are not directly adjacent, but are accessibly through
    /// other means (jumping, climbing, etc...).
    /// </summary>
    public class OffMeshConnection
    {
        /// <summary>
        /// Gets or sets the first endpoint of the connection
        /// </summary>
        public Vector3 Pos0 { get; set; }
        /// <summary>
        /// Gets or sets the second endpoint of the connection
        /// </summary>
        public Vector3 Pos1 { get; set; }
        /// <summary>
        /// Gets or sets the radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Gets or sets the polygon's index
        /// </summary>
        public int Poly { get; set; }
        /// <summary>
        /// Gets or sets the polygon flag
        /// </summary>
        public OffMeshConnectionFlags Flags { get; set; }
        /// <summary>
        /// Gets or sets the endpoint's side
        /// </summary>
        public BoundarySide Side { get; set; }
        /// <summary>
        /// Gets or sets user data for this connection.
        /// </summary>
        public object Tag { get; set; }
    }
    /// <summary>
    /// A set of flags that define properties about an off-mesh connection.
    /// </summary>
    [Flags]
    public enum OffMeshConnectionFlags : byte
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The connection is bi-directional.
        /// </summary>
        Bidirectional = 0x1
    }
}
