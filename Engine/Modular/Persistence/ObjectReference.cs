using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    using Engine.Content;
    using Engine.Content.Persistence;

    /// <summary>
    /// Object reference
    /// </summary>
    public class ObjectReference
    {
        /// <summary>
        /// Asset map id
        /// </summary>
        /// <remarks>
        /// References an id in the asset reference list of the LEVEL
        /// </remarks>
        public string LevelAssetId { get; set; }
        /// <summary>
        /// Internal asset id
        /// </summary>
        /// <remarks>
        /// References an id in the asset reference list of the ASSET MAP
        /// </remarks>
        public string MapAssetId { get; set; }
        /// <summary>
        /// Item id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Asset name in the asset map
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
        public bool PathFinding { get; set; }
        /// <summary>
        /// Animation plan list
        /// </summary>
        public IEnumerable<ObjectAnimationPlan> AnimationPlans { get; set; } = Enumerable.Empty<ObjectAnimationPlan>();
        /// <summary>
        /// Action list
        /// </summary>
        public IEnumerable<ObjectAction> Actions { get; set; } = Enumerable.Empty<ObjectAction>();
        /// <summary>
        /// States list
        /// </summary>
        public IEnumerable<ObjectState> States { get; set; } = Enumerable.Empty<ObjectState>();
        /// <summary>
        /// Next level
        /// </summary>
        /// <remarks>Only for exit doors</remarks>
        public string NextLevel { get; set; }
        /// <summary>
        /// Position vector
        /// </summary>
        public Position3 Position { get; set; } = Position3.Zero;
        /// <summary>
        /// Rotation
        /// </summary>
        public RotationQ Rotation { get; set; } = RotationQ.Identity;
        /// <summary>
        /// Scale
        /// </summary>
        public Scale3 Scale { get; set; } = Scale3.One;
        /// <summary>
        /// Load model lights into scene
        /// </summary>
        public bool LoadLights { get; set; }
        /// <summary>
        /// Lights cast shadows
        /// </summary>
        public bool CastShadows { get; set; }
        /// <summary>
        /// Particle
        /// </summary>
        public ParticleEmitterFile ParticleLight { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName}; AssetMapId: {LevelAssetId}; AssetId: {MapAssetId};";
        }
    }
}
