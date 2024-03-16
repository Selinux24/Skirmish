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
                return new TileRef();
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
        /// <param name="filter">Query filter</param>
        /// <param name="parentRef">Parent reference</param>
        /// <param name="navMesh">Navigation mesh</param>
        /// <param name="nodePool">Node pool</param>
        public readonly IEnumerable<(int Ref, TileRef TileRef, Node NeighbourNode)> ItearatePolygonLinks(QueryFilter filter, int parentRef, NavMesh navMesh, NodePool nodePool)
        {
            for (int i = Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = Tile.Links[i].Next)
            {
                int neighbourRef = Tile.Links[i].NRef;

                // Skip invalid ids
                if (neighbourRef == 0)
                {
                    continue;
                }

                // Do not expand back to where we came from.
                if (neighbourRef == parentRef)
                {
                    continue;
                }

                // Get neighbour poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var neighbour = navMesh.GetTileAndPolyByRefUnsafe(neighbourRef);

                if (!filter.PassFilter(neighbour.Poly.Flags))
                {
                    continue;
                }

                // deal explicitly with crossing tile boundaries
                int crossSide = 0;
                if (Tile.Links[i].Side != 0xff)
                {
                    crossSide = Tile.Links[i].Side >> 1;
                }

                // get the node
                var neighbourNode = nodePool.GetNode(neighbourRef, crossSide);

                yield return (neighbourRef, neighbour, neighbourNode);
            }
        }
    }
}