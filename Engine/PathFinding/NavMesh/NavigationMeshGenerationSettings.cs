
namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Contains all the settings necessary to convert a mesh to a navmesh.
    /// </summary>
    public class NavigationMeshGenerationSettings : PathFinderSettings
    {
        /// <summary>
        /// Gets the "default" generation settings for a model where 1 unit represents 1 meter.
        /// </summary>
        public static NavigationMeshGenerationSettings Default
        {
            get
            {
                return new NavigationMeshGenerationSettings();
            }
        }

        /// <summary>
        /// Gets or sets the size of a cell in the X and Z axes in world units.
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// Gets or sets the height of a cell in world units.
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// Gets or sets the maximum climb height.
        /// </summary>
        public float MaxClimb { get; set; }
        /// <summary>
        /// Gets or sets the height of the agents traversing the <see cref="NavMesh"/>.
        /// </summary>
        public float AgentHeight { get; set; }
        /// <summary>
        /// Gets or sets the radius of the agents traversing the <see cref="NavMesh"/>.
        /// </summary>
        public float AgentRadius { get; set; }
        /// <summary>
        /// Gets or sets the minimum number of spans that can form a region. Any less than this, and they will be
        /// merged with another region.
        /// </summary>
        public int MinRegionSize { get; set; }
        /// <summary>
        /// Gets or sets the size of the merged regions
        /// </summary>
        public int MergedRegionSize { get; set; }
        /// <summary>
        /// Gets or sets the maximum edge length allowed
        /// </summary>
        public int MaxEdgeLength { get; set; }
        /// <summary>
        /// Gets or sets the maximum error allowed
        /// </summary>
        public float MaxEdgeError { get; set; }
        /// <summary>
        /// Gets or sets the flags that determine how the <see cref="ContourSet"/> is generated.
        /// </summary>
        public ContourBuildFlags ContourFlags { get; set; }
        /// <summary>
        /// Gets or sets the number of vertices a polygon can have.
        /// </summary>
        public int VertsPerPoly { get; set; }
        /// <summary>
        /// Gets or sets the sampling distance for the PolyMeshDetail
        /// </summary>
        public int SampleDistance { get; set; }
        /// <summary>
        /// Gets or sets the maximium error allowed in sampling for the PolyMeshDetail
        /// </summary>
        public int MaxSampleError { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether a bounding volume tree is generated for the mesh.
        /// </summary>
        public bool BuildBoundingVolumeTree { get; set; }
        /// <summary>
        /// Gets the height of the agents traversing the <see cref="NavMesh"/> in voxel (cell) units.
        /// </summary>
        public int VoxelAgentHeight
        {
            get
            {
                return (int)(AgentHeight / CellHeight);
            }
        }
        /// <summary>
        /// Gets the maximum clim height in voxel (cell) units.
        /// </summary>
        public int VoxelMaxClimb
        {
            get
            {
                return (int)(MaxClimb / CellHeight);
            }
        }
        /// <summary>
        /// Gets the radius of the agents traversing the <see cref="NavMesh"/> in voxel (cell) units.
        /// </summary>
        public int VoxelAgentRadius
        {
            get
            {
                return (int)(AgentRadius / CellHeight);
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NavigationMeshGenerationSettings"/> class from being created.
        /// Use <see cref="Default"/> instead.
        /// </summary>
        public NavigationMeshGenerationSettings()
        {
            this.CellSize = 0.3f;
            this.CellHeight = 0.2f;
            this.MaxClimb = 1f;
            this.AgentHeight = 2.0f;
            this.AgentRadius = 0.6f;
            this.MinRegionSize = 8;
            this.MergedRegionSize = 20;
            this.MaxEdgeLength = 12;
            this.MaxEdgeError = 1.8f;
            this.ContourFlags = ContourBuildFlags.None;
            this.VertsPerPoly = 6;
            this.SampleDistance = 6;
            this.MaxSampleError = 1;
            this.BuildBoundingVolumeTree = true;
        }
    }
}
