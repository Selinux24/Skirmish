
namespace Engine.Modular
{
    /// <summary>
    /// Object path finding configuration
    /// </summary>
    public enum ModularSceneryPathFindingModes
    {
        /// <summary>
        /// Not used
        /// </summary>
        None = 0,
        /// <summary>
        /// Use the object's OBB
        /// </summary>
        Coarse = 1,
        /// <summary>
        /// Use the object's linked hull
        /// </summary>
        Hull = 2,
        /// <summary>
        /// Use the object's triangle list
        /// </summary>
        Geometry = 3,
    }
}
