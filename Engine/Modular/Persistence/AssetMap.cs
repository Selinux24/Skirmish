using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Scenery assets file configuration
    /// </summary>
    /// <remarks>
    /// Defines a list of reusable assets (rooms, corridor, stairs, etc.) in leves
    /// </remarks>
    public class AssetMap
    {
        /// <summary>
        /// Complex assets configuration
        /// </summary>
        public IEnumerable<Asset> Assets { get; set; } = Enumerable.Empty<Asset>();
        /// <summary>
        /// Maintain texture direction for ceilings and floors, avoiding asset map rotations
        /// </summary>
        public bool MaintainTextureDirection { get; set; } = true;
    }
}
