using Engine.Animation;
using Engine.Content.Persistence;
using Engine.Mathematics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Object reference
    /// </summary>
    public class ObjectReference
    {
        const string DefaultStr = "default";

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
        public ObjectAnimationPlan[] AnimationPlans { get; set; } = [];
        /// <summary>
        /// Action list
        /// </summary>
        public ObjectAction[] Actions { get; set; } = [];
        /// <summary>
        /// States list
        /// </summary>
        public ObjectState[] States { get; set; } = [];
        /// <summary>
        /// Next level
        /// </summary>
        /// <remarks>Only for exit doors</remarks>
        public string NextLevel { get; set; }
        /// <summary>
        /// Position vector
        /// </summary>
        public Vector3 Position { get; set; } = Vector3.Zero;
        /// <summary>
        /// Rotation
        /// </summary>
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        /// <summary>
        /// Scale
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.One;
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
        public (string Name, AnimationPlan Plan)[] GetAnimations()
        {
            if (AnimationPlans == null || AnimationPlans.Length == 0)
            {
                return [];
            }

            List<(string Name, AnimationPlan Plan)> res = [];

            foreach (var dPlan in AnimationPlans)
            {
                AnimationPlan plan = new();

                foreach (var dPath in dPlan.Paths)
                {
                    AnimationPath path = new();
                    path.Add(dPath.Name);

                    plan.AddItem(path);
                }

                res.Add((dPlan.Name, plan));
            }

            return [.. res];
        }
        /// <summary>
        /// Gets the default animation plan name
        /// </summary>
        public string GetDefaultAnimationPlanName()
        {
            if (AnimationPlans == null || AnimationPlans.Length == 0)
            {
                return DefaultStr;
            }

            foreach (var a in AnimationPlans)
            {
                if (a != null && a.Default)
                {
                    return a.Name;
                }
            }

            return DefaultStr;
        }
        /// <summary>
        /// Gets the object trigger list
        /// </summary>
        public ItemTrigger[] GetTriggers()
        {
            if (Actions == null || Actions.Length == 0)
            {
                return [];
            }

            List<ItemTrigger> res = [];

            foreach (var action in Actions)
            {
                ItemAction[] acItems = null;
                if ((action.Items?.Length ?? 0) > 0)
                {
                    acItems = new ItemAction[action.Items.Length];
                    for (int i = 0; i < action.Items.Length; i++)
                    {
                        var acItem = action.Items[i];
                        acItems[i] = new ItemAction { Id = acItem.Id, Action = acItem.Action };
                    }
                }

                res.Add(new()
                {
                    Name = action.Name,
                    StateFrom = action.StateFrom,
                    StateTo = action.StateTo,
                    AnimationPlan = action.AnimationPlan,
                    Actions = acItems ?? [],
                });
            }

            return [.. res];
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Type: {Type}; Id: {Id}; AssetName: {AssetName}; AssetMapId: {LevelAssetId}; AssetId: {MapAssetId};";
        }
    }
}
