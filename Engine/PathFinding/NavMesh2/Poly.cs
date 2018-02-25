
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Defines a polygon within a dtMeshTile object.
    /// </summary>
    public class Poly
    {
        public const int DT_VERTS_PER_POLYGON = 6;

        /// <summary>
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        /// </summary>
        public uint firstLink;
        /// <summary>
        /// The indices of the polygon's vertices. The actual vertices are located in dtMeshTile::verts.
        /// </summary>
        public uint[] verts = new uint[DT_VERTS_PER_POLYGON];
        /// <summary>
        /// Packed data representing neighbor polygons references and flags for each edge.
        /// </summary>
        public uint[] neis = new uint[DT_VERTS_PER_POLYGON];
        /// <summary>
        /// The user defined polygon flags.
        /// </summary>
        public uint flags;
        /// <summary>
        /// The number of vertices in the polygon.
        /// </summary>
        public uint vertCount;

        /// <summary>
        /// The bit packed area id and polygon type.
        /// </summary>
        private uint areaAndtype;
    }
}
