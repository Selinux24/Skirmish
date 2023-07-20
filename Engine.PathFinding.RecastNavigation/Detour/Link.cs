
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

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a text representation of the instance</returns>
        public override readonly string ToString()
        {
            return $"Ref {NRef}; Next {Next}; Edge {Edge}; Side {Side}; BMin {BMin}; BMax {BMax};";
        }
    }
}
