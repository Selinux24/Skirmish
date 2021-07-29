using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Modular.Persistence
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Connection between assets
    /// </summary>
    public class ModularSceneryAssetConnection
    {
        /// <summary>
        /// Connection type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ModularSceneryAssetConnectionTypes Type { get; set; } = ModularSceneryAssetConnectionTypes.None;
        /// <summary>
        /// Position
        /// </summary>
        public Position3 Position { get; set; } = new Position3(0, 0, 0);
        /// <summary>
        /// Direction
        /// </summary>
        public Direction3 Direction { get; set; } = new Direction3(0, 0, 0);
    }
}
