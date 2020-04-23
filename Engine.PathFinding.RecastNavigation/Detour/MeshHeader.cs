using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides high level information related to a dtMeshTile object.
    /// </summary>
    [Serializable]
    public struct MeshHeader : ISerializable
    {
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
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Serializatio context</param>
        internal MeshHeader(SerializationInfo info, StreamingContext context)
        {
            Magic = info.GetInt32("magic");
            Version = info.GetInt32("version");
            X = info.GetInt32("x");
            Y = info.GetInt32("y");
            Layer = info.GetInt32("layer");
            UserId = info.GetInt32("userId");
            PolyCount = info.GetInt32("polyCount");
            VertCount = info.GetInt32("vertCount");
            MaxLinkCount = info.GetInt32("maxLinkCount");
            DetailMeshCount = info.GetInt32("detailMeshCount");
            DetailVertCount = info.GetInt32("detailVertCount");
            DetailTriCount = info.GetInt32("detailTriCount");
            BvNodeCount = info.GetInt32("bvNodeCount");
            OffMeshConCount = info.GetInt32("offMeshConCount");
            OffMeshBase = info.GetInt32("offMeshBase");
            WalkableHeight = info.GetSingle("walkableHeight");
            WalkableRadius = info.GetSingle("walkableRadius");
            WalkableClimb = info.GetSingle("walkableClimb");
            Bounds = info.GetValue<BoundingBox>("bounds");
            BvQuantFactor = info.GetSingle("bvQuantFactor");
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("magic", Magic);
            info.AddValue("version", Version);
            info.AddValue("x", X);
            info.AddValue("y", Y);
            info.AddValue("layer", Layer);
            info.AddValue("userId", UserId);
            info.AddValue("polyCount", PolyCount);
            info.AddValue("vertCount", VertCount);
            info.AddValue("maxLinkCount", MaxLinkCount);
            info.AddValue("detailMeshCount", DetailMeshCount);
            info.AddValue("detailVertCount", DetailVertCount);
            info.AddValue("detailTriCount", DetailTriCount);
            info.AddValue("bvNodeCount", BvNodeCount);
            info.AddValue("offMeshConCount", OffMeshConCount);
            info.AddValue("offMeshBase", OffMeshBase);
            info.AddValue("walkableHeight", WalkableHeight);
            info.AddValue("walkableRadius", WalkableRadius);
            info.AddValue("walkableClimb", WalkableClimb);
            info.AddValue("bounds", Bounds);
            info.AddValue("bvQuantFactor", BvQuantFactor);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}; Id: {3}; Bbox: {4}; Polys: {5}; Vertices: {6}; DMeshes: {7}; DTriangles: {8}; DVertices: {9}",
                X, Y, Layer, UserId,
                Bounds,
                PolyCount, VertCount,
                DetailMeshCount, DetailTriCount, DetailVertCount);
        }
    };
}
