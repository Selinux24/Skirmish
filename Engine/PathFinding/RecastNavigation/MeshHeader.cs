using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
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
        public int magic;
        /// <summary>
        /// Tile data format version number.
        /// </summary>
        public int version;
        /// <summary>
        /// The x-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int x;
        /// <summary>
        /// The y-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int y;
        /// <summary>
        /// The layer of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int layer;
        /// <summary>
        /// The user defined id of the tile.
        /// </summary>
        public int userId;
        /// <summary>
        /// The number of polygons in the tile.
        /// </summary>
        public int polyCount;
        /// <summary>
        /// The number of vertices in the tile.
        /// </summary>
        public int vertCount;
        /// <summary>
        /// The number of allocated links.
        /// </summary>
        public int maxLinkCount;
        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int detailMeshCount;

        /// <summary>
        /// The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)
        /// </summary>
        public int detailVertCount;
        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int detailTriCount;
        /// <summary>
        /// The number of bounding volume nodes. (Zero if bounding volumes are disabled.)
        /// </summary>
        public int bvNodeCount;
        /// <summary>
        /// The number of off-mesh connections.
        /// </summary>
        public int offMeshConCount;
        /// <summary>
        /// The index of the first polygon which is an off-mesh connection.
        /// </summary>
        public int offMeshBase;
        /// <summary>
        /// The height of the agents using the tile.
        /// </summary>
        public float walkableHeight;
        /// <summary>
        /// The radius of the agents using the tile.
        /// </summary>
        public float walkableRadius;
        /// <summary>
        /// The maximum climb height of the agents using the tile.
        /// </summary>
        public float walkableClimb;
        /// <summary>
        /// The minimum bounds of the tile's AABB. [(x, y, z)]
        /// </summary>
        public Vector3 bmin;
        /// <summary>
        /// The maximum bounds of the tile's AABB. [(x, y, z)]
        /// </summary>
        public Vector3 bmax;

        /// <summary>
        /// The bounding volume quantization factor.
        /// </summary>
        public float bvQuantFactor;


        public MeshHeader(SerializationInfo info, StreamingContext context)
        {
            magic = info.GetInt32("magic");
            version = info.GetInt32("version");
            x = info.GetInt32("x");
            y = info.GetInt32("y");
            layer = info.GetInt32("layer");
            userId = info.GetInt32("userId");
            polyCount = info.GetInt32("polyCount");
            vertCount = info.GetInt32("vertCount");
            maxLinkCount = info.GetInt32("maxLinkCount");
            detailMeshCount = info.GetInt32("detailMeshCount");
            detailVertCount = info.GetInt32("detailVertCount");
            detailTriCount = info.GetInt32("detailTriCount");
            bvNodeCount = info.GetInt32("bvNodeCount");
            offMeshConCount = info.GetInt32("offMeshConCount");
            offMeshBase = info.GetInt32("offMeshBase");
            walkableHeight = info.GetSingle("walkableHeight");
            walkableRadius = info.GetSingle("walkableRadius");
            walkableClimb = info.GetSingle("walkableClimb");
            bmin = info.GetVector3("bmin");
            bmax = info.GetVector3("bmax");
            bvQuantFactor = info.GetSingle("bvQuantFactor");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("magic", magic);
            info.AddValue("version", version);
            info.AddValue("x", x);
            info.AddValue("y", y);
            info.AddValue("layer", layer);
            info.AddValue("userId", userId);
            info.AddValue("polyCount", polyCount);
            info.AddValue("vertCount", vertCount);
            info.AddValue("maxLinkCount", maxLinkCount);
            info.AddValue("detailMeshCount", detailMeshCount);
            info.AddValue("detailVertCount", detailVertCount);
            info.AddValue("detailTriCount", detailTriCount);
            info.AddValue("bvNodeCount", bvNodeCount);
            info.AddValue("offMeshConCount", offMeshConCount);
            info.AddValue("offMeshBase", offMeshBase);
            info.AddValue("walkableHeight", walkableHeight);
            info.AddValue("walkableRadius", walkableRadius);
            info.AddValue("walkableClimb", walkableClimb);
            info.AddVector3("bmin", bmin);
            info.AddVector3("bmax", bmax);
            info.AddValue("bvQuantFactor", bvQuantFactor);
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}; Id: {3}; Bbox: {4}{5}; Polys: {6}; Vertices: {7}; DMeshes: {8}; DTriangles: {9}; DVertices: {10}",
                x, y, layer, userId,
                bmin, bmax,
                polyCount, vertCount,
                detailMeshCount, detailTriCount, detailVertCount);
        }
    };
}
