using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Modular
{
    using Engine.Modular.Persistence;

    /// <summary>
    /// Terrain model
    /// </summary>
    public sealed class ModularScenery : Ground<ModularSceneryDescription>
    {
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
        /// Scenery entities
        /// </summary>
        private readonly List<Item> entities = new();
        /// <summary>
        /// Triggers list by instance
        /// </summary>
        private readonly Dictionary<ModelInstance, IEnumerable<ItemTrigger>> triggers = new();
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
        /// Scene bounding box
        /// </summary>
        private BoundingBox? sceneBoundingBox;
        /// <summary>
        /// Scene bounding sphere
        /// </summary>
        private BoundingSphere? sceneBoundingSphere;
        /// <summary>
        /// Scene triangle list
        /// </summary>
        private IEnumerable<Triangle> sceneTriangles;

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
        public event TriggerStartHandler TriggerStart;
        /// <summary>
        /// Trigger ends it's execution event
        /// </summary>
        public event TriggerEndHandler TriggerEnd;

        /// <summary>
        /// Gets the bounding sphere list of all items in the specified dictionary
        /// </summary>
        /// <param name="instances">Instances dictionary</param>
        /// <param name="refresh">Refresh internal item cache</param>
        private static IEnumerable<BoundingSphere> GetSceneAssetSpheres(Dictionary<string, ModelInstanced> instances, bool refresh)
        {
            foreach (var item in instances.Keys)
            {
                var model = instances[item];
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < model.InstanceCount; i++)
                {
                    yield return model[i].GetBoundingSphere(refresh);
                }
            }
        }
        /// <summary>
        /// Gets the bounding box list of all items in the specified dictionary
        /// </summary>
        /// <param name="instances">Instances dictionary</param>
        /// <param name="refresh">Refresh internal item cache</param>
        private static IEnumerable<BoundingBox> GetSceneAssetBoxes(Dictionary<string, ModelInstanced> instances, bool refresh)
        {
            foreach (var item in instances.Keys)
            {
                var model = instances[item];
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < model.InstanceCount; i++)
                {
                    yield return model[i].GetBoundingBox(refresh);
                }
            }
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
        public override async Task ReadAssets(ModularSceneryDescription description)
        {
            await base.ReadAssets(description);

            assetMap = Description.GetAssetMap();

            levelMap = Description.GetLevelMap();
        }

        /// <summary>
        /// Loads the first level
        /// </summary>
        /// <param name="progress">Resource loading progress updater</param>
        public async Task LoadFirstLevel(IProgress<LoadResourceProgress> progress = null)
        {
            await LoadLevel(FirstLevel, progress);
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
            //Crear cache
            ClearCache();

            //Removes previous level components from scene
            Scene.Components.RemoveComponents(assets.Select(a => a.Value));
            Scene.Components.RemoveComponents(objects.Select(o => o.Value));

            //Clear internal lists and data
            assets.Clear();
            objects.Clear();
            entities.Clear();
            particleManager?.Clear();
            particleDescriptors.Clear();

            CurrentLevel = level;

            var contentLibrary = await Description.ReadContentLibrary();

            await InitializeParticles(progress);
            await InitializeAssets(level, contentLibrary, progress);
            await InitializeObjects(level, contentLibrary, progress);

            ParseAssetsMap(level, progress);

            InitializeEntities(level, progress);

            // Initialize quad-tree for ray picking
            GroundPickingQuadtree = Description.ReadQuadTree(GetGeometry(GeometryTypes.Picking));
        }
        /// <summary>
        /// Initialize the particle system and the particle descriptions
        /// </summary>
        private async Task InitializeParticles(IProgress<LoadResourceProgress> progress = null)
        {
            var particleSystems = Description.GetLevelParticleSystems();
            if (!particleSystems.Any())
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

            float total = particleSystems.Count();
            int current = 0;

            foreach (var item in particleSystems)
            {
                particleDescriptors.Add(item.Name, item.SystemDescription);

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
            var assetDescriptions = Description.GetLevelAssets(level, contentLibrary);

            float total = assetDescriptions.Count();
            int current = 0;

            foreach (var asset in assetDescriptions)
            {
                string assetId = asset.Id;
                string assetName = asset.Name;
                var assetDesc = asset.ModelDescription;

                var model = await Scene.AddComponent<ModelInstanced, ModelInstancedDescription>(assetId, assetName, assetDesc, SceneObjectUsages.Object);
                model.Owner = this;

                assets.Add(assetName, model);

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
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
            var objectDescriptions = Description.GetLevelObjects(level, contentLibrary);

            float total = objectDescriptions.Count();
            int current = 0;

            foreach (var obj in objectDescriptions)
            {
                string objectId = obj.Id;
                string objectName = obj.Name;
                var objectDesc = obj.ModelDescription;

                var model = await Scene.AddComponent<ModelInstanced, ModelInstancedDescription>(objectId, objectName, objectDesc, SceneObjectUsages.Object);
                model.Owner = this;

                //Get the object list to process
                var objList = level.Objects
                    .Where(o => string.Equals(o.AssetName, objectName, StringComparison.OrdinalIgnoreCase))
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

                objects.Add(objectName, model);

                progress?.Report(new LoadResourceProgress { Progress = ++current / total });
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
            //Plans
            var animationPlans = obj.GetAnimations();
            if (animationPlans.Any())
            {
                var animationDict = animationPlans.ToDictionary(a => a.Name, e => e.Plan);
                animations.Add(instance, animationDict);

                var defaultPlan = obj.GetDefaultAnimationPlanName();

                instance.AnimationController.ReplacePlan(new AnimationPlan(defaultPlan));
                instance.InvalidateCache();
            }

            var instanceTriggers = obj.GetTriggers();
            if (instanceTriggers.Any())
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

                        entities.Add(new Item(obj, instance, emitters, defaultState));
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

            var asset = assetMap.Assets.ElementAt(assetIndex);
            var assetTransforms = asset.GetInstanceTransforms();

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
                            basicAssetType == AssetTypes.Floor ||
                            basicAssetType == AssetTypes.Ceiling;
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
            var res = CurrentLevel.FindAssetReference(assetMap.Assets, levelAssetId, mapAssetId);
            if (res == null)
            {
                return null;
            }

            // Look for all geometry references
            int index = CurrentLevel.GetMapInstanceIndex(assetMap.Assets, res.AssetName, levelAssetId, mapAssetId);
            if (index >= 0)
            {
                return assets[res.AssetName][index];
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
            var index = CurrentLevel.GetObjectInstanceIndex(assetName, id);
            if (index >= 0)
            {
                return objects[assetName][index];
            }

            return null;
        }
        /// <summary>
        /// Clears the scene geometry caché
        /// </summary>
        private void ClearCache()
        {
            sceneBoundingBox = null;
            sceneBoundingSphere = null;
            sceneTriangles = null;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            UpdateTriggers();
        }
        /// <summary>
        /// Verifies the active triggers states and fires the ending events
        /// </summary>
        private void UpdateTriggers()
        {
            if (activeCallbacks?.Any() != true)
            {
                return;
            }

            activeCallbacks.ForEach(c =>
            {
                if (c.Waiting)
                {
                    return;
                }

                TriggerEnd?.Invoke(this, new TriggerEventArgs()
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
            if (refresh)
            {
                sceneBoundingSphere = null;
            }

            if (sceneBoundingSphere.HasValue)
            {
                return sceneBoundingSphere.Value;
            }

            var assetsSph = GetSceneAssetSpheres(assets, refresh);
            var objectsSph = GetSceneAssetSpheres(objects, refresh);

            BoundingSphere res = new();
            bool initialized = false;
            foreach (var sph in assetsSph.Concat(objectsSph))
            {
                if (!initialized)
                {
                    res = sph;
                    initialized = true;
                    continue;
                }

                res = BoundingSphere.Merge(res, sph);
            }

            sceneBoundingSphere = res;

            return sceneBoundingSphere.Value;
        }
        /// <inheritdoc/>
        public override BoundingBox GetBoundingBox(bool refresh = false)
        {
            if (refresh)
            {
                sceneBoundingBox = null;
            }

            if (sceneBoundingBox.HasValue)
            {
                return sceneBoundingBox.Value;
            }

            var assetsBoxes = GetSceneAssetBoxes(assets, refresh);
            var objectsBoxes = GetSceneAssetBoxes(objects, refresh);

            BoundingBox res = new();
            bool initialized = false;
            foreach (var box in assetsBoxes.Concat(objectsBoxes))
            {
                if (!initialized)
                {
                    res = box;
                    initialized = true;
                    continue;
                }

                res = BoundingBox.Merge(res, box);
            }

            sceneBoundingBox = res;

            return sceneBoundingBox.Value;
        }
        /// <inheritdoc/>
        public override IEnumerable<Triangle> GetGeometry(GeometryTypes geometryType)
        {
            if (sceneTriangles != null)
            {
                return sceneTriangles;
            }

            List<Triangle> triangleLits = new();

            var assetTriangles = assets.Values
                .SelectMany(asset => asset.GetInstances().Where(i => i.Visible))
                .SelectMany(instance => instance.GetGeometry(geometryType));

            triangleLits.AddRange(assetTriangles);

            var objTriangles = objects.Values
                .SelectMany(obj => obj.GetInstances().Where(i => i.Visible))
                .SelectMany(instance => instance.GetGeometry(geometryType));

            triangleLits.AddRange(objTriangles);

            sceneTriangles = triangleLits.ToArray();

            return sceneTriangles;
        }

        /// <summary>
        /// Gets the specified object by id
        /// </summary>
        /// <param name="id">Object id</param>
        /// <returns>Returns the specified object by id</returns>
        public Item GetObjectById(string id)
        {
            return entities.Find(o => string.Equals(o.Object.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get objects by name
        /// </summary>
        /// <param name="name">Object name</param>
        /// <returns>Returns a list of objects by name</returns>
        public IEnumerable<Item> GetObjectsByName(string name)
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
        public IEnumerable<Item> GetObjectsByType(ObjectTypes objectType)
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
        public IEnumerable<Item> GetObjectsInVolume(BoundingBox bbox, bool useSphere, bool sortByDistance)
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
        public IEnumerable<Item> GetObjectsInVolume(BoundingBox bbox, ObjectTypes filter, bool useSphere, bool sortByDistance)
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
        public IEnumerable<Item> GetObjectsInVolume(BoundingSphere sphere, bool useSphere, bool sortByDistance)
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
        public IEnumerable<Item> GetObjectsInVolume(BoundingSphere sphere, ObjectTypes filter, bool useSphere, bool sortByDistance)
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
        public IEnumerable<Item> GetObjectsInVolume(BoundingFrustum frustum, bool useSphere, bool sortByDistance)
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
        public IEnumerable<Item> GetObjectsInVolume(BoundingFrustum frustum, ObjectTypes filter, bool useSphere, bool sortByDistance)
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
        private IEnumerable<Item> GetObjects(BoundingBox bbox, ObjectTypes? filter, bool useSphere, bool sortByDistance)
        {
            var res = entities
                .Where(e => !filter.HasValue || filter.Value.HasFlag(e.Object.Type))
                .Where(e =>
                {
                    if (useSphere)
                    {
                        return bbox.Contains(e.Instance.GetBoundingSphere()) != ContainmentType.Disjoint;
                    }
                    else
                    {
                        return bbox.Contains(e.Instance.GetBoundingBox()) != ContainmentType.Disjoint;
                    }
                })
                .ToList();

            if (sortByDistance)
            {
                var center = bbox.GetCenter();

                res.Sort((a, b) =>
                {
                    var aPos = Vector3.DistanceSquared(a.Instance.Manipulator.Position, center);
                    var bPos = Vector3.DistanceSquared(b.Instance.Manipulator.Position, center);

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
        private IEnumerable<Item> GetObjects(BoundingSphere sphere, ObjectTypes? filter, bool useSphere, bool sortByDistance)
        {
            var res = entities
                .Where(e => !filter.HasValue || filter.Value.HasFlag(e.Object.Type))
                .Where(e =>
                {
                    if (useSphere)
                    {
                        var sph = e.Instance.GetBoundingSphere();

                        return sphere.Contains(ref sph) != ContainmentType.Disjoint;
                    }
                    else
                    {
                        var bbox = e.Instance.GetBoundingBox();

                        return sphere.Contains(ref bbox) != ContainmentType.Disjoint;
                    }
                })
                .ToList();

            if (sortByDistance)
            {
                var center = sphere.Center;

                res.Sort((a, b) =>
                {
                    var aPos = Vector3.DistanceSquared(a.Instance.Manipulator.Position, center);
                    var bPos = Vector3.DistanceSquared(b.Instance.Manipulator.Position, center);

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
        private IEnumerable<Item> GetObjects(BoundingFrustum frustum, ObjectTypes? filter, bool useSphere, bool sortByDistance)
        {
            var res = entities
                .Where(e => !filter.HasValue || filter.Value.HasFlag(e.Object.Type))
                .Where(e =>
                {
                    if (useSphere)
                    {
                        var sph = e.Instance.GetBoundingSphere();

                        return frustum.Contains(ref sph) != ContainmentType.Disjoint;
                    }
                    else
                    {
                        var bbox = e.Instance.GetBoundingBox();

                        return frustum.Contains(ref bbox) != ContainmentType.Disjoint;
                    }
                })
                .ToList();

            if (sortByDistance)
            {
                var center = frustum.GetCameraParams().Position;

                res.Sort((a, b) =>
                {
                    var aPos = Vector3.DistanceSquared(a.Instance.Manipulator.Position, center);
                    var bPos = Vector3.DistanceSquared(b.Instance.Manipulator.Position, center);

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
        public IEnumerable<ItemTrigger> GetTriggersByObject(Item item)
        {
            if (!triggers.ContainsKey(item.Instance))
            {
                return Array.Empty<ItemTrigger>();
            }

            return triggers[item.Instance]
                .Where(t => string.Equals(t.StateFrom, item.CurrentState, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        /// <summary>
        /// Executes the specified callback
        /// </summary>
        /// <param name="staterItem">Starter item</param>
        /// <param name="starterTrigger">Starter trigger</param>
        public void ExecuteTrigger(Item staterItem, ItemTrigger starterTrigger)
        {
            var callback = new TriggerCallback(starterTrigger, staterItem);

            ExecuteTriggerInternal(callback, staterItem, starterTrigger);

            TriggerStart?.Invoke(this, new TriggerEventArgs()
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
        private void ExecuteTriggerInternal(TriggerCallback callback, Item item, ItemTrigger trigger)
        {
            //Validate item state
            if (item.CurrentState != trigger.StateFrom)
            {
                return;
            }

            callback.Items.Add(item);

            item.CurrentState = trigger.StateTo;

            //Execute the action in the item first
            if (animations.TryGetValue(item.Instance, out var anim) && anim.TryGetValue(trigger.AnimationPlan, out var plan))
            {
                item.Instance.AnimationController.ReplacePlan(plan);
                item.Instance.AnimationController.Start();
                item.Instance.InvalidateCache();
            }

            //Find the referenced items and execute actions recursively
            foreach (var action in trigger.Actions)
            {
                //Find item
                var refItem = GetObjectById(action.Id);
                if (refItem == null)
                {
                    continue;
                }

                //Find trigger collection
                if (!triggers.ContainsKey(refItem.Instance))
                {
                    continue;
                }

                //Find trigger
                var refTrigger = triggers[refItem.Instance].FirstOrDefault(t => string.Equals(t.Name, action.Action, StringComparison.OrdinalIgnoreCase));
                if (refTrigger == null)
                {
                    continue;
                }

                ExecuteTriggerInternal(callback, refItem, refTrigger);
            }
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
            /// Complex asset transform
            /// </summary>
            public Matrix Transform;
            /// <summary>
            /// Individual asset indices
            /// </summary>
            public Dictionary<string, List<int>> Assets = new();

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"{Name}; Index: {Index}; Assets: {string.Join("|", Assets?.Select(a => a.Key) ?? Array.Empty<string>())}";
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
            public ItemTrigger Trigger { get; set; }
            /// <summary>
            /// Starter item
            /// </summary>
            public Item Item { get; set; }

            /// <summary>
            /// Affected items
            /// </summary>
            public List<Item> Items { get; set; } = new List<Item>();
            /// <summary>
            /// Returns true if any item is performing actions
            /// </summary>
            public bool Waiting
            {
                get
                {
                    return Items.Exists(i => i.Instance.AnimationController?.Playing == true);
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="trigger">Trigger</param>
            public TriggerCallback(ItemTrigger trigger, Item item)
            {
                Trigger = trigger;
                Item = item;
            }
        }
    }
}
