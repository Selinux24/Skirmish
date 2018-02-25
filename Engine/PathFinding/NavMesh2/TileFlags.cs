
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Tile flags used for various functions and fields.
    /// </summary>
    public enum TileFlags
    {
        /// <summary>
        /// The navigation mesh owns the tile memory and is responsible for freeing it.
        /// </summary>
        FreeData = 0x01,
    }
}
