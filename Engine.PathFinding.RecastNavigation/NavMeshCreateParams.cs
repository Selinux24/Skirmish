using SharpDX;

namespace Engine.PathFinding.RecastNavigation
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
        public Polygoni[] Polys { get; set; }
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
        public int polyCount { get; set; }
        /// <summary>
        /// Number maximum number of vertices per polygon. [Limit: >= 3]
        /// </summary>
        public int nvp { get; set; }

        #endregion

        #region Height Detail Attributes (Optional)

        // See #rcPolyMeshDetail for details related to these attributes.

        /// <summary>
        /// The height detail sub-mesh data. [Size: 4 * #polyCount]
        /// </summary>
        public Int4[] detailMeshes { get; set; }
        /// <summary>
        /// The detail mesh vertices. [Size: 3 * #detailVertsCount] [Unit: wu]
        /// </summary>
        public Vector3[] detailVerts { get; set; }
        /// <summary>
        /// The number of vertices in the detail mesh.
        /// </summary>
        public int detailVertsCount { get; set; }
        /// <summary>
        /// The detail mesh triangles. [Size: 4 * #detailTriCount]
        /// </summary>
        public Int4[] detailTris { get; set; }
        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int detailTriCount { get; set; }

        #endregion

        #region Off-Mesh Connections Attributes (Optional)

        // Used to define a custom point-to-point edge within the navigation graph, an 
        // off-mesh connection is a user defined traversable connection made up to two vertices, 
        // at least one of which resides within a navigation mesh polygon.

        /// <summary>
        /// Off-mesh connections
        /// </summary>
        public IGraphConnection[] offMeshCon { get; set; }
        /// <summary>
        /// The number of off-mesh connections. [Limit: >= 0]
        /// </summary>
        public int offMeshConCount { get; set; }

        #endregion

        #region Tile Attributes

        // The tile grid/layer data can be left at zero if the destination is a single tile mesh.

        /// <summary>
        /// The user defined id of the tile.
        /// </summary>
        public int userId { get; set; }
        /// <summary>
        /// The tile's x-grid location within the multi-tile destination mesh. (Along the x-axis.)
        /// </summary>
        public int tileX { get; set; }
        /// <summary>
        /// The tile's y-grid location within the multi-tile desitation mesh. (Along the z-axis.)
        /// </summary>
        public int tileY { get; set; }
        /// <summary>
        /// The tile's layer within the layered destination mesh. [Limit: >= 0] (Along the y-axis.)
        /// </summary>
        public int tileLayer { get; set; }
        /// <summary>
        /// The minimum bounds of the tile. [(x, y, z)] [Unit: wu]
        /// </summary>
        public Vector3 bmin { get; set; }
        /// <summary>
        /// The maximum bounds of the tile. [(x, y, z)] [Unit: wu]
        /// </summary>
        public Vector3 bmax { get; set; }

        #endregion

        #region General Configuration Attributes

        /// <summary>
        /// The agent height. [Unit: wu]
        /// </summary>
        public float walkableHeight { get; set; }
        /// <summary>
        /// The agent radius. [Unit: wu]
        /// </summary>
        public float walkableRadius { get; set; }
        /// <summary>
        /// The agent maximum traversable ledge. (Up/Down) [Unit: wu]
        /// </summary>
        public float walkableClimb { get; set; }
        /// <summary>
        /// The xz-plane cell size of the polygon mesh. [Limit: > 0] [Unit: wu]
        /// </summary>
        public float cs { get; set; }
        /// <summary>
        /// The y-axis cell height of the polygon mesh. [Limit: > 0] [Unit: wu]
        /// </summary>
        public float ch { get; set; }

        /// <summary>
        /// True if a bounding volume tree should be built for the tile.
        /// @note The BVTree is not normally needed for layered navigation meshes.
        /// </summary>
        public bool buildBvTree { get; set; }

        #endregion
    }
}
