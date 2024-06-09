
namespace TerrainSamples.SceneNavMeshTest
{
    enum States
    {
        /// <summary>
        /// Default state
        /// </summary>
        Default,
        /// <summary>
        /// Mesh
        /// </summary>
        Mesh,
        /// <summary>
        /// Agent parameters
        /// </summary>
        MeshAgent,
        /// <summary>
        /// Navmesh parameters
        /// </summary>
        MeshNavMesh,
        /// <summary>
        /// Rasterizer
        /// </summary>
        Rasterizer,
        /// <summary>
        /// Tile management
        /// </summary>
        Tiles,
        /// <summary>
        /// Obstable adding
        /// </summary>
        AddObstacle,
        /// <summary>
        /// Area adding
        /// </summary>
        AddArea,
        /// <summary>
        /// Connection adding
        /// </summary>
        AddConnection,
        /// <summary>
        /// Path finding
        /// </summary>
        PathFinding,
        /// <summary>
        /// Debug drawing selector state
        /// </summary>
        Debug,
    }
}
