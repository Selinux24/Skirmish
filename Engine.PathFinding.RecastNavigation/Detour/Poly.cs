using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines a polygon within a MeshTile object.
    /// </summary>
    [Serializable]
    public class Poly
    {
        /// <summary>
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        /// </summary>
        public int FirstLink { get; set; }
        /// <summary>
        /// The indices of the polygon's vertices. The actual vertices are located in dtMeshTile::verts.
        /// </summary>
        public IndexedPolygon Verts { get; set; } = new IndexedPolygon(DetourUtils.DT_VERTS_PER_POLYGON);
        /// <summary>
        /// Packed data representing neighbor polygons references and flags for each edge.
        /// </summary>
        public int[] Neis { get; set; } = new int[DetourUtils.DT_VERTS_PER_POLYGON];
        /// <summary>
        /// The user defined polygon flags.
        /// </summary>
        public SamplePolyFlagTypes Flags { get; set; }
        /// <summary>
        /// The number of vertices in the polygon.
        /// </summary>
        public int VertCount { get; set; }
        /// <summary>
        /// Polygon area
        /// </summary>
        public SamplePolyAreas Area { get; set; }
        /// <summary>
        /// Polygon type
        /// </summary>
        public PolyTypes Type { get; set; }

        /// <summary>
        /// Gets the neighbour direction
        /// </summary>
        /// <param name="index">Neighbour index</param>
        /// <returns></returns>
        public int GetNeighbourDir(int index)
        {
            return Neis[index] & 0xff;
        }

        /// <summary>
        /// Gets the text representation of the polygon
        /// </summary>
        /// <returns>Returns the text representation of the polygon</returns>
        public override string ToString()
        {
            return string.Format("FirstLink {0}; Flags {1}; Area: {2}; Type: {3}; Verts {4}; VertCount: {5}; Neis: {6}",
                FirstLink, Flags, Area, Type, Verts, VertCount, Neis?.Join(","));
        }
    }
}
