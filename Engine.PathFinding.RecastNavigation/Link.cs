
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
        public int nref { get; set; }
        /// <summary>
        /// Index of the next link.
        /// </summary>
        public int next { get; set; }
        /// <summary>
        /// Index of the polygon edge that owns this link.
        /// </summary>
        public int edge { get; set; }
        /// <summary>
        /// If a boundary link, defines on which side the link is.
        /// </summary>
        public int side { get; set; }
        /// <summary>
        /// If a boundary link, defines the minimum sub-edge area.
        /// </summary>
        public int bmin { get; set; }
        /// <summary>
        /// If a boundary link, defines the maximum sub-edge area.
        /// </summary>
        public int bmax { get; set; }

        public override string ToString()
        {
            return string.Format("Ref {0}; Next {1}; Edge {2}; Side {3}; BMin {4}; BMax {5};",
                nref, next, edge, side, bmin, bmax);
        }
    }
}
