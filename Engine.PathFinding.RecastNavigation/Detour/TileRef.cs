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
        
        /// <summary>
        /// Returns portal points between two polygons.
        /// </summary>
        public static Status GetPortalPoints(TileRef from, TileRef to, out Vector3 left, out Vector3 right)
        {
            left = new();
            right = new();

            // Find the link that points to the 'to' polygon.
            Link? link = null;
            for (int i = from.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = from.Tile.Links[i].Next)
            {
                if (from.Tile.Links[i].NRef == to.Ref)
                {
                    link = from.Tile.Links[i];
                    break;
                }
            }
            if (!link.HasValue)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Handle off-mesh connections.
            if (from.Poly.Type == PolyTypes.OffmeshConnection)
            {
                // Find link that points to first vertex.
                return from.Tile.FindLinkToNeighbour(from.Poly, to.Ref, out left, out right);
            }

            if (to.Poly.Type == PolyTypes.OffmeshConnection)
            {
                return to.Tile.FindLinkToNeighbour(to.Poly, from.Ref, out left, out right);
            }

            // Find portal vertices.
            int v0 = from.Poly.Verts[link.Value.Edge];
            int v1 = from.Poly.Verts[(link.Value.Edge + 1) % from.Poly.VertCount];
            left = from.Tile.Verts[v0];
            right = from.Tile.Verts[v1];

            // If the link is at tile boundary, dtClamp the vertices to
            // the link width.
            if (link.Value.Side != 0xff && (link.Value.BMin != 0 || link.Value.BMax != 255))
            {
                // Unpack portal limits.
                float s = 1.0f / 255.0f;
                float tmin = link.Value.BMin * s;
                float tmax = link.Value.BMax * s;
                left = Vector3.Lerp(from.Tile.Verts[v0], from.Tile.Verts[v1], tmin);
                right = Vector3.Lerp(from.Tile.Verts[v0], from.Tile.Verts[v1], tmax);
            }

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Returns edge mid point between two polygons.
        /// </summary>
        public static Status GetEdgeMidPoint(TileRef from, TileRef to, out Vector3 mid)
        {
            mid = new();

            if (GetPortalPoints(from, to, out var left, out var right).HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            mid = (left + right) * 0.5f;

            return Status.DT_SUCCESS;
        }
    }
}