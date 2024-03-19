using System;
using System.Linq;

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
        public int[] Verts { get; set; } = new int[IndexedPolygon.DT_VERTS_PER_POLYGON];
        /// <summary>
        /// Packed data representing neighbor polygons references and flags for each edge.
        /// </summary>
        public int[] Neis { get; set; } = new int[IndexedPolygon.DT_VERTS_PER_POLYGON];
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
        /// Creates a polygon
        /// </summary>
        /// <param name="start">Start reference</param>
        /// <param name="end">End reference</param>
        /// <param name="flags">Connection flags</param>
        /// <param name="area">Area flags</param>
        public static Poly CreateOffMesh(int start, int end, SamplePolyFlagTypes flags, SamplePolyAreas area)
        {
            var p = new Poly
            {
                Flags = flags,
                Area = area,
                Type = PolyTypes.OffmeshConnection
            };
            p.Verts[0] = start;
            p.Verts[1] = end;
            p.VertCount = 2;

            return p;
        }
        /// <summary>
        /// Creates a polygon
        /// </summary>
        /// <param name="flags">Sample flags</param>
        /// <param name="area">Sample area</param>
        /// <param name="nvp">Maximum vertices per poligon</param>
        public static Poly Create(IndexedPolygon polygon, SamplePolyFlagTypes flags, SamplePolyAreas area, int nvp)
        {
            int[] verts = new int[nvp];
            int[] neis = new int[nvp];

            var polyVerts = polygon.GetVertices();
            var polyAdjacency = polygon.GetAdjacency();

            Array.ConstrainedCopy(polyVerts, 0, verts, 0, nvp);
            Array.ConstrainedCopy(polyAdjacency, 0, neis, 0, nvp);

            neis = neis.Select(DecodeNei).ToArray();

            int vertCount = Array.IndexOf(verts, -1);
            vertCount = vertCount < 0 ? nvp : vertCount;

            return new Poly
            {
                Flags = flags,
                Area = area,
                Type = PolyTypes.Ground,
                Verts = verts,
                Neis = neis,
                VertCount = vertCount,
            };
        }
        /// <summary>
        /// Decodes the neighbor index
        /// </summary>
        /// <param name="n">Neighbor index</param>
        private static int DecodeNei(int n)
        {
            if (Edge.IsExternalLink(n))
            {
                // Border or portal edge.
                return Edge.CalculateVertexPortalFlag(n);
            }
            else
            {
                // Normal connection
                return n + 1;
            }
        }

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
        /// Gets whether the neighbour is an external link or not
        /// </summary>
        /// <param name="nei">Neighbour index</param>
        public bool NeighbourIsExternalLink(int nei)
        {
            return Edge.IsExternalLink(Neis[nei]);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"FirstLink {FirstLink}; Flags {Flags}; Area: {Area}; Type: {Type}; Verts {Verts}; VertCount: {VertCount}; Neis: {Neis?.Join(",")}";
        }
    }
}
