
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Defines a link between polygons.
    /// </summary>
    public struct Link
    {
        /// <summary>
        /// Neighbour reference. (The neighbor that is linked to.)
        /// </summary>
        public int nref;
        /// <summary>
        /// Index of the next link.
        /// </summary>
        public int next;
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

        public override string ToString()
        {
            return string.Format("Ref {0}; Next {1}; Edge {2}; Side {3}; BMin {4}; BMax {5};",
                nref, next, edge, side, bmin, bmax);
        }
    }
}
