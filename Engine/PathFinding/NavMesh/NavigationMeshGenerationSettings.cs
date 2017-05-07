
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
        /// Agent types list
        /// </summary>
        public NavigationMeshAgentType[] Agents = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="NavigationMeshGenerationSettings"/> class from being created.
        /// Use <see cref="Default"/> instead.
        /// </summary>
        public NavigationMeshGenerationSettings()
        {
            this.CellSize = 0.3f;
            this.CellHeight = 0.2f;
            this.MinRegionSize = 8;
            this.MergedRegionSize = 20;
            this.MaxEdgeLength = 12;
            this.MaxEdgeError = 1.8f;
            this.ContourFlags = ContourBuildFlags.None;
            this.VertsPerPoly = 6;
            this.SampleDistance = 6;
            this.MaxSampleError = 1;
            this.BuildBoundingVolumeTree = true;

            this.Agents = new[]
            {
                new NavigationMeshAgentType()
                {
                    MaxClimb = 1f,
                    Height = 2.0f,
                    Radius = 0.6f,
                }
            };
        }

        /// <summary>
        /// Gets the height of the agents traversing the <see cref="NavMesh"/> in voxel (cell) units.
        /// </summary>
        public int GetVoxelAgentHeight(NavigationMeshAgentType agent)
        {
            var vah = (int)(agent.Height / CellHeight);

            return vah == 0 ? 1 : vah;
        }
        /// <summary>
        /// Gets the radius of the agents traversing the <see cref="NavMesh"/> in voxel (cell) units.
        /// </summary>
        public int GetVoxelAgentRadius(NavigationMeshAgentType agent)
        {
            var var = (int)(agent.Radius / CellHeight);

            return var == 0 ? 1 : var;
        }
        /// <summary>
        /// Gets the maximum clim height in voxel (cell) units.
        /// </summary>
        public int GetVoxelMaxClimb(NavigationMeshAgentType agent)
        {
            var vmc = (int)(agent.MaxClimb / CellHeight);

            return vmc == 0 ? 1 : vmc;
        }
    }
}
