using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Modular.Persistence
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Object reference
    /// </summary>
    public class ModularSceneryObjectReference
    {
        /// <summary>
        /// Asset map id
        /// </summary>
        public string AssetMapId { get; set; }
        /// <summary>
        /// Asset id
        /// </summary>
        public string AssetId { get; set; }
        /// <summary>
        /// Item id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Asset name
        /// </summary>
        public string AssetName { get; set; }
        /// <summary>
        /// Item type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ModularSceneryObjectTypes Type { get; set; } = ModularSceneryObjectTypes.Default;
        /// <summary>
        /// Include object in path finding
        /// </summary>
        public bool PathFinding { get; set; } = false;
        /// <summary>
        /// Animation plan list
        /// </summary>
        public ModularSceneryObjectAnimationPlan[] AnimationPlans { get; set; } = null;
        /// <summary>
        /// Action list
        /// </summary>
        public ModularSceneryObjectAction[] Actions { get; set; } = null;
        /// <summary>
        /// States list
        /// </summary>
        public ModularSceneryObjectState[] States { get; set; } = null;
        /// <summary>
        /// Next level
        /// </summary>
        /// <remarks>Only for exit doors</remarks>
        public string NextLevel { get; set; }

        /// <summary>
        /// Position vector
        /// </summary>
        public Position3 Position { get; set; } = new Position3(0, 0, 0);
        /// <summary>
        /// Rotation
        /// </summary>
        public RotationQ Rotation { get; set; } = new RotationQ(0, 0, 0, 1);
        /// <summary>
        /// Scale
        /// </summary>
        public Scale3 Scale { get; set; } = new Scale3(1, 1, 1);
        /// <summary>
        /// Load model lights into scene
        /// </summary>
        public bool LoadLights { get; set; } = true;
        /// <summary>
        /// Lights cast shadows
        /// </summary>
        public bool CastShadows { get; set; } = true;
        /// <summary>
        /// Particle
        /// </summary>
        public ParticleEmitterDescription ParticleLight { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName}; AssetMapId: {AssetMapId}; AssetId: {AssetId};";
        }
    }
}
