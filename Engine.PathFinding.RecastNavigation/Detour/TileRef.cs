using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Tile reference
    /// </summary>
    public struct TileRef
    {
        /// <summary>
        /// Returns the null tile
        /// </summary>
        public static TileRef Null
        {
            get
            {
                return new();
            }
        }

        /// <summary>
        /// Tile reference
        /// </summary>
        public int Ref { get; set; }
        /// <summary>
        /// Tile data
        /// </summary>
        public MeshTile Tile { get; set; }
        /// <summary>
        /// Polygon data
        /// </summary>
        public Poly Poly { get; set; }
        /// <summary>
        /// Node data
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Iterates the tile polygon links
        /// </summary>
        /// <returns></returns>
        public readonly IEnumerable<Link> IteratePolygonLinks()
        {
            for (int i = Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = Tile.Links[i].Next)
            {
                yield return Tile.Links[i];
            }
        }

        /// <summary>
        /// Gets the tile polygon vertex list
        /// </summary>
        public readonly Vector3[] GetPolyVertices()
        {
            int n = Poly.VertCount;

            Vector3[] verts = new Vector3[n];

            for (int k = 0; k < n; ++k)
            {
                verts[k] = Tile.Verts[Poly.Verts[k]];
            }

            return verts;
        }
    }
}