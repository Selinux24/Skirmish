
namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A link is formed between two polygons in a TiledNavMesh
    /// </summary>
    class Link
    {
        /// <summary>
        /// Entity links to external entity.
        /// </summary>
        public const int External = unchecked((int)0x80000000);
        /// <summary>
        /// Doesn't link to anything.
        /// </summary>
        public const int Null = unchecked((int)0xffffffff);

        public static bool IsExternal(int link)
        {
            return (link & Link.External) != 0;
        }

        /// <summary>
        /// Gets or sets the neighbor reference (the one it's linked to)
        /// </summary>
        public int Reference { get; set; }
        /// <summary>
        /// Gets or sets the index of polygon edge
        /// </summary>
        public int Edge { get; set; }
        /// <summary>
        /// Gets or sets the polygon side
        /// </summary>
        public BoundarySide Side { get; set; }
        /// <summary>
        /// Gets or sets the minimum Vector3 of the bounding box
        /// </summary>
        public int BMin { get; set; }
        /// <summary>
        /// Gets or sets the maximum Vector3 of the bounding box
        /// </summary>
        public int BMax { get; set; }
    }
}
