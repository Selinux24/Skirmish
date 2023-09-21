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
        /// A flag that indicates that an entity links to an external entity.
        /// (E.g. A polygon edge is a portal that links to another polygon.)
        /// </summary>
        public const int DT_EXT_LINK = 0x8000;

        /// <summary>
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        /// </summary>
        public int FirstLink { get; set; }
        /// <summary>
        /// The indices of the polygon's vertices. The actual vertices are located in dtMeshTile::verts.
        /// </summary>
        public int[] Verts { get; set; } = new int[NavMeshCreateParams.DT_VERTS_PER_POLYGON];
        /// <summary>
        /// Packed data representing neighbor polygons references and flags for each edge.
        /// </summary>
        public int[] Neis { get; set; } = new int[NavMeshCreateParams.DT_VERTS_PER_POLYGON];
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

        public static Poly Create(IndexedPolygon src, SamplePolyFlagTypes flags, SamplePolyAreas area, int nvp)
        {
            int[] verts = new int[nvp];
            int[] neis = new int[nvp];

            Array.ConstrainedCopy(src.GetVertices(), 0, verts, 0, nvp);
            Array.ConstrainedCopy(src.GetVertices(), nvp, neis, 0, nvp);

            neis = neis.Select(n => DecodeNei(n)).ToArray();

            int vertCount = Array.IndexOf(verts, -1);
            vertCount = vertCount < 0 ? nvp : vertCount;

            var p = new Poly
            {
                Flags = flags,
                Area = area,
                Type = PolyTypes.Ground,
                Verts = verts,
                Neis = neis,
                VertCount = vertCount,
            };

            return p;
        }
        public static Poly Create(int start, int end, GraphConnectionFlagTypes flags, GraphConnectionAreaTypes area)
        {
            var p = new Poly
            {
                Flags = (SamplePolyFlagTypes)flags,
                Area = (SamplePolyAreas)area,
                Type = PolyTypes.OffmeshConnection
            };
            p.Verts[0] = start;
            p.Verts[1] = end;
            p.VertCount = 2;

            return p;
        }
        private static int DecodeNei(int n)
        {
            if ((n & 0x8000) != 0)
            {
                // Border or portal edge.
                var dir = n & 0xf;
                if (dir == 0xf) // Border
                {
                    return 0;
                }
                else if (dir == 0) // Portal x-
                {
                    return DT_EXT_LINK | 4;
                }
                else if (dir == 1) // Portal z+
                {
                    return DT_EXT_LINK | 2;
                }
                else if (dir == 2) // Portal x+
                {
                    return DT_EXT_LINK;
                }
                else if (dir == 3) // Portal z-
                {
                    return DT_EXT_LINK | 6;
                }
            }
            else
            {
                // Normal connection
                return n + 1;
            }

            return n;
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"FirstLink {FirstLink}; Flags {Flags}; Area: {Area}; Type: {Type}; Verts {Verts}; VertCount: {VertCount}; Neis: {Neis?.Join(",")}";
        }
    }
}
