
namespace Engine
{
    /// <summary>
    /// Cullable interface
    /// </summary>
    public interface ICullable
    {
        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        bool Cull(ICullingVolume volume, out float distance);
    }
}
