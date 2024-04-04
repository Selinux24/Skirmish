using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Represents the source data used to build an navigation mesh tile.
    /// </summary>
    public struct NavMeshCreateParams
    {
        #region Polygon Mesh Attributes

        // Used to create the base navigation graph.
        // See #rcPolyMesh for details related to these attributes.

        /// <summary>
        /// The polygon mesh vertices. [(x, y, z) * #vertCount] [Unit: vx]
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// The number vertices in the polygon mesh. [Limit: >= 3]
        /// </summary>
        public int VertCount { get; set; }
        /// <summary>
        /// The polygon data. [Size: #polyCount * 2 * #nvp]
        /// </summary>
        public IndexedPolygon[] Polys { get; set; }
        /// <summary>
        /// The user defined flags assigned to each polygon. [Size: #polyCount]
        /// </summary>
        public SamplePolyFlagTypes[] PolyFlags { get; set; }
        /// <summary>
        /// The user defined area ids assigned to each polygon. [Size: #polyCount]
        /// </summary>
        public SamplePolyAreas[] PolyAreas { get; set; }
        /// <summary>
        /// Number of polygons in the mesh. [Limit: >= 1]
        /// </summary>
        public int PolyCount { get; set; }
        /// <summary>
        /// Number maximum number of vertices per polygon. [Limit: >= 3]
        /// </summary>
        public int NVP { get; set; }

        #endregion

        #region Height Detail Attributes (Optional)

        // See #rcPolyMeshDetail for details related to these attributes.

        /// <summary>
        /// The height detail sub-mesh data. [Size: 4 * #polyCount]
        /// </summary>
        public PolyMeshIndices[] DetailMeshes { get; set; }
        /// <summary>
        /// The detail mesh vertices. [Size: 3 * #detailVertsCount] [Unit: wu]
        /// </summary>
        public Vector3[] DetailVerts { get; set; }
        /// <summary>
        /// The number of vertices in the detail mesh.
        /// </summary>
        public int DetailVertsCount { get; set; }
        /// <summary>
        /// The detail mesh triangles. [Size: 4 * #detailTriCount]
        /// </summary>
        public PolyMeshTriangleIndices[] DetailTris { get; set; }
        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int DetailTriCount { get; set; }

        #endregion

        #region Off-Mesh Connections Attributes (Optional)

        // Used to define a custom point-to-point edge within the navigation graph, an 
        // off-mesh connection is a user defined traversable connection made up to two vertices, 
        // at least one of which resides within a navigation mesh polygon.

        /// <summary>
        /// Off-mesh connections
        /// </summary>
        public IGraphConnection[] OffMeshCon { get; set; }
        /// <summary>
        /// The number of off-mesh connections. [Limit: >= 0]
        /// </summary>
        public int OffMeshConCount { get; set; }

        #endregion

        #region Tile Attributes

        // The tile grid/layer data can be left at zero if the destination is a single tile mesh.

        /// <summary>
        /// The user defined id of the tile.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The tile's x-grid location within the multi-tile destination mesh. (Along the x-axis.)
        /// </summary>
        public int TileX { get; set; }
        /// <summary>
        /// The tile's y-grid location within the multi-tile desitation mesh. (Along the z-axis.)
        /// </summary>
        public int TileY { get; set; }
        /// <summary>
        /// The tile's layer within the layered destination mesh. [Limit: >= 0] (Along the y-axis.)
        /// </summary>
        public int TileLayer { get; set; }
        /// <summary>
        /// The bounds of the tile. [Unit: wu]
        /// </summary>
        public BoundingBox Bounds { get; set; }

        #endregion

        #region General Configuration Attributes

        /// <summary>
        /// The agent height. [Unit: wu]
        /// </summary>
        public float WalkableHeight { get; set; }
        /// <summary>
        /// The agent radius. [Unit: wu]
        /// </summary>
        public float WalkableRadius { get; set; }
        /// <summary>
        /// The agent maximum traversable ledge. (Up/Down) [Unit: wu]
        /// </summary>
        public float WalkableClimb { get; set; }
        /// <summary>
        /// The xz-plane cell size of the polygon mesh. [Limit: > 0] [Unit: wu]
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// The y-axis cell height of the polygon mesh. [Limit: > 0] [Unit: wu]
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// True if a bounding volume tree should be built for the tile.
        /// @note The BVTree is not normally needed for layered navigation meshes.
        /// </summary>
        public bool BuildBvTree { get; set; }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Find tight heigh bounds, used for culling out off-mesh start locations.
        /// </summary>
        /// <returns>Returns a bounding box</returns>
        public readonly BoundingBox FindBounds()
        {
            float hmin = float.MaxValue;
            float hmax = float.MinValue;

            if (DetailVerts != null && DetailVertsCount > 0)
            {
                for (int i = 0; i < DetailVertsCount; ++i)
                {
                    var h = DetailVerts[i].Y;
                    hmin = Math.Min(hmin, h);
                    hmax = Math.Max(hmax, h);
                }
            }
            else
            {
                for (int i = 0; i < VertCount; ++i)
                {
                    var iv = Verts[i];
                    float h = Bounds.Minimum.Y + iv.Y * CellHeight;
                    hmin = Math.Min(hmin, h);
                    hmax = Math.Max(hmax, h);
                }
            }
            hmin -= WalkableClimb;
            hmax += WalkableClimb;
            var bmin = Bounds.Minimum;
            var bmax = Bounds.Maximum;
            bmin.Y = hmin;
            bmax.Y = hmax;

            return new BoundingBox(bmin, bmax);
        }

        #endregion
    }
}
