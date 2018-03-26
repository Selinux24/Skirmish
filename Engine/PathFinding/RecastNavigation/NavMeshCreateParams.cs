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
        public Int3[] verts;
        /// <summary>
        /// The number vertices in the polygon mesh. [Limit: >= 3]
        /// </summary>
        public int vertCount;
        /// <summary>
        /// The polygon data. [Size: #polyCount * 2 * #nvp]
        /// </summary>
        public Polygoni[] polys;
        /// <summary>
        /// The user defined flags assigned to each polygon. [Size: #polyCount]
        /// </summary>
        public SamplePolyFlags[] polyFlags;
        /// <summary>
        /// The user defined area ids assigned to each polygon. [Size: #polyCount]
        /// </summary>
        public SamplePolyAreas[] polyAreas;
        /// <summary>
        /// Number of polygons in the mesh. [Limit: >= 1]
        /// </summary>
        public int polyCount;
        /// <summary>
        /// Number maximum number of vertices per polygon. [Limit: >= 3]
        /// </summary>
        public int nvp;

        #endregion

        #region Height Detail Attributes (Optional)

        // See #rcPolyMeshDetail for details related to these attributes.

        /// <summary>
        /// The height detail sub-mesh data. [Size: 4 * #polyCount]
        /// </summary>
        public Int4[] detailMeshes;
        /// <summary>
        /// The detail mesh vertices. [Size: 3 * #detailVertsCount] [Unit: wu]
        /// </summary>
        public Vector3[] detailVerts;
        /// <summary>
        /// The number of vertices in the detail mesh.
        /// </summary>
        public int detailVertsCount;
        /// <summary>
        /// The detail mesh triangles. [Size: 4 * #detailTriCount]
        /// </summary>
        public Int4[] detailTris;
        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int detailTriCount;

        #endregion

        #region Off-Mesh Connections Attributes (Optional)

        // Used to define a custom point-to-point edge within the navigation graph, an 
        // off-mesh connection is a user defined traversable connection made up to two vertices, 
        // at least one of which resides within a navigation mesh polygon.

        /// <summary>
        /// Off-mesh connections
        /// </summary>
        public OffMeshConnectionDef[] offMeshCon;
        /// <summary>
        /// The number of off-mesh connections. [Limit: >= 0]
        /// </summary>
        public int offMeshConCount;

        #endregion

        #region Tile Attributes

        // The tile grid/layer data can be left at zero if the destination is a single tile mesh.

        /// <summary>
        /// The user defined id of the tile.
        /// </summary>
        public int userId;
        /// <summary>
        /// The tile's x-grid location within the multi-tile destination mesh. (Along the x-axis.)
        /// </summary>
        public int tileX;
        /// <summary>
        /// The tile's y-grid location within the multi-tile desitation mesh. (Along the z-axis.)
        /// </summary>
        public int tileY;
        /// <summary>
        /// The tile's layer within the layered destination mesh. [Limit: >= 0] (Along the y-axis.)
        /// </summary>
        public int tileLayer;
        /// <summary>
        /// The minimum bounds of the tile. [(x, y, z)] [Unit: wu]
        /// </summary>
        public Vector3 bmin;
        /// <summary>
        /// The maximum bounds of the tile. [(x, y, z)] [Unit: wu]
        /// </summary>
        public Vector3 bmax;

        #endregion

        #region General Configuration Attributes

        /// <summary>
        /// The agent height. [Unit: wu]
        /// </summary>
        public float walkableHeight;
        /// <summary>
        /// The agent radius. [Unit: wu]
        /// </summary>
        public float walkableRadius;
        /// <summary>
        /// The agent maximum traversable ledge. (Up/Down) [Unit: wu]
        /// </summary>
        public float walkableClimb;
        /// <summary>
        /// The xz-plane cell size of the polygon mesh. [Limit: > 0] [Unit: wu]
        /// </summary>
        public float cs;
        /// <summary>
        /// The y-axis cell height of the polygon mesh. [Limit: > 0] [Unit: wu]
        /// </summary>
        public float ch;

        /// <summary>
        /// True if a bounding volume tree should be built for the tile.
        /// @note The BVTree is not normally needed for layered navigation meshes.
        /// </summary>
        public bool buildBvTree;

        #endregion
    }
}
