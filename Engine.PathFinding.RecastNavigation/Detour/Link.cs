
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines a link between polygons.
    /// </summary>
    public struct Link
    {
        /// <summary>
        /// Neighbour reference. (The neighbor that is linked to.)
        /// </summary>
        public int NRef { get; set; }
        /// <summary>
        /// Index of the next link.
        /// </summary>
        public int Next { get; set; }
        /// <summary>
        /// Index of the polygon edge that owns this link.
        /// </summary>
        public int Edge { get; set; }
        /// <summary>
        /// If a boundary link, defines on which side the link is.
        /// </summary>
        public int Side { get; set; }
        /// <summary>
        /// If a boundary link, defines the minimum sub-edge area.
        /// </summary>
        public int BMin { get; set; }
        /// <summary>
        /// If a boundary link, defines the maximum sub-edge area.
        /// </summary>
        public int BMax { get; set; }

        public override string ToString()
        {
            return string.Format("Ref {0}; Next {1}; Edge {2}; Side {3}; BMin {4}; BMax {5};",
                NRef, Next, Edge, Side, BMin, BMax);
        }
    }
}
