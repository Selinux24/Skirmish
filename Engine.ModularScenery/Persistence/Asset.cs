using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Asset map class
    /// </summary>
    /// <remarks>
    /// Defines a complex asset, like a corridor or a room, with doors and connection points with other complex assets
    /// </remarks>
    public class Asset
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Assets list
        /// </summary>
        public IEnumerable<AssetReference> References { get; set; } = Enumerable.Empty<AssetReference>();
        /// <summary>
        /// Connections list
        /// </summary>
        public IEnumerable<AssetConnection> Connections { get; set; } = Enumerable.Empty<AssetConnection>();
    }
}
