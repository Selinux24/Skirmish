using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    using Engine.Animation;
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
        public ObjectTypes Type { get; set; } = ObjectTypes.Default;
        /// <summary>
        /// Path finding
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PathFindingModes PathFinding { get; set; } = PathFindingModes.None;
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

        /// <summary>
        /// Gets the animation plan list
        /// </summary>
        public IEnumerable<(string Name, AnimationPlan Plan)> GetAnimations()
        {
            if ((AnimationPlans?.Any()) != true)
            {
                yield break;
            }

            foreach (var dPlan in AnimationPlans)
            {
                AnimationPlan plan = new();

                foreach (var dPath in dPlan.Paths)
                {
                    AnimationPath path = new();
                    path.Add(dPath.Name);

                    plan.AddItem(path);
                }

                yield return (dPlan.Name, plan);
            }
        }
        /// <summary>
        /// Gets the default animation plan name
        /// </summary>
        public string GetDefaultAnimationPlanName()
        {
            return AnimationPlans?.FirstOrDefault(a => a.Default)?.Name ?? "default";
        }
        /// <summary>
        /// Gets the object trigger list
        /// </summary>
        public IEnumerable<ItemTrigger> GetTriggers()
        {
            if ((Actions?.Any()) != true)
            {
                yield break;
            }

            foreach (var action in Actions)
            {
                yield return new()
                {
                    Name = action.Name,
                    StateFrom = action.StateFrom,
                    StateTo = action.StateTo,
                    AnimationPlan = action.AnimationPlan,
                    Actions = action.Items.Select(i => new ItemAction { Id = i.Id, Action = i.Action }),
                };
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName}; AssetMapId: {LevelAssetId}; AssetId: {MapAssetId};";
        }
    }
}
