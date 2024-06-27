using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Ground interface
    /// </summary>
    public interface IGround
    {
        /// <summary>
        /// Gets the culling volume, if any
        /// </summary>
        /// <returns>Returns a culling volume</returns>
        ICullingVolume GetCullingVolume();
    }
}
