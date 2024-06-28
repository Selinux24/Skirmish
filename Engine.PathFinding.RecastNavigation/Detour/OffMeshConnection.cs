using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines an navigation mesh off-mesh connection within a dtMeshTile object.
    /// An off-mesh connection is a user defined traversable connection made up to two vertices.
    /// </summary>
    [Serializable]
    public class OffMeshConnection
    {
        /// <summary>
        /// The start endpoint of the connection. [(ax, ay, az)]
        /// </summary>
        public Vector3 Start { get; set; }
        /// <summary>
        /// The end endpoint of the connection. [(bx, by, bz)]
        /// </summary>
        public Vector3 End { get; set; }
        /// <summary>
        /// The radius of the endpoints. [Limit: >= 0]
        /// </summary>
        public float Rad { get; set; }
        /// <summary>
        /// The polygon reference of the connection within the tile.
        /// </summary>
        public int Poly { get; set; }
        /// <summary>
        /// Link flags. 
        /// </summary>
        /// <remarks>
        /// These are not the connection's user defined flags. Those are assigned via the connection's dtPoly definition. These are link flags used for internal purposes.
        /// </remarks>
        public OffMeshConnectionDirections Direction { get; set; }
        /// <summary>
        /// End point side.
        /// </summary>
        public int Side { get; set; }
        /// <summary>
        /// The id of the offmesh connection. (User assigned when the navigation mesh is built.)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets whether this off-mesh connection is bidirectional or not
        /// </summary>
        /// <returns></returns>
        public bool IsBidirectional()
        {
            return Direction == OffMeshConnectionDirections.Bidirectional;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"start: {Start}; end: {End}; rad: {Rad}; poly: {Poly}; direction: {Direction}; side: {Side}; userId: {UserId};";
        }
    }
}
