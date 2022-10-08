﻿using SharpDX;
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
