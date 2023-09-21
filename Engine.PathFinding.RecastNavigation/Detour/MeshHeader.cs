using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides high level information related to a dtMeshTile object.
    /// </summary>
    [Serializable]
    public struct MeshHeader
    {
        /// <summary>
        /// A magic number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int DT_NAVMESH_MAGIC = 'D' << 24 | 'N' << 16 | 'A' << 8 | 'V';
        /// <summary>
        /// A version number used to detect compatibility of navigation tile data.
        /// </summary>
        public const int DT_NAVMESH_VERSION = 7;

        /// <summary>
        /// Tile magic number. (Used to identify the data format.)
        /// </summary>
        public int Magic { get; set; }
        /// <summary>
        /// Tile data format version number.
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// The x-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// The y-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// The layer of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int Layer { get; set; }
        /// <summary>
        /// The user defined id of the tile.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The number of polygons in the tile.
        /// </summary>
        public int PolyCount { get; set; }
        /// <summary>
        /// The number of vertices in the tile.
        /// </summary>
        public int VertCount { get; set; }
        /// <summary>
        /// The number of allocated links.
        /// </summary>
        public int MaxLinkCount { get; set; }
        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int DetailMeshCount { get; set; }
        /// <summary>
        /// The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)
        /// </summary>
        public int DetailVertCount { get; set; }
        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int DetailTriCount { get; set; }
        /// <summary>
        /// The number of bounding volume nodes. (Zero if bounding volumes are disabled.)
        /// </summary>
        public int BvNodeCount { get; set; }
        /// <summary>
        /// The number of off-mesh connections.
        /// </summary>
        public int OffMeshConCount { get; set; }
        /// <summary>
        /// The index of the first polygon which is an off-mesh connection.
        /// </summary>
        public int OffMeshBase { get; set; }
        /// <summary>
        /// The height of the agents using the tile.
        /// </summary>
        public float WalkableHeight { get; set; }
        /// <summary>
        /// The radius of the agents using the tile.
        /// </summary>
        public float WalkableRadius { get; set; }
        /// <summary>
        /// The maximum climb height of the agents using the tile.
        /// </summary>
        public float WalkableClimb { get; set; }
        /// <summary>
        /// The bounds of the tile's AABB.
        /// </summary>
        public BoundingBox Bounds { get; set; }
        /// <summary>
        /// The bounding volume quantization factor.
        /// </summary>
        public float BvQuantFactor { get; set; }

        /// <summary>
        /// Validates the header magic and version
        /// </summary>
        public readonly bool IsValid()
        {
            if (Magic != DT_NAVMESH_MAGIC)
            {
                return false;
            }
            if (Version != DT_NAVMESH_VERSION)
            {
                return false;
            }

            return true;
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{X}.{Y}.{Layer}; Id: {UserId}; Bbox: {Bounds}; Polys: {PolyCount}; Vertices: {VertCount}; DMeshes: {DetailMeshCount}; DTriangles: {DetailTriCount}; DVertices: {DetailVertCount}";
        }
    };
}
