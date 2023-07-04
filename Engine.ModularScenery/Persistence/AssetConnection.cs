using Engine.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Connection between assets
    /// </summary>
    /// <remarks>
    /// Identifies the connection points between assets
    /// </remarks>
    public class AssetConnection
    {
        /// <summary>
        /// Connection type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetConnectionTypes Type { get; set; } = AssetConnectionTypes.None;
        /// <summary>
        /// Position
        /// </summary>
        public Position3 Position { get; set; } = Position3.Zero;
        /// <summary>
        /// Direction
        /// </summary>
        public Direction3 Direction { get; set; } = Direction3.ForwardLH;
    }
}
