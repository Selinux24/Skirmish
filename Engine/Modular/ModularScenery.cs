using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Modular
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Content;
    using Engine.Modular.Persistence;

    /// <summary>
    /// Terrain model
    /// </summary>
    public sealed class ModularScenery : Ground<ModularSceneryDescription>
    {
        /// <summary>
        /// Objects auto identifier counter
        /// </summary>
        private static int ObjectsAutoId = 1;
        /// <summary>
        /// Objects base string for identification build
        /// </summary>
        private static readonly string ObjectsAutoString = "__objauto__";

        /// <summary>
        /// Asset models dictionary
        /// </summary>
        private readonly Dictionary<string, ModelInstanced> assets = new();
        /// <summary>
        /// Object models dictionary
        /// </summary>
        private readonly Dictionary<string, ModelInstanced> objects = new();
        /// <summary>
        /// Particle descriptors dictionary
        /// </summary>
        private readonly Dictionary<string, ParticleSystemDescription> particleDescriptors = new();
        /// <summary>
        /// Particle manager
        /// </summary>
        private ParticleManager particleManager = null;
        /// <summary>
        /// Asset map
        /// </summary>
        private AssetMapIntersections assetMapIntersections = null;
        /// <summary>
        /// Scenery entities
        /// </summary>
        private readonly List<ModularSceneryItem> entities = new();
        /// <summary>
        /// Triggers list by instance
        /// </summary>
        private readonly Dictionary<ModelInstance, List<ModularSceneryTrigger>> triggers = new();
        /// <summary>
        /// Animations plan dictionary by instance
        /// </summary>
        private readonly Dictionary<ModelInstance, Dictionary<string, AnimationPlan>> animations = new();
        /// <summary>
        /// Active trigger callbacks
        /// </summary>
        private readonly List<TriggerCallback> activeCallbacks = new();
        /// <summary>
        /// Gets the assets description
        /// </summary>
        private AssetMap assetMap;
        /// <summary>
        /// Gets the levels map
        /// </summary>
        private LevelMap levelMap;

        /// <summary>
        /// First level
        /// </summary>
        public Level FirstLevel
        {
            get
            {
                return levelMap.Levels.FirstOrDefault();
            }
        }
        /// <summary>
        /// Current level
        /// </summary>
        public Level CurrentLevel { get; set; }

        /// <summary>
        /// Trigger starts it's execution event
        /// </summary>
        public event ModularSceneryTriggerStartHandler TriggerStart;
        /// <summary>
        /// Trigger ends it's execution event
        /// </summary>
        public event ModularSceneryTriggerEndHandler TriggerEnd;

        /// <summary>
        /// Populate objects empty ids
        /// </summary>
        /// <param name="level">Level definition</param>
        private static void PopulateObjectIds(Level level)
        {
            if (level.Objects?.Any() != true)
            {
                return;
            }

            foreach (var obj in level.Objects)
            {
                if (!string.IsNullOrEmpty(obj.Id))
                {
                    continue;
                }

                obj.Id = $"{ObjectsAutoString}_{GetNextObjectId()}";
            }
        }
        /// <summary>
        /// Gets the next Id
        /// </summary>
        /// <returns>Returns the next Id</returns>
        private static int GetNextObjectId()
        {
            return ++ObjectsAutoId;
        }
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        private static Dictionary<string, InstanceInfo> GetMapInstanceCounters(Level level, IEnumerable<Asset> assets)
        {
            Dictionary<string, InstanceInfo> res = new();

            var vAssets = assets.ToArray();

            foreach (var item in level.Map)
            {
                var asset = Array.Find(vAssets, a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase));
                if (asset == null)
                {
                    continue;
                }

                var assetInstances = GetInstanceCounters(asset);
                foreach (var key in assetInstances.Keys)
                {
                    var assetInstance = assetInstances[key];

                    if (!res.ContainsKey(key))
                    {
                        res.Add(key, new InstanceInfo { Count = assetInstance.Count, PathFinding = assetInstance.PathFinding });

                        continue;
                    }

                    res[key].Count += assetInstance.Count;
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the instance count dictionary
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <returns>Returns a dictionary that contains the instance count by asset name</returns>
        private static Dictionary<string, (int Count, ModularSceneryPathFindingModes PathFinding)> GetInstanceCounters(Asset asset)
        {
            Dictionary<string, (int, ModularSceneryPathFindingModes)> res = new();

            var assetNames = asset.References.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assetNames)
            {
                var count = asset.References.Count(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase));
                if (count > 0)
                {
                    var pf = asset.References.First(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase)).PathFinding;

                    res.Add(assetName, (count, pf));
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the instance transforms dictionary
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <returns>Returns a dictionary that contains the instance transform list by asset name</returns>
        private static Dictionary<string, Matrix[]> GetInstanceTransforms(Asset asset)
        {
            Dictionary<string, Matrix[]> res = new();

            var assetNames = asset.References.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assetNames)
            {
                var transforms = asset.References
                    .Where(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                    .Select(a => GeometryUtil.Transformation(a.Position, a.Rotation, a.Scale)).ToArray();

                res.Add(assetName, transforms);
            }

            return res;
        }
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        private static Dictionary<string, InstanceInfo> GetObjectsInstanceCounters(Level level)
        {
            Dictionary<string, InstanceInfo> res = new();

            foreach (var item in level.Objects)
            {
                if (string.IsNullOrEmpty(item.AssetName))
                {
                    continue;
                }

                if (!res.ContainsKey(item.AssetName))
                {
                    res.Add(item.AssetName, new InstanceInfo { Count = 0, PathFinding = item.PathFinding });
                }

                res[item.AssetName].Count += 1;
            }

            return res;
        }
        /// <summary>
        /// Finds the asset reference by asset map id and asset id
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="assets">Asset list</param>
        /// <param name="levelAssetId">Asset reference id in the level asset reference list</param>
        /// <param name="mapAssetId">Asset id in the asset map reference list</param>
        /// <returns>Returns the asset reference</returns>
        private static AssetReference FindAssetReference(Level level, IEnumerable<Asset> assets, string levelAssetId, string mapAssetId)
        {
            var res = assets
                //Search any asset wich contains a reference with the specified level, by asset name or by the level asset map id
                .Where(a => level.Map.Any(r =>
                    string.Equals(r.AssetName, a.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.Id, levelAssetId, StringComparison.OrdinalIgnoreCase)));
            var res2 = res
                //Then, get the first reference wich matching reference Id with the level asset id
                .Select(a => a.References.FirstOrDefault(r => string.Equals(r.Id, mapAssetId, StringComparison.OrdinalIgnoreCase)));
            var res3 = res2
                .FirstOrDefault();

            return res3;
        }
        /// <summary>
        /// Gets the first index of the asset in the current configuration
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="assets">Asset list</param>
        /// <param name="assetName">Asset name</param>
        /// <param name="levelAssetId">Asset reference id in the level asset reference list</param>
        /// <param name="mapAssetId">Asset id in the asset map reference list</param>
        /// <returns>Returns the first index</returns>
        private static int GetMapInstanceIndex(Level level, IEnumerable<Asset> assets, string assetName, string levelAssetId, string mapAssetId)
        {
            int index = 0;

            foreach (var item in level.Map)
            {
                var asset = assets
                    .FirstOrDefault(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase));

                if (asset == null)
                {
                    continue;
                }

                foreach (var a in asset.References.Where(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (string.Equals(item.Id, levelAssetId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(a.Id, mapAssetId, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }

                    index++;
                }
            }

            return -1;
        }
        /// <summary>
        /// Gets the first index of the object in the current configuration
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="assetName">Asset name</param>
        /// <param name="objectId">Object id</param>
        /// <returns>Returns the first index</returns>
        private static int GetObjectInstanceIndex(Level level, string assetName, string objectId)
        {
            int index = 0;

            foreach (var item in level.Objects)
            {
                if (string.IsNullOrEmpty(item.AssetName))
                {
                    continue;
                }

                if (string.Equals(item.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(item.Id, objectId, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }

                    index++;
                }
            }

            return -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public ModularScenery(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(ModularSceneryDescription description)
        {
            await base.InitializeAssets(description);

            assetMap = Description.GetAssetMap();

            levelMap = Description.GetLevelMap();
        }

        /// <summary>
        /// Loads the first level
        /// </summary>
        /// <param name="progress">Resource loading progress updater</param>
        public async Task LoadFirstLevel(IProgress<LoadResourceProgress> progress = null)
        {
            await LoadLevel(levelMap.Levels.FirstOrDefault(), progress);
        }
        /// <summary>
        /// Loads the level by name
        /// </summary>
        /// <param name="levelName">Level name</param>
        /// <param name="progress">Resource loading progress updater</param>
        public async Task LoadLevel(string levelName, IProgress<LoadResourceProgress> progress = null)
        {
            if (string.Equals(CurrentLevel.Name, levelName, StringComparison.OrdinalIgnoreCase))
            {
                //Same level
                return;
            }

            //Find the level
            var level = levelMap.Levels.FirstOrDefault(l => string.Equals(l.Name, levelName, StringComparison.OrdinalIgnoreCase));
            if (level != null)
            {
                //Load the level
                await LoadLevel(level, progress);
            }
        }
        /// <summary>
        /// Loads a level
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="progress">Resource loading progress updater</param>
        private async Task LoadLevel(Level level, IProgress<LoadResourceProgress> progress = null)
        {
            //Removes previous level components from scene
            Scene.Components.RemoveComponents(assets.Select(a => a.Value));
            Scene.Components.RemoveComponents(objects.Select(o => o.Value));

            //Clear internal lists and data
            assets.Clear();
            objects.Clear();
            entities.Clear();
            assetMapIntersections = null;
            particleManager?.Clear();
            particleDescriptors.Clear();

            CurrentLevel = level;

            var contentLibrary = new ContentLibrary(await Description.ReadContentData());

            await InitializeParticles(progress);
            await InitializeAssets(level, contentLibrary, progress);
            await InitializeObjects(level, contentLibrary, progress);

            ParseAssetsMap(level, progress);

            InitializeEntities(level, progress);
        }
        /// <summary>
        /// Initialize the particle system and the particle descriptions
        /// </summary>
        private async Task InitializeParticles(IProgress<LoadResourceProgress> progress = null)
        {
            if (levelMap.ParticleSystems?.Any() != true)
            {
                progress?.Report(new LoadResourceProgress { Progress = 1 });

                return;
            }

            string modelId = $"{Name ?? nameof(ModularScenery)}.Particle Manager";

            particleManager = await Scene.AddComponentEffect<ParticleManager, ParticleManagerDescription>(
                modelId,
                Name,
                ParticleManagerDescription.Default(),
                98);

            float total = levelMap.ParticleSystems.Count();
            int current = 0;

            foreach (var item in levelMap.ParticleSystems)
            {
                try
                {
                    string contentPath = item.ContentPath ?? Description.Content.ContentFolder;

                    var pDesc = ParticleSystemDescription.Initialize(item, contentPath);

                    particleDescriptors.Add(item.Name, pDesc);
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"{nameof(ModularScenery)}. Error loading particle system {item.Name}: {ex.Message}", ex);
                }

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
            }
        }
        /// <summary>
        /// Initialize all assets into asset dictionary 
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="contentLibrary">Assets model content</param>
        /// <param name="progress">Resource loading progress updater</param>
        private async Task InitializeAssets(Level level, ContentLibrary contentLibrary, IProgress<LoadResourceProgress> progress = null)
        {
            // Get instance count for all single geometries from Map
            var instances = GetMapInstanceCounters(level, assetMap.Assets);

            float total = instances.Keys.Count;
            int current = 0;

            // Load all single geometries into single instanced model components
            foreach (var assetName in instances.Keys)
            {
                int count = instances[assetName].Count;
                if (count <= 0)
                {
                    continue;
                }

                var modelContent = contentLibrary.GetContentDataByName(assetName);
                if (modelContent == null)
                {
                    continue;
                }

                var model = await InitializeAsset(assetName, count, level, modelContent, instances[assetName].PathFinding);
                if (model == null)
                {
                    continue;
                }

                assets.Add(assetName, model);

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
            }
        }
        /// <summary>
        /// Creates a new instanced model for the asset
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="count">Instance count</param>
        /// <param name="level">Level</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="pathFinding">Path finding</param>
        private async Task<ModelInstanced> InitializeAsset(string assetName, int count, Level level, ContentData modelContent, ModularSceneryPathFindingModes pathFinding)
        {
            var assetId = $"{Name ?? nameof(ModularScenery)}.Asset.{assetName}.{level.Name}";

            try
            {
                var usage = pathFinding switch
                {
                    ModularSceneryPathFindingModes.None => SceneObjectUsages.None,
                    ModularSceneryPathFindingModes.Coarse => SceneObjectUsages.Object,
                    ModularSceneryPathFindingModes.Hull => SceneObjectUsages.Object,
                    ModularSceneryPathFindingModes.Geometry => SceneObjectUsages.Object,
                    _ => SceneObjectUsages.None,
                };

                var desc = new ModelInstancedDescription()
                {
                    CastShadow = Description.CastShadow,
                    UseAnisotropicFiltering = Description.UseAnisotropic,
                    Instances = count,
                    LoadAnimation = false,
                    BlendMode = Description.BlendMode,
                    Content = ContentDescription.FromContentData(modelContent),
                };

                var model = await Scene.AddComponent<ModelInstanced, ModelInstancedDescription>(assetId, Name, desc, usage);

                model.Owner = this;

                return model;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{nameof(ModularScenery)}. Error loading asset {assetId}: {ex.Message}", ex);

                throw;
            }
        }
        /// <summary>
        /// Initialize all objects into asset dictionary 
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="contentLibrary">Assets model content</param>
        /// <param name="progress">Resource loading progress updater</param>
        private async Task InitializeObjects(Level level, ContentLibrary contentLibrary, IProgress<LoadResourceProgress> progress = null)
        {
            // Set auto-identifiers
            PopulateObjectIds(level);

            // Get instance count for all single geometries from Map
            var instances = GetObjectsInstanceCounters(level);

            float total = instances.Keys.Count;
            int current = 0;

            // Load all single geometries into single instanced model components
            foreach (var assetName in instances.Keys)
            {
                var count = instances[assetName].Count;
                if (count <= 0)
                {
                    continue;
                }

                var modelContent = contentLibrary.GetContentDataByName(assetName);
                if (modelContent == null)
                {
                    continue;
                }

                var model = await InitializeObject(assetName, count, level, modelContent, instances[assetName].PathFinding);
                if (model == null)
                {
                    continue;
                }

                objects.Add(assetName, model);

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
            }
        }
        /// <summary>
        /// Creates a new instanced model for the object
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="count">Instance count</param>
        /// <param name="level">Level</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="pathFinding">Path finding enabled flag</param>
        private async Task<ModelInstanced> InitializeObject(string assetName, int count, Level level, ContentData modelContent, ModularSceneryPathFindingModes pathFinding)
        {
            var modelId = $"{Name ?? nameof(ModularScenery)}.{assetName}.{level.Name}";

            try
            {
                var usage = pathFinding switch
                {
                    ModularSceneryPathFindingModes.None => SceneObjectUsages.None,
                    ModularSceneryPathFindingModes.Coarse => SceneObjectUsages.Object,
                    ModularSceneryPathFindingModes.Hull => SceneObjectUsages.Object,
                    ModularSceneryPathFindingModes.Geometry => SceneObjectUsages.Object,
                    _ => SceneObjectUsages.None,
                };

                var model = await Scene.AddComponent<ModelInstanced, ModelInstancedDescription>(
                    modelId,
                    Name,
                    new ModelInstancedDescription()
                    {
                        CastShadow = Description.CastShadow,
                        UseAnisotropicFiltering = Description.UseAnisotropic,
                        Instances = count,
                        BlendMode = Description.BlendMode,
                        Content = ContentDescription.FromContentData(modelContent),
                    },
                    usage);

                model.Owner = this;

                //Get the object list to process
                var objList = level.Objects
                    .Where(o => string.Equals(o.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                //Positioning
                var transforms = objList.Select(o => GeometryUtil.Transformation(o.Position, o.Rotation, o.Scale));
                model.SetTransforms(transforms);

                //Lights
                for (int i = 0; i < model.InstanceCount; i++)
                {
                    await InitializeObjectLights(objList[i], model[i]);

                    InitializeObjectAnimations(objList[i], model[i]);
                }

                return model;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"{nameof(ModularScenery)}. Error loading object {modelId}: {ex.Message}", ex);

                throw;
            }
        }
        /// <summary>
        /// Initialize lights attached to the specified object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="instance">Model instance</param>
        private async Task InitializeObjectLights(ObjectReference obj, ModelInstance instance)
        {
            if (!obj.LoadLights)
            {
                return;
            }

            var trn = instance.Manipulator.LocalTransform;

            var lights = instance.Lights;
            if (lights?.Any() != true)
            {
                return;
            }

            foreach (var light in lights)
            {
                light.CastShadow = obj.CastShadows;

                if (obj.ParticleLight == null)
                {
                    continue;
                }

                Vector3 lightPosition;
                if (light is SceneLightPoint pointL)
                {
                    lightPosition = pointL.Position;
                }
                else if (light is SceneLightSpot spotL)
                {
                    lightPosition = spotL.Position;
                }
                else
                {
                    Logger.WriteWarning(this, $"{nameof(ModularScenery)}. Light type not allowed {light.GetType()}");

                    continue;
                }

                var emitter = new ParticleEmitter(obj.ParticleLight)
                {
                    Position = Vector3.TransformCoordinate(lightPosition, trn),
                    Instance = instance,
                };

                await particleManager.AddParticleSystem(
                    ParticleSystemTypes.CPU,
                    particleDescriptors[obj.ParticleLight.Name],
                    emitter);
            }

            Scene.Lights.AddRange(lights);
        }
        /// <summary>
        /// Initialize animations and triggers
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="instance">Model instance</param>
        private void InitializeObjectAnimations(ObjectReference obj, ModelInstance instance)
        {
            Dictionary<string, AnimationPlan> animationDict = new();

            //Plans
            if (obj.AnimationPlans?.Any() == true)
            {
                foreach (var dPlan in obj.AnimationPlans)
                {
                    AnimationPlan plan = new();

                    foreach (var dPath in dPlan.Paths)
                    {
                        AnimationPath path = new();
                        path.Add(dPath.Name);

                        plan.AddItem(path);
                    }

                    animationDict.Add(dPlan.Name, plan);
                }
            }

            if (animationDict.Count > 0)
            {
                animations.Add(instance, animationDict);

                var defaultPlan = obj.AnimationPlans.FirstOrDefault(a => a.Default)?.Name ?? "default";

                AnimationPath def = new();
                def.Add(defaultPlan);

                instance.AnimationController.ReplacePlan(new AnimationPlan(def));
                instance.InvalidateCache();
            }

            List<ModularSceneryTrigger> instanceTriggers = new();

            //Actions
            if (obj.Actions?.Any() == true)
            {
                foreach (var action in obj.Actions)
                {
                    ModularSceneryTrigger trigger = new()
                    {
                        Name = action.Name,
                        StateFrom = action.StateFrom,
                        StateTo = action.StateTo,
                        AnimationPlan = action.AnimationPlan,
                    };

                    foreach (var item in action.Items)
                    {
                        ModularSceneryAction act = new()
                        {
                            ItemId = item.Id,
                            ItemAction = item.Action,
                        };

                        trigger.Actions.Add(act);
                    }

                    instanceTriggers.Add(trigger);
                }
            }

            if (instanceTriggers.Count > 0)
            {
                triggers.Add(instance, instanceTriggers);
            }
        }
        /// <summary>
        /// Initialize scenery entities proxy list
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="progress">Resource loading progress updater</param>
        private void InitializeEntities(Level level, IProgress<LoadResourceProgress> progress = null)
        {
            if (level?.Objects?.Any() != true)
            {
                progress?.Report(new LoadResourceProgress { Progress = 1 });

                return;
            }

            float total = level.Objects.Count();
            int current = 0;

            foreach (var obj in level.Objects)
            {
                try
                {
                    ModelInstance instance;

                    if (string.IsNullOrEmpty(obj.AssetName))
                    {
                        // Adding object with referenced geometry
                        instance = FindAssetModelInstance(obj.LevelAssetId, obj.MapAssetId);
                    }
                    else
                    {
                        // Adding object with it's own geometry
                        instance = FindObjectInstance(obj.AssetName, obj.Id);
                    }

                    if (instance != null)
                    {
                        //Find emitters
                        var emitters = particleManager?.ParticleSystems
                            .Where(p => p.Emitter.Instance == instance)
                            .Select(p => p.Emitter)
                            .ToArray();

                        //Find first state
                        var defaultState = obj.States?.FirstOrDefault()?.Name;

                        entities.Add(new ModularSceneryItem(obj, instance, emitters, defaultState));
                    }

                    progress?.Report(new LoadResourceProgress { Progress = ++current / total });
                }
                catch (Exception ex)
                {
                    Logger.WriteError(this, $"{obj.Id}: {obj.AssetName}/{obj.LevelAssetId}.{obj.MapAssetId}");
                    Logger.WriteError(this, ex.ToString(), ex);

                    throw;
                }
            }
        }
        /// <summary>
        /// Parse the assets map to set the assets transforms
        /// </summary>
        /// <param name="level">Level definition</param>
        /// <param name="progress">Resource loading progress updater</param>
        private void ParseAssetsMap(Level level, IProgress<LoadResourceProgress> progress = null)
        {
            assetMapIntersections = new AssetMapIntersections();

            var transforms = new Dictionary<string, List<Matrix>>();

            float total = level.Map.Count() + transforms.Keys.Count;
            int current = 0;

            var vAssets = assetMap.Assets.ToArray();

            // Paser map for instance positioning
            foreach (var item in level.Map)
            {
                var assetIndex = Array.FindIndex(vAssets, a => a.Name == item.AssetName);
                if (assetIndex < 0)
                {
                    throw new EngineException($"Modular Scenery asset not found: {item.AssetName}");
                }

                ParseAssetReference(item, assetIndex, transforms);

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
            }

            foreach (var assetName in transforms.Keys)
            {
                assets[assetName].SetTransforms(transforms[assetName]);

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
            }

            assetMapIntersections.Build(assetMap, assets);
        }
        /// <summary>
        /// Parses the specified asset reference
        /// </summary>
        /// <param name="item">Reference</param>
        /// <param name="assetIndex">Asset index</param>
        /// <param name="transforms">Transforms dictionary</param>
        private void ParseAssetReference(AssetReference item, int assetIndex, Dictionary<string, List<Matrix>> transforms)
        {
            var complexAssetTransform = GeometryUtil.Transformation(item.Position, item.Rotation, item.Scale);
            var complexAssetRotation = item.Rotation;

            AssetMapItem aMap = new()
            {
                Index = assetIndex,
                Name = item.AssetName,
                Transform = complexAssetTransform,
                Assets = new Dictionary<string, List<int>>(),
            };
            assetMapIntersections.Add(aMap);

            var asset = assetMap.Assets.ElementAt(assetIndex);
            var assetTransforms = GetInstanceTransforms(asset);

            foreach (var basicAsset in assetTransforms.Keys)
            {
                if (!transforms.ContainsKey(basicAsset))
                {
                    transforms.Add(basicAsset, new List<Matrix>());
                }

                if (!aMap.Assets.ContainsKey(basicAsset))
                {
                    aMap.Assets.Add(basicAsset, new List<int>());
                }

                //Get basic asset type
                var basicAssetType = asset.References.First(a => a.AssetName == basicAsset).Type;

                Array.ForEach(assetTransforms[basicAsset], t =>
                {
                    var basicTrn = t;

                    if (assetMap.MaintainTextureDirection)
                    {
                        var maintain =
                            basicAssetType == ModularSceneryAssetTypes.Floor ||
                            basicAssetType == ModularSceneryAssetTypes.Ceiling;
                        if (maintain)
                        {
                            //Invert complex asset rotation
                            basicTrn = Matrix.RotationQuaternion(Quaternion.Invert(complexAssetRotation)) * t;
                        }
                    }

                    aMap.Assets[basicAsset].Add(transforms[basicAsset].Count);
                    transforms[basicAsset].Add(basicTrn * complexAssetTransform);
                });
            }
        }
        /// <summary>
        /// Finds the model instance for the specified asset map id and asset id
        /// </summary>
        /// <param name="levelAssetId">Asset id in the level asset reference list</param>
        /// <param name="mapAssetId">Asset id in the asset map reference list</param>
        /// <returns>Returns the model instance</returns>
        private ModelInstance FindAssetModelInstance(string levelAssetId, string mapAssetId)
        {
            // Find the assetName by object asset_id
            var res = FindAssetReference(CurrentLevel, assetMap.Assets, levelAssetId, mapAssetId);
            if (res != null)
            {
                // Look for all geometry references
                int index = GetMapInstanceIndex(CurrentLevel, assetMap.Assets, res.AssetName, levelAssetId, mapAssetId);
                if (index >= 0)
                {
                    return assets[res.AssetName][index];
                }
            }

            return null;
        }
        /// <summary>
        /// Finds the model instance for the specified object asset name and object id
        /// </summary>
        /// <param name="assetName">Object asset name</param>
        /// <param name="id">Object id</param>
        /// <returns>Returns the model instance</returns>
        private ModelInstance FindObjectInstance(string assetName, string id)
        {
            var index = GetObjectInstanceIndex(CurrentLevel, assetName, id);
            if (index >= 0)
            {
                return objects[assetName][index];
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            assetMapIntersections?.Update(context.CameraVolume);

            if (activeCallbacks?.Any() == true)
            {
                UpdateTriggers();
            }
        }
        /// <summary>
        /// Verifies the active triggers states and fires the ending events
        /// </summary>
        private void UpdateTriggers()
        {
            if (!activeCallbacks.Any())
            {
                return;
            }

            activeCallbacks.ForEach(c =>
            {
                if (c.Waiting)
                {
                    return;
                }

                TriggerEnd?.Invoke(this, new ModularSceneryTriggerEventArgs()
                {
                    StarterTrigger = c.Trigger,
                    StarterItem = c.Item,
                    Items = c.Items,
                });
            });

            activeCallbacks.RemoveAll(c => !c.Waiting);
        }

        /// <inheritdoc/>
        public override BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            var res = new BoundingSphere();
            bool initialized = false;

            foreach (var item in objects.Keys)
            {
                var model = objects[item];
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < model.InstanceCount; i++)
                {
                    var bsph = model[i].GetBoundingSphere(refresh);

                    if (!initialized)
                    {
                        res = bsph;
                        initialized = true;
                    }
                    else
                    {
                        res = BoundingSphere.Merge(res, bsph);
                    }
                }
            }

            return res;
        }
        /// <inheritdoc/>
        public override BoundingBox GetBoundingBox(bool refresh = false)
        {
            var res = new BoundingBox();
            bool initialized = false;

            foreach (var item in objects.Keys)
            {
                var model = objects[item];
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < model.InstanceCount; i++)
                {
                    var bbox = model[i].GetBoundingBox(refresh);

                    if (!initialized)
                    {
                        res = bbox;
                        initialized = true;
                    }
                    else
                    {
                        res = BoundingBox.Merge(res, bbox);
                    }
                }
            }

            return res;
        }
        /// <inheritdoc/>
        public override IEnumerable<Triangle> GetPickingHull(PickingHullTypes geometryType)
        {
            List<Triangle> triangles = new();

            var assetTriangles = assets.Values
                .SelectMany(asset => asset.GetInstances().Where(i => i.Visible))
                .SelectMany(instance => instance.GetPickingHull(geometryType));

            triangles.AddRange(assetTriangles);

            var objTriangles = objects.Values
                .SelectMany(obj => obj.GetInstances().Where(i => i.Visible))
                .SelectMany(instance => instance.GetPickingHull(geometryType));

            triangles.AddRange(objTriangles);

            return triangles.ToArray();
        }

        /// <summary>
        /// Gets all complex map asset volumes
        /// </summary>
        /// <returns>Returns a dictionary of complex asset volumes by asset name</returns>
        public Dictionary<string, List<BoundingBox>> GetMapVolumes()
        {
            return assetMapIntersections.GetMapVolumes();
        }
        /// <summary>
        /// Gets all individual map asset volumes
        /// </summary>
        /// <returns>Returns a dictionary of individual asset volumes by asset name</returns>
        public Dictionary<string, List<BoundingBox>> GetMapAssetsVolumes()
        {
            var res = new Dictionary<string, List<BoundingBox>>();

            foreach (var item in assets.Keys)
            {
                res.Add(item, new List<BoundingBox>());

                for (int i = 0; i < assets[item].InstanceCount; i++)
                {
                    res[item].Add(assets[item][i].GetBoundingBox());
                }
            }

            return res;
        }
        /// <summary>
        /// Gets all objects volumes
        /// </summary>
        /// <returns>Returns a dictionary of object volumes by object name</returns>
        public Dictionary<string, List<BoundingBox>> GetObjectVolumes()
        {
            var res = new Dictionary<string, List<BoundingBox>>();

            foreach (var item in objects.Keys)
            {
                var model = objects[item];
                if (model != null)
                {
                    res.Add(item, new List<BoundingBox>());

                    for (int i = 0; i < model.InstanceCount; i++)
                    {
                        res[item].Add(model[i].GetBoundingBox());
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Gets the specified object by id
        /// </summary>
        /// <param name="id">Object id</param>
        /// <returns>Returns the specified object by id</returns>
        public ModularSceneryItem GetObjectById(string id)
        {
            var obj = entities.Find(o => string.Equals(o.Object.Id, id, StringComparison.OrdinalIgnoreCase));

            return obj;
        }

        /// <summary>
        /// Get objects by name
        /// </summary>
        /// <param name="name">Object name</param>
        /// <returns>Returns a list of objects by name</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsByName(string name)
        {
            var res = entities
                .Where(o => string.Equals(o.Object.AssetName, name, StringComparison.OrdinalIgnoreCase));

            return res.ToArray();
        }
        /// <summary>
        /// Gets objects by type
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <returns>Returns a list of objects of the specified type</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsByType(ModularSceneryObjectTypes objectType)
        {
            var res = entities
                .Where(o => objectType.HasFlag(o.Object.Type));

            return res.ToArray();
        }

        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsInVolume(BoundingBox bbox, bool useSphere, bool sortByDistance)
        {
            return GetObjects(bbox, null, useSphere, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsInVolume(BoundingBox bbox, ModularSceneryObjectTypes filter, bool useSphere, bool sortByDistance)
        {
            return GetObjects(bbox, filter, useSphere, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsInVolume(BoundingSphere sphere, bool useSphere, bool sortByDistance)
        {
            return GetObjects(sphere, null, useSphere, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsInVolume(BoundingSphere sphere, ModularSceneryObjectTypes filter, bool useSphere, bool sortByDistance)
        {
            return GetObjects(sphere, filter, useSphere, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsInVolume(BoundingFrustum frustum, bool useSphere, bool sortByDistance)
        {
            return GetObjects(frustum, null, useSphere, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsInVolume(BoundingFrustum frustum, ModularSceneryObjectTypes filter, bool useSphere, bool sortByDistance)
        {
            return GetObjects(frustum, filter, useSphere, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        private IEnumerable<ModularSceneryItem> GetObjects(BoundingBox bbox, ModularSceneryObjectTypes? filter, bool useSphere, bool sortByDistance)
        {
            var res = entities
                .Where(e => !filter.HasValue || filter.Value.HasFlag(e.Object.Type))
                .Where(e =>
                {
                    if (useSphere)
                    {
                        return bbox.Contains(e.Item.GetBoundingSphere()) != ContainmentType.Disjoint;
                    }
                    else
                    {
                        return bbox.Contains(e.Item.GetBoundingBox()) != ContainmentType.Disjoint;
                    }
                })
                .ToList();

            if (sortByDistance)
            {
                var center = bbox.GetCenter();

                res.Sort((a, b) =>
                {
                    var aPos = Vector3.DistanceSquared(a.Item.Manipulator.Position, center);
                    var bPos = Vector3.DistanceSquared(b.Item.Manipulator.Position, center);

                    return aPos.CompareTo(bPos);
                });
            }

            return res.ToArray();
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        private IEnumerable<ModularSceneryItem> GetObjects(BoundingSphere sphere, ModularSceneryObjectTypes? filter, bool useSphere, bool sortByDistance)
        {
            var res = entities
                .Where(e => !filter.HasValue || filter.Value.HasFlag(e.Object.Type))
                .Where(e =>
                {
                    if (useSphere)
                    {
                        var sph = e.Item.GetBoundingSphere();

                        return sphere.Contains(ref sph) != ContainmentType.Disjoint;
                    }
                    else
                    {
                        var bbox = e.Item.GetBoundingBox();

                        return sphere.Contains(ref bbox) != ContainmentType.Disjoint;
                    }
                })
                .ToList();

            if (sortByDistance)
            {
                var center = sphere.Center;

                res.Sort((a, b) =>
                {
                    var aPos = Vector3.DistanceSquared(a.Item.Manipulator.Position, center);
                    var bPos = Vector3.DistanceSquared(b.Item.Manipulator.Position, center);

                    return aPos.CompareTo(bPos);
                });
            }

            return res.ToArray();
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="useSphere">Sets wether use item bounding sphere or bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        private IEnumerable<ModularSceneryItem> GetObjects(BoundingFrustum frustum, ModularSceneryObjectTypes? filter, bool useSphere, bool sortByDistance)
        {
            var res = entities
                .Where(e => !filter.HasValue || filter.Value.HasFlag(e.Object.Type))
                .Where(e =>
                {
                    if (useSphere)
                    {
                        var sph = e.Item.GetBoundingSphere();

                        return frustum.Contains(ref sph) != ContainmentType.Disjoint;
                    }
                    else
                    {
                        var bbox = e.Item.GetBoundingBox();

                        return frustum.Contains(ref bbox) != ContainmentType.Disjoint;
                    }
                })
                .ToList();

            if (sortByDistance)
            {
                var center = frustum.GetCameraParams().Position;

                res.Sort((a, b) =>
                {
                    var aPos = Vector3.DistanceSquared(a.Item.Manipulator.Position, center);
                    var bPos = Vector3.DistanceSquared(b.Item.Manipulator.Position, center);

                    return aPos.CompareTo(bPos);
                });
            }

            return res.ToArray();
        }

        /// <summary>
        /// Gets the available triggers for the specified object
        /// </summary>
        /// <param name="item">Scenery item</param>
        /// <returns>Returns a list of triggers</returns>
        public IEnumerable<ModularSceneryTrigger> GetTriggersByObject(ModularSceneryItem item)
        {
            if (!triggers.ContainsKey(item.Item))
            {
                return Array.Empty<ModularSceneryTrigger>();
            }

            return triggers[item.Item]
                .Where(t => string.Equals(t.StateFrom, item.CurrentState, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        /// <summary>
        /// Executes the specified callback
        /// </summary>
        /// <param name="staterItem">Starter item</param>
        /// <param name="starterTrigger">Starter trigger</param>
        public void ExecuteTrigger(ModularSceneryItem staterItem, ModularSceneryTrigger starterTrigger)
        {
            var callback = new TriggerCallback(starterTrigger, staterItem);

            ExecuteTriggerInternal(callback, staterItem, starterTrigger);

            TriggerStart?.Invoke(this, new ModularSceneryTriggerEventArgs()
            {
                StarterTrigger = starterTrigger,
                StarterItem = staterItem,
                Items = callback.Items,
            });

            activeCallbacks.Add(callback);
        }
        /// <summary>
        /// Executes the specified trigger
        /// </summary>
        /// <param name="callback">Trigger callback</param>
        /// <param name="item">Triggered item</param>
        /// <param name="trigger">Trigger</param>
        /// <returns>Returns the affected items</returns>
        private void ExecuteTriggerInternal(TriggerCallback callback, ModularSceneryItem item, ModularSceneryTrigger trigger)
        {
            //Validate item state
            if (item.CurrentState != trigger.StateFrom)
            {
                return;
            }

            callback.Items.Add(item);

            item.CurrentState = trigger.StateTo;

            //Execute the action in the item first
            if (animations.TryGetValue(item.Item, out var anim) && anim.TryGetValue(trigger.AnimationPlan, out var plan))
            {
                item.Item.AnimationController.ReplacePlan(plan);
                item.Item.AnimationController.Start();
                item.Item.InvalidateCache();
            }

            //Find the referenced items and execute actions recursively
            foreach (var action in trigger.Actions)
            {
                //Find item
                var refItem = GetObjectById(action.ItemId);
                if (refItem == null)
                {
                    continue;
                }

                //Find trigger collection
                if (!triggers.ContainsKey(refItem.Item))
                {
                    continue;
                }

                //Find trigger
                var refTrigger = triggers[refItem.Item].Find(t => string.Equals(t.Name, action.ItemAction, StringComparison.OrdinalIgnoreCase));
                if (refTrigger == null)
                {
                    continue;
                }

                ExecuteTriggerInternal(callback, refItem, refTrigger);
            }
        }

        /// <inheritdoc/>
        public override ICullingVolume GetCullingVolume()
        {
            return assetMapIntersections;
        }

        /// <summary>
        /// Asset map intersection helpers
        /// </summary>
        class AssetMapIntersections : ICullingVolume
        {
            /// <summary>
            /// Asset map
            /// </summary>
            private readonly List<AssetMapItem> assetMap = new();
            /// <summary>
            /// Visible bounding boxes
            /// </summary>
            private readonly List<BoundingBox> visibleBoxes = new();

            /// <inheritdoc/>
            public Vector3 Position
            {
                get
                {
                    return Vector3.Zero;
                }
            }

            /// <inheritdoc/>
            public ContainmentType Contains(BoundingBox bbox)
            {
                for (int i = 0; i < visibleBoxes.Count; i++)
                {
                    var res = Intersection.BoxContainsBox(visibleBoxes[i], bbox);

                    if (res != ContainmentType.Disjoint) return res;
                }

                return ContainmentType.Disjoint;
            }
            /// <inheritdoc/>
            public ContainmentType Contains(BoundingSphere sphere)
            {
                for (int i = 0; i < visibleBoxes.Count; i++)
                {
                    var res = Intersection.BoxContainsSphere(visibleBoxes[i], sphere);

                    if (res != ContainmentType.Disjoint) return res;
                }

                return ContainmentType.Disjoint;
            }
            /// <inheritdoc/>
            public ContainmentType Contains(BoundingFrustum frustum)
            {
                for (int i = 0; i < visibleBoxes.Count; i++)
                {
                    var res = Intersection.BoxContainsFrustum(visibleBoxes[i], frustum);

                    if (res != ContainmentType.Disjoint) return res;
                }

                return ContainmentType.Disjoint;
            }
            /// <inheritdoc/>
            public ContainmentType Contains(IEnumerable<Triangle> mesh)
            {
                for (int i = 0; i < visibleBoxes.Count; i++)
                {
                    var res = Intersection.BoxContainsMesh(visibleBoxes[i], mesh);

                    if (res != ContainmentType.Disjoint) return res;
                }

                return ContainmentType.Disjoint;
            }

            /// <summary>
            /// Adds a new item to collection
            /// </summary>
            /// <param name="item">Item</param>
            public void Add(AssetMapItem item)
            {
                assetMap.Add(item);
            }
            /// <summary>
            /// Builds the asset map
            /// </summary>
            /// <param name="assetConfiguration">Configuration</param>
            /// <param name="assets">Asset list</param>
            public void Build(AssetMap assetConfiguration, Dictionary<string, ModelInstanced> assets)
            {
                //Fill per complex asset bounding boxes
                Fill(assets);

                //Find connections
                for (int s = 0; s < assetMap.Count; s++)
                {
                    for (int t = s + 1; t < assetMap.Count; t++)
                    {
                        var source = assetMap[s];
                        var target = assetMap[t];

                        if (source.Volume.Contains(target.Volume) != ContainmentType.Disjoint)
                        {
                            //Find if contacted volumes has portals between them
                            FindPortals(assetConfiguration, source, target, s, t);
                        }
                    }
                }
            }
            /// <summary>
            /// Fills the full bounding volume of the assets in the map
            /// </summary>
            /// <param name="assets">Asset list</param>
            private void Fill(Dictionary<string, ModelInstanced> assets)
            {
                for (int i = 0; i < assetMap.Count; i++)
                {
                    var item = assetMap[i];

                    BoundingBox bbox = new();

                    foreach (var assetName in item.Assets.Keys)
                    {
                        foreach (int assetIndex in item.Assets[assetName])
                        {
                            var aBbox = assets[assetName][assetIndex].GetBoundingBox();

                            if (bbox == new BoundingBox())
                            {
                                bbox = aBbox;
                            }
                            else
                            {
                                bbox = BoundingBox.Merge(bbox, aBbox);
                            }
                        }
                    }

                    item.Volume = bbox;
                }
            }
            /// <summary>
            /// Finds portals between the specified asset items
            /// </summary>
            /// <param name="assetConfiguration">Configuration</param>
            /// <param name="source">Source item</param>
            /// <param name="target">Target item</param>
            /// <param name="s">Source index</param>
            /// <param name="t">Target index</param>
            private static void FindPortals(AssetMap assetConfiguration, AssetMapItem source, AssetMapItem target, int s, int t)
            {
                var sourceConf = assetConfiguration.Assets.FirstOrDefault(a => a.Name == source.Name);
                if (sourceConf?.Connections?.Any() == true)
                {
                    var targetConf = assetConfiguration.Assets.FirstOrDefault(a => a.Name == target.Name);
                    if (targetConf?.Connections?.Any() == true)
                    {
                        //Transform connection positions and directions
                        var sourcePositions = sourceConf.Connections.Select(i => ReadConnection(i, source.Transform));
                        var targetPositions = targetConf.Connections.Select(i => ReadConnection(i, target.Transform));

                        if (sourcePositions.Any(p1 =>
                        {
                            return targetPositions.Any(p2 =>
                            {
                                return ConnectorInfo.IsConnected(p1, p2);
                            });
                        }))
                        {
                            source.Connections.Add(t);
                            target.Connections.Add(s);
                        }
                    }
                }
            }
            /// <summary>
            /// Reads a connection from an asset, and transfoms it for portal detection
            /// </summary>
            /// <param name="connection">Connection</param>
            /// <param name="transform">Transform to apply</param>
            /// <returns>Returns the connector information</returns>
            private static ConnectorInfo ReadConnection(AssetConnection connection, Matrix transform)
            {
                return new ConnectorInfo
                {
                    OpenConection = connection.Type == ModularSceneryAssetConnectionTypes.Open,
                    Position = Vector3.TransformCoordinate(connection.Position, transform),
                    Direction = Vector3.TransformNormal(connection.Direction, transform),
                };
            }

            /// <summary>
            /// Gets all complex map asset volumes
            /// </summary>
            /// <returns>Returns a dictionary of complex asset volumes by asset name</returns>
            public Dictionary<string, List<BoundingBox>> GetMapVolumes()
            {
                var res = new Dictionary<string, List<BoundingBox>>();

                for (int i = 0; i < assetMap.Count; i++)
                {
                    var item = assetMap[i];

                    if (!res.ContainsKey(item.Name))
                    {
                        res.Add(item.Name, new List<BoundingBox>());
                    }

                    res[item.Name].Add(item.Volume);
                }

                return res;
            }

            /// <summary>
            /// Updates internal visible volume collection
            /// </summary>
            /// <param name="camera">Camera volume</param>
            public void Update(IntersectionVolumeFrustum camera)
            {
                //Find current box
                var itemIndex = assetMap.FindIndex(b => b.Volume.Contains(camera.Position) != ContainmentType.Disjoint);
                if (itemIndex >= 0)
                {
                    visibleBoxes.Clear();
                    visibleBoxes.Add(assetMap[itemIndex].Volume);

                    List<int> visited = new()
                    {
                        itemIndex
                    };

                    foreach (var conIndex in assetMap[itemIndex].Connections)
                    {
                        UpdateItem(camera, conIndex, visited);
                    }
                }
            }
            /// <summary>
            /// Updates internal visible volume collection recursive
            /// </summary>
            /// <param name="camera">Camera volume</param>
            /// <param name="itemIndex">Item index</param>
            /// <param name="visited">Visited list</param>
            private void UpdateItem(IntersectionVolumeFrustum camera, int itemIndex, List<int> visited)
            {
                visited.Add(itemIndex);

                if (camera.Contains(assetMap[itemIndex].Volume) != ContainmentType.Disjoint)
                {
                    visibleBoxes.Add(assetMap[itemIndex].Volume);

                    foreach (var conIndex in assetMap[itemIndex].Connections)
                    {
                        if (!visited.Contains(conIndex))
                        {
                            UpdateItem(camera, conIndex, visited);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Conector information
        /// </summary>
        struct ConnectorInfo
        {
            /// <summary>
            /// Gets whether the connectors were connected or not
            /// </summary>
            /// <param name="x">Connector x</param>
            /// <param name="y">Connector y</param>
            /// <returns>Returns true if the connectors were connected</returns>
            public static bool IsConnected(ConnectorInfo x, ConnectorInfo y)
            {
                var a = x;
                var b = y;

                if (a.OpenConection)
                {
                    // True if oppsite directions
                    return Vector3.Dot(a.Direction, b.Direction) == -1;
                }
                else
                {
                    return a.Position == b.Position;
                }
            }

            /// <summary>
            /// Open connection
            /// </summary>
            public bool OpenConection { get; set; }
            /// <summary>
            /// Position
            /// </summary>
            public Vector3 Position { get; set; }
            /// <summary>
            /// Direction
            /// </summary>
            public Vector3 Direction { get; set; }
        }
        /// <summary>
        /// Asset map item
        /// </summary>
        class AssetMapItem
        {
            /// <summary>
            /// Map index
            /// </summary>
            public int Index;
            /// <summary>
            /// Complex asset name
            /// </summary>
            public string Name;
            /// <summary>
            /// Complex asset volume
            /// </summary>
            public BoundingBox Volume;
            /// <summary>
            /// Complex asset transform
            /// </summary>
            public Matrix Transform;
            /// <summary>
            /// Individual asset indices
            /// </summary>
            public Dictionary<string, List<int>> Assets = new();
            /// <summary>
            /// Connections with other complex assets
            /// </summary>
            public List<int> Connections = new();

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"{Name}; Index: {Index}; Connections: {Connections.Count};";
            }
        }
        /// <summary>
        /// Trigger callback
        /// </summary>
        /// <remarks>Helper class to test when all items affected by a trigger were done with their actions</remarks>
        class TriggerCallback
        {
            /// <summary>
            /// Starter trigger
            /// </summary>
            public ModularSceneryTrigger Trigger { get; set; }
            /// <summary>
            /// Starter item
            /// </summary>
            public ModularSceneryItem Item { get; set; }

            /// <summary>
            /// Affected items
            /// </summary>
            public List<ModularSceneryItem> Items { get; set; } = new List<ModularSceneryItem>();
            /// <summary>
            /// Returns true if any item is performing actions
            /// </summary>
            public bool Waiting
            {
                get
                {
                    return Items.Exists(i => i.Item.AnimationController?.Playing == true);
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="trigger">Trigger</param>
            public TriggerCallback(ModularSceneryTrigger trigger, ModularSceneryItem item)
            {
                Trigger = trigger;
                Item = item;
            }
        }
        /// <summary>
        /// Instance info
        /// </summary>
        class InstanceInfo
        {
            /// <summary>
            /// Instance count
            /// </summary>
            public int Count { get; set; }
            /// <summary>
            /// Use of path finding
            /// </summary>
            public ModularSceneryPathFindingModes PathFinding { get; set; }
        }
    }
}
