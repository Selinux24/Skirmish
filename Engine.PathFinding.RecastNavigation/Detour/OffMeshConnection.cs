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
        public int Flags { get; set; }
        /// <summary>
        /// End point side.
        /// </summary>
        public int Side { get; set; }
        /// <summary>
        /// The id of the offmesh connection. (User assigned when the navigation mesh is built.)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("start: {0}; end: {1}; rad: {2}; poly: {3}; flags: {4}; side: {5}; userId: {6};",
                Start, End,
                Rad, Poly,
                Flags, Side, UserId);
        }
    }
}
