using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Asset reference
    /// </summary>
    /// <remarks>
    /// Defines how to use an imported asset from a model file, into the parent asset.
    /// </remarks>
    public class AssetReference
    {
        /// <summary>
        /// Asset name in the model
        /// </summary>
        public string AssetName { get; set; }
        /// <summary>
        /// Internal id
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
        public Position3 Position { get; set; } = Position3.Zero;
        /// <summary>
        /// Rotation
        /// </summary>
        public RotationQ Rotation { get; set; } = RotationQ.Identity;
        /// <summary>
        /// Rotation quaternion
        /// </summary>
        /// <summary>
        /// Scale
        /// </summary>
        public Scale3 Scale { get; set; } = Scale3.One;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName};";
        }
    }
}
