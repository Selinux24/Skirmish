using Engine.Content;
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
        public AssetTypes Type { get; set; } = AssetTypes.None;
        /// <summary>
        /// Path finding
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PathFindingModes PathFinding { get; set; } = PathFindingModes.None;
        /// <summary>
        /// Position vector
        /// </summary>
        public CntPosition3 Position { get; set; } = CntPosition3.Zero;
        /// <summary>
        /// Rotation
        /// </summary>
        public CntRotationQ Rotation { get; set; } = CntRotationQ.Identity;
        /// <summary>
        /// Rotation quaternion
        /// </summary>
        /// <summary>
        /// Scale
        /// </summary>
        public CntScale3 Scale { get; set; } = CntScale3.One;

        /// <summary>
        /// Gets the initial transform matrix
        /// </summary>
        public CntMatrix4x4 GetTransform()
        {
            return
                CntMatrix4x4.Scaling(Scale) *
                CntMatrix4x4.Rotation(Rotation) *
                CntMatrix4x4.Translation(Position);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName}; PathFinding: {PathFinding};";
        }
    }
}
