using System.Collections.Generic;
using System.IO;
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
        /// Reads asset map from file
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public static AssetMap FromFile(string contentFolder, string fileName)
        {
            return SerializationHelper.DeserializeFromFile<AssetMap>(Path.Combine(contentFolder, fileName));
        }

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
