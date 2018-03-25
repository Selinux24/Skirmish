using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Defines a polygon within a dtMeshTile object.
    /// </summary>
    [Serializable]
    public class Poly
    {
        /// <summary>
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        /// </summary>
        public int firstLink;
        /// <summary>
        /// The indices of the polygon's vertices. The actual vertices are located in dtMeshTile::verts.
        /// </summary>
        public Polygoni verts = new Polygoni(Constants.DT_VERTS_PER_POLYGON);
        /// <summary>
        /// Packed data representing neighbor polygons references and flags for each edge.
        /// </summary>
        public int[] neis = new int[Constants.DT_VERTS_PER_POLYGON];
        /// <summary>
        /// The user defined polygon flags.
        /// </summary>
        public SamplePolyFlags flags;
        /// <summary>
        /// The number of vertices in the polygon.
        /// </summary>
        public int vertCount;

        public SamplePolyAreas Area { get; set; }

        public PolyTypes Type { get; set; }

        public override string ToString()
        {
            return string.Format("FirstLink {0}; Flags {1}; Area: {2}; Type: {3}; Verts {4}; VertCount: {5}; Neis: {6}",
                firstLink, flags, Area, Type, verts, vertCount, neis?.Join(","));
        }
    }
}
