
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Defines a link between polygons.
    /// </summary>
    public struct Link
    {
        /// <summary>
        /// Neighbour reference. (The neighbor that is linked to.)
        /// </summary>
        public uint nref;
        /// <summary>
        /// Index of the next link.
        /// </summary>
        public uint next;
        /// <summary>
        /// Index of the polygon edge that owns this link.
        /// </summary>
        public int edge;
        /// <summary>
        /// If a boundary link, defines on which side the link is.
        /// </summary>
        public int side;
        /// <summary>
        /// If a boundary link, defines the minimum sub-edge area.
        /// </summary>
        public int bmin;
        /// <summary>
        /// If a boundary link, defines the maximum sub-edge area.
        /// </summary>
        public int bmax;
    }
}
