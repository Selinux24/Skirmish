using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Modular.Persistence
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Asset reference
    /// </summary>
    public class ModularSceneryAssetReference
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string AssetName { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ModularSceneryAssetTypes Type { get; set; } = ModularSceneryAssetTypes.None;
        /// <summary>
        /// Position vector
        /// </summary>
        public Position3 Position { get; set; } = new Position3(0, 0, 0);
        /// <summary>
        /// Rotation
        /// </summary>
        public RotationQ Rotation { get; set; } = new RotationQ(0, 0, 0, 1);
        /// <summary>
        /// Rotation quaternion
        /// </summary>
        /// <summary>
        /// Scale
        /// </summary>
        public Scale3 Scale { get; set; } = new Scale3(1, 1, 1);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName};";
        }
    }
}
