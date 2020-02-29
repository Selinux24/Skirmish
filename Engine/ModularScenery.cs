using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class ModularScenery : Ground
    {
        /// <summary>
        /// Asset models dictionary
        /// </summary>
        private readonly Dictionary<string, ModelInstanced> assets = new Dictionary<string, ModelInstanced>();
        /// <summary>
        /// Object models dictionary
        /// </summary>
        private readonly Dictionary<string, ModelInstanced> objects = new Dictionary<string, ModelInstanced>();
        /// <summary>
        /// Particle descriptors dictionary
        /// </summary>
        private readonly Dictionary<string, ParticleSystemDescription> particleDescriptors = new Dictionary<string, ParticleSystemDescription>();
        /// <summary>
        /// Particle manager
        /// </summary>
        private ParticleManager particleManager = null;
        /// <summary>
        /// Asset map
        /// </summary>
        private AssetMap assetMap = null;
        /// <summary>
        /// Scenery entities
        /// </summary>
        private readonly List<ModularSceneryItem> entities = new List<ModularSceneryItem>();
        /// <summary>
        /// Triggers list by instance
        /// </summary>
        private readonly Dictionary<ModelInstance, List<ModularSceneryTrigger>> triggers = new Dictionary<ModelInstance, List<ModularSceneryTrigger>>();
        /// <summary>
        /// Animations plan dictionary by instance
        /// </summary>
        private readonly Dictionary<ModelInstance, Dictionary<string, AnimationPlan>> animations = new Dictionary<ModelInstance, Dictionary<string, AnimationPlan>>();
        /// <summary>
        /// Active trigger callbacks
        /// </summary>
        private readonly List<TriggerCallback> activeCallbacks = new List<TriggerCallback>();

        /// <summary>
        /// Gets the assets description
        /// </summary>
        protected ModularSceneryAssetConfiguration AssetConfiguration { get; set; }
        /// <summary>
        /// Gets the level list
        /// </summary>
        protected ModularSceneryLevels Levels { get; set; }

        /// <summary>
        /// First level
        /// </summary>
        public ModularSceneryLevel FirstLevel
        {
            get
            {
                return this.Levels.Levels.FirstOrDefault();
            }
        }
        /// <summary>
        /// Current level
        /// </summary>
        public ModularSceneryLevel CurrentLevel { get; set; }
        /// <summary>
        /// Gets the description
        /// </summary>
        public new ModularSceneryDescription Description
        {
            get
            {
                return base.Description as ModularSceneryDescription;
            }
        }

        /// <summary>
        /// Trigger starts it's execution event
        /// </summary>
        public event ModularSceneryTriggerStartHandler TriggerStart;
        /// <summary>
        /// Trigger ends it's execution event
        /// </summary>
        public event ModularSceneryTriggerEndHandler TriggerEnd;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Scenery description</param>
        public ModularScenery(Scene scene, ModularSceneryDescription description)
            : base(scene, description)
        {
            if (description.AssetsConfiguration != null)
            {
                this.AssetConfiguration = description.AssetsConfiguration;
            }
            else if (!string.IsNullOrWhiteSpace(description.AssetsConfigurationFile))
            {
                this.AssetConfiguration = Helper.DeserializeFromFile<ModularSceneryAssetConfiguration>(Path.Combine(description.Content.ContentFolder, description.AssetsConfigurationFile));
            }

            if (description.Levels != null)
            {
                this.Levels = description.Levels;
            }
            else if (!string.IsNullOrWhiteSpace(description.LevelsFile))
            {
                this.Levels = Helper.DeserializeFromFile<ModularSceneryLevels>(Path.Combine(description.Content.ContentFolder, description.LevelsFile));
            }
        }
        /// <summary>
        /// Resource dispose
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected override void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Loads the level by name
        /// </summary>
        /// <param name="levelName">Level name</param>
        public async Task LoadLevel(string levelName)
        {
            if (string.Equals(this.CurrentLevel?.Name, levelName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //Removes previous level components from scene
            this.Scene.RemoveComponents(this.assets.Select(a => a.Value));
            this.Scene.RemoveComponents(this.objects.Select(o => o.Value));

            //Clear internal lists and data
            this.assets.Clear();
            this.objects.Clear();
            this.entities.Clear();
            this.assetMap = null;
            this.particleManager?.Clear();
            this.particleDescriptors.Clear();

            //Find the level
            var level = this.Levels.Levels
                .FirstOrDefault(l => string.Equals(l.Name, levelName, StringComparison.OrdinalIgnoreCase));
            if (level != null)
            {
                this.CurrentLevel = level;

                //Load the level
                await this.LoadLevel(level);
            }
        }

        /// <summary>
        /// Loads the model content
        /// </summary>
        /// <returns>Returns the model content</returns>
        private ModelContent LoadModelContent()
        {
            ModelContent content = null;

            if (!string.IsNullOrEmpty(Description.Content.ModelContentFilename))
            {
                var contentDesc = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(Description.Content.ContentFolder, Description.Content.ModelContentFilename));
                var loader = GameResourceManager.GetLoaderForFile(contentDesc.ModelFileName);
                var t = loader.Load(Description.Content.ContentFolder, contentDesc);
                content = t.First();
            }
            else if (Description.Content.ModelContentDescription != null)
            {
                var contentDesc = Description.Content.ModelContentDescription;
                var loader = GameResourceManager.GetLoaderForFile(contentDesc.ModelFileName); 
                var t = loader.Load(Description.Content.ContentFolder, contentDesc);
                content = t.First();
            }
            else if (Description.Content.HeightmapDescription != null)
            {
                content = ModelContent.FromHeightmap(
                    Description.Content.HeightmapDescription.ContentPath,
                    Description.Content.HeightmapDescription.HeightmapFileName,
                    Description.Content.HeightmapDescription.Textures.TexturesLR,
                    Description.Content.HeightmapDescription.CellSize,
                    Description.Content.HeightmapDescription.MaximumHeight);
            }
            else if (Description.Content.ModelContent != null)
            {
                content = Description.Content.ModelContent;
            }

            return content;
        }
        /// <summary>
        /// Loads a level
        /// </summary>
        /// <param name="level">Level definition</param>
        private async Task LoadLevel(ModularSceneryLevel level)
        {
            ModelContent content = LoadModelContent();

            await this.InitializeParticles();
            await this.InitializeAssets(level, content);
            await this.InitializeObjects(level, content);

            this.ParseAssetsMap(level);

            this.InitializeEntities(level);
        }
        /// <summary>
        /// Initialize the particle system and the particle descriptions
        /// </summary>
        private async Task InitializeParticles()
        {
            if (this.Levels.ParticleSystems?.Any() == true)
            {
                this.particleManager = await this.Scene.AddComponentParticleManager(
                    new ParticleManagerDescription()
                    {
                        Name = string.Format("{0}.{1}", this.Description.Name, "Particle Manager"),
                    },
                    SceneObjectUsages.None,
                    98);

                foreach (var item in this.Levels.ParticleSystems)
                {
                    item.ContentPath = item.ContentPath ?? this.Description.Content.ContentFolder;

                    var pDesc = ParticleSystemDescription.Initialize(item);

                    this.particleDescriptors.Add(item.Name, pDesc);
                }
            }
        }
        /// <summary>
        /// Initialize all assets into asset dictionary 
        /// </summary>
        /// <param name="content">Assets model content</param>
        private async Task InitializeAssets(ModularSceneryLevel level, ModelContent content)
        {
            // Get instance count for all single geometries from Map
            var instances = level.GetMapInstanceCounters(this.AssetConfiguration.Assets);

            // Load all single geometries into single instanced model components
            foreach (var assetName in instances.Keys)
            {
                var count = instances[assetName];
                if (count > 0)
                {
                    var modelContent = content.FilterMask(assetName);
                    if (modelContent != null)
                    {
                        var masks = this.Levels.GetMasksForAsset(assetName);
                        var hasVolumes = modelContent.SetVolumeMark(true, masks) > 0;

                        var model = await this.Scene.AddComponentModelInstanced(
                            new ModelInstancedDescription()
                            {
                                Name = string.Format("{0}.{1}.{2}", this.Description.Name, assetName, level.Name),
                                CastShadow = this.Description.CastShadow,
                                UseAnisotropicFiltering = this.Description.UseAnisotropic,
                                Instances = count,
                                LoadAnimation = false,
                                Content = new ContentDescription()
                                {
                                    ModelContent = modelContent,
                                }
                            },
                            hasVolumes ? SceneObjectUsages.CoarsePathFinding : SceneObjectUsages.FullPathFinding);

                        model.HasParent = true;

                        this.assets.Add(assetName, model);
                    }
                }
            }
        }
        /// <summary>
        /// Initialize all objects into asset dictionary 
        /// </summary>
        /// <param name="content">Assets model content</param>
        private async Task InitializeObjects(ModularSceneryLevel level, ModelContent content)
        {
            // Set auto-identifiers
            level.PopulateObjectIds();

            // Get instance count for all single geometries from Map
            var instances = level.GetObjectsInstanceCounters();

            // Load all single geometries into single instanced model components
            foreach (var assetName in instances.Keys)
            {
                var count = instances[assetName];
                if (count <= 0)
                {
                    continue;
                }

                var modelContent = content.FilterMask(assetName);
                if (modelContent == null)
                {
                    continue;
                }

                var masks = this.Levels.GetMasksForAsset(assetName);
                var hasVolumes = modelContent.SetVolumeMark(true, masks) > 0;

                var model = await this.Scene.AddComponentModelInstanced(
                    new ModelInstancedDescription()
                    {
                        Name = string.Format("{0}.{1}.{2}", this.Description.Name, assetName, level.Name),
                        CastShadow = this.Description.CastShadow,
                        UseAnisotropicFiltering = this.Description.UseAnisotropic,
                        Instances = count,
                        AlphaEnabled = this.Description.AlphaEnabled,
                        Content = new ContentDescription()
                        {
                            ModelContent = modelContent,
                        }
                    },
                    hasVolumes ? SceneObjectUsages.CoarsePathFinding : SceneObjectUsages.FullPathFinding);

                //Get the object list to process
                var objList = Array.FindAll(level.Objects, o => string.Equals(o.AssetName, assetName, StringComparison.OrdinalIgnoreCase));

                //Positioning
                model.SetTransforms(objList.Select(o => o.GetTransform()).ToArray());

                //Lights
                for (int i = 0; i < model.InstanceCount; i++)
                {
                    this.InitializeObjectLights(objList[i], model[i]);

                    this.InitializeObjectAnimations(objList[i], model[i]);
                }

                this.objects.Add(assetName, model);
            }
        }
        /// <summary>
        /// Initialize lights attached to the specified object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="instance">Model instance</param>
        private void InitializeObjectLights(ModularSceneryObjectReference obj, ModelInstance instance)
        {
            if (!obj.LoadLights)
            {
                return;
            }

            var trn = instance.Manipulator.LocalTransform;

            var lights = instance.Lights;
            if (lights.Any())
            {
                var emitterDesc = obj.ParticleLight;

                foreach (var light in lights)
                {
                    light.CastShadow = obj.CastShadows;

                    if (emitterDesc != null && light is SceneLightPoint pointL)
                    {
                        var pos = Vector3.TransformCoordinate(pointL.Position, trn);

                        var emitter = new ParticleEmitter(emitterDesc)
                        {
                            Position = pos,
                            Instance = instance,
                        };

                        this.particleManager.AddParticleSystem(
                            ParticleSystemTypes.CPU,
                            this.particleDescriptors[emitterDesc.Name],
                            emitter);
                    }
                }

                this.Scene.Lights.AddRange(lights);
            }
        }
        /// <summary>
        /// Initialize animations and triggers
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="instance">Model instance</param>
        private void InitializeObjectAnimations(ModularSceneryObjectReference obj, ModelInstance instance)
        {
            Dictionary<string, AnimationPlan> animationDict = new Dictionary<string, AnimationPlan>();

            //Plans
            for (int i = 0; i < obj.AnimationPlans?.Length; i++)
            {
                var dPlan = obj.AnimationPlans[i];

                AnimationPlan plan = new AnimationPlan();

                for (int a = 0; a < dPlan.Paths?.Length; a++)
                {
                    var dPath = dPlan.Paths[a];

                    AnimationPath path = new AnimationPath();
                    path.Add(dPath.Name);

                    plan.Add(path);
                }

                animationDict.Add(dPlan.Name, plan);
            }

            if (animationDict.Count > 0)
            {
                this.animations.Add(instance, animationDict);
            }

            List<ModularSceneryTrigger> instanceTriggers = new List<ModularSceneryTrigger>();

            //Actions
            for (int a = 0; a < obj.Actions?.Length; a++)
            {
                var action = obj.Actions[a];

                ModularSceneryTrigger trigger = new ModularSceneryTrigger()
                {
                    Name = action.Name,
                    StateFrom = action.StateFrom,
                    StateTo = action.StateTo,
                    AnimationPlan = action.AnimationPlan,
                };

                for (int i = 0; i < action.Items?.Length; i++)
                {
                    ModularSceneryAction act = new ModularSceneryAction()
                    {
                        ItemId = action.Items[i].Id,
                        ItemAction = action.Items[i].Action,
                    };

                    trigger.Actions.Add(act);
                }

                instanceTriggers.Add(trigger);
            }

            if (instanceTriggers.Count > 0)
            {
                this.triggers.Add(instance, instanceTriggers);
            }
        }

        /// <summary>
        /// Initialize scenery entities proxy list
        /// </summary>
        private void InitializeEntities(ModularSceneryLevel level)
        {
            foreach (var obj in level.Objects)
            {
                ModelInstance instance;

                if (string.IsNullOrEmpty(obj.AssetName))
                {
                    // Adding object with referenced geometry
                    instance = this.FindAssetInstance(obj.AssetMapId, obj.AssetId);
                }
                else
                {
                    // Adding object with it's own geometry
                    instance = this.FindObjectInstance(obj.AssetName, obj.Id);
                }

                if (instance != null)
                {
                    //Find emitters
                    var emitters = this.particleManager.ParticleSystems
                        .Where(p => p.Emitter.Instance == instance)
                        .Select(p => p.Emitter)
                        .ToArray();

                    //Find first state
                    var defaultState = obj.States?.FirstOrDefault()?.Name;

                    this.entities.Add(new ModularSceneryItem(obj, instance, emitters, defaultState));
                }
            }
        }

        /// <summary>
        /// Parse the assets map to set the assets transforms
        /// </summary>
        private void ParseAssetsMap(ModularSceneryLevel level)
        {
            this.assetMap = new AssetMap();

            var transforms = new Dictionary<string, List<Matrix>>();

            // Paser map for instance positioning
            foreach (var item in level.Map)
            {
                var assetIndex = Array.FindIndex(this.AssetConfiguration.Assets, a => a.Name == item.AssetName);
                if (assetIndex < 0)
                {
                    throw new EngineException(string.Format("Modular Scenery asset not found: {0}", item.AssetName));
                }

                this.ParseAssetReference(item, assetIndex, transforms);
            }

            foreach (var assetName in transforms.Keys)
            {
                this.assets[assetName].SetTransforms(transforms[assetName].ToArray());
            }

            this.assetMap.Build(this.AssetConfiguration, this.assets);
        }
        /// <summary>
        /// Parses the specified asset reference
        /// </summary>
        /// <param name="item">Reference</param>
        /// <param name="assetIndex">Asset index</param>
        /// <param name="transforms">Transforms dictionary</param>
        private void ParseAssetReference(ModularSceneryAssetReference item, int assetIndex, Dictionary<string, List<Matrix>> transforms)
        {
            var complexAssetTransform = item.GetTransform();
            var complexAssetRotation = item.Rotation;

            AssetMapItem aMap = new AssetMapItem()
            {
                Index = assetIndex,
                Name = item.AssetName,
                Transform = complexAssetTransform,
                Assets = new Dictionary<string, List<int>>(),
            };
            this.assetMap.Add(aMap);

            var asset = this.AssetConfiguration.Assets[assetIndex];
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
                var basicAssetType = Array.Find(asset.Assets, a => a.AssetName == basicAsset).Type;

                Array.ForEach(assetTransforms[basicAsset], t =>
                {
                    var basicTrn = t;

                    if (this.AssetConfiguration.MaintainTextureDirection)
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
        /// <param name="mapId">Asset map id</param>
        /// <param name="id">Asset id</param>
        /// <returns>Returns the model instance</returns>
        private ModelInstance FindAssetInstance(string mapId, string id)
        {
            // Find the assetName by object asset_id
            var res = this.CurrentLevel.FindAssetInstance(this.AssetConfiguration.Assets, mapId, id);
            if (res != null)
            {
                // Look for all geometry references
                int index = this.CurrentLevel.GetMapInstanceIndex(this.AssetConfiguration.Assets, res.AssetName, mapId, id);
                if (index >= 0)
                {
                    return this.assets[res.AssetName][index];
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
            var index = this.CurrentLevel.GetObjectInstanceIndex(assetName, id);
            if (index >= 0)
            {
                return this.objects[assetName][index];
            }

            return null;
        }

        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.assetMap?.Update(context.CameraVolume);

            if (this.activeCallbacks?.Any() == true)
            {
                UpdateTriggers();
            }
        }
        /// <summary>
        /// Verifies the active triggers states and fires the ending events
        /// </summary>
        private void UpdateTriggers()
        {
            this.activeCallbacks.ForEach(c =>
            {
                if (!c.Waiting)
                {
                    TriggerEnd?.Invoke(this, new ModularSceneryTriggerEventArgs()
                    {
                        StarterTrigger = c.Trigger,
                        StarterItem = c.Item,
                        Items = c.Items,
                    });
                }
            });

            this.activeCallbacks.RemoveAll(c => !c.Waiting);
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public override BoundingSphere GetBoundingSphere()
        {
            var res = new BoundingSphere();
            bool initialized = false;

            foreach (var item in this.objects.Keys)
            {
                var model = this.objects[item];
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < model.InstanceCount; i++)
                {
                    var bsph = model[i].GetBoundingSphere();

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
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public override BoundingBox GetBoundingBox()
        {
            var res = new BoundingBox();
            bool initialized = false;

            foreach (var item in this.objects.Keys)
            {
                var model = this.objects[item];
                if (model == null)
                {
                    continue;
                }

                for (int i = 0; i < model.InstanceCount; i++)
                {
                    var bbox = model[i].GetBoundingBox();

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

        /// <summary>
        /// Gets the ground volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns all the triangles of the ground</returns>
        public override IEnumerable<Triangle> GetVolume(bool full)
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (var asset in this.assets.Values)
            {
                var instances = asset.GetInstances().Where(i => i.Visible);

                foreach (var instance in instances)
                {
                    triangles.AddRange(instance.GetVolume(false));
                }
            }

            return triangles.ToArray();
        }

        /// <summary>
        /// Gets all complex map asset volumes
        /// </summary>
        /// <returns>Returns a dictionary of complex asset volumes by asset name</returns>
        public Dictionary<string, List<BoundingBox>> GetMapVolumes()
        {
            return this.assetMap.GetMapVolumes();
        }
        /// <summary>
        /// Gets all individual map asset volumes
        /// </summary>
        /// <returns>Returns a dictionary of individual asset volumes by asset name</returns>
        public Dictionary<string, List<BoundingBox>> GetMapAssetsVolumes()
        {
            var res = new Dictionary<string, List<BoundingBox>>();

            foreach (var item in this.assets.Keys)
            {
                res.Add(item, new List<BoundingBox>());

                for (int i = 0; i < this.assets[item].InstanceCount; i++)
                {
                    res[item].Add(this.assets[item][i].GetBoundingBox());
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

            foreach (var item in this.objects.Keys)
            {
                var model = this.objects[item];
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
            var obj = this.entities
                .FirstOrDefault(o => string.Equals(o.Object.Id, id, StringComparison.OrdinalIgnoreCase));

            return obj;
        }

        /// <summary>
        /// Get objects by name
        /// </summary>
        /// <param name="name">Object name</param>
        /// <returns>Returns a list of objects by name</returns>
        public IEnumerable<ModularSceneryItem> GetObjectsByName(string name)
        {
            var res = this.entities
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
            var res = this.entities
                .Where(o => o.Object.Type == objectType);

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
                return new ModularSceneryTrigger[] { };
            }

            return this.triggers[item.Item]
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
            if (animations.ContainsKey(item.Item) && animations[item.Item].ContainsKey(trigger.AnimationPlan))
            {
                var plan = animations[item.Item]?[trigger.AnimationPlan];

                item.Item.AnimationController.SetPath(plan);
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
                var refTrigger = triggers[refItem.Item].FirstOrDefault(t => string.Equals(t.Name, action.ItemAction, StringComparison.OrdinalIgnoreCase));
                if (refTrigger == null)
                {
                    continue;
                }

                ExecuteTriggerInternal(callback, refItem, refTrigger);
            }
        }

        /// <summary>
        /// Gets the culling volume for scene culling tests
        /// </summary>
        /// <returns>Return the culling volume</returns>
        public override ICullingVolume GetCullingVolume()
        {
            return this.assetMap;
        }

        /// <summary>
        /// Asset map
        /// </summary>
        class AssetMap : ICullingVolume
        {
            /// <summary>
            /// Asset map
            /// </summary>
            private readonly List<AssetMapItem> assetMap = new List<AssetMapItem>();
            /// <summary>
            /// Visible bounding boxes
            /// </summary>
            private readonly List<BoundingBox> visibleBoxes = new List<BoundingBox>();

            /// <summary>
            /// Position
            /// </summary>
            public Vector3 Position
            {
                get
                {
                    return Vector3.Zero;
                }
            }

            /// <summary>
            /// Gets if the current volume contains the bounding frustum
            /// </summary>
            /// <param name="frustum">Bounding frustum</param>
            /// <returns>Returns the containment type</returns>
            public ContainmentType Contains(BoundingFrustum frustum)
            {
                for (int i = 0; i < this.visibleBoxes.Count; i++)
                {
                    var res = frustum.Contains(this.visibleBoxes[i]);

                    if (res != ContainmentType.Disjoint) return res;
                }

                return ContainmentType.Disjoint;
            }
            /// <summary>
            /// Gets if the current volume contains the bounding box
            /// </summary>
            /// <param name="bbox">Bounding box</param>
            /// <returns>Returns the containment type</returns>
            public ContainmentType Contains(BoundingBox bbox)
            {
                for (int i = 0; i < this.visibleBoxes.Count; i++)
                {
                    var res = this.visibleBoxes[i].Contains(bbox);

                    if (res != ContainmentType.Disjoint) return res;
                }

                return ContainmentType.Disjoint;
            }
            /// <summary>
            /// Gets if the current volume contains the bounding sphere
            /// </summary>
            /// <param name="sph">Bounding sphere</param>
            /// <returns>Returns the containment type</returns>
            public ContainmentType Contains(BoundingSphere sph)
            {
                for (int i = 0; i < this.visibleBoxes.Count; i++)
                {
                    var res = this.visibleBoxes[i].Contains(sph);

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
                this.assetMap.Add(item);
            }
            /// <summary>
            /// Builds the asset map
            /// </summary>
            /// <param name="assetConfiguration">Configuration</param>
            /// <param name="assets">Asset list</param>
            public void Build(ModularSceneryAssetConfiguration assetConfiguration, Dictionary<string, ModelInstanced> assets)
            {
                //Fill per complex asset bounding boxes
                Fill(assets);

                //Find connections
                for (int s = 0; s < this.assetMap.Count; s++)
                {
                    for (int t = s + 1; t < this.assetMap.Count; t++)
                    {
                        var source = this.assetMap[s];
                        var target = this.assetMap[t];

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
                for (int i = 0; i < this.assetMap.Count; i++)
                {
                    var item = this.assetMap[i];

                    BoundingBox bbox = new BoundingBox();

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
            private void FindPortals(ModularSceneryAssetConfiguration assetConfiguration, AssetMapItem source, AssetMapItem target, int s, int t)
            {
                var sourceConf = Array.Find(assetConfiguration.Assets, (a => a.Name == source.Name));
                if (sourceConf.Connections?.Length > 0)
                {
                    var targetConf = Array.Find(assetConfiguration.Assets, (a => a.Name == target.Name));
                    if (targetConf.Connections?.Length > 0)
                    {
                        //Transform connection positions and directions
                        var sourcePositions = sourceConf.Connections.Select(i => Vector3.TransformCoordinate(i.Position, source.Transform));
                        var targetPositions = targetConf.Connections.Select(i => Vector3.TransformCoordinate(i.Position, target.Transform));

                        if (sourcePositions.Any(p1 => targetPositions.Contains(p1)))
                        {
                            source.Connections.Add(t);
                            target.Connections.Add(s);
                        }
                    }
                }
            }

            /// <summary>
            /// Gets all complex map asset volumes
            /// </summary>
            /// <returns>Returns a dictionary of complex asset volumes by asset name</returns>
            public Dictionary<string, List<BoundingBox>> GetMapVolumes()
            {
                var res = new Dictionary<string, List<BoundingBox>>();

                for (int i = 0; i < this.assetMap.Count; i++)
                {
                    var item = this.assetMap[i];

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
            public void Update(CullingVolumeCamera camera)
            {
                //Find current box
                var itemIndex = this.assetMap.FindIndex(b => b.Volume.Contains(camera.Position) != ContainmentType.Disjoint);
                if (itemIndex >= 0)
                {
                    this.visibleBoxes.Clear();
                    this.visibleBoxes.Add(this.assetMap[itemIndex].Volume);

                    List<int> visited = new List<int>
                    {
                        itemIndex
                    };

                    foreach (var conIndex in this.assetMap[itemIndex].Connections)
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
            private void UpdateItem(CullingVolumeCamera camera, int itemIndex, List<int> visited)
            {
                visited.Add(itemIndex);

                if (camera.Contains(this.assetMap[itemIndex].Volume) != ContainmentType.Disjoint)
                {
                    this.visibleBoxes.Add(this.assetMap[itemIndex].Volume);

                    foreach (var conIndex in this.assetMap[itemIndex].Connections)
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
            public Dictionary<string, List<int>> Assets = new Dictionary<string, List<int>>();
            /// <summary>
            /// Connections with other complex assets
            /// </summary>
            public List<int> Connections = new List<int>();

            /// <summary>
            /// Gets the instance text representation
            /// </summary>
            /// <returns>Returns the instance text representation</returns>
            public override string ToString()
            {
                return string.Format("{0}; Index: {1}; Connections: {2};", this.Name, this.Index, this.Connections.Count);
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
                    return Items.Any(i => i.Item.AnimationController?.Playing == true);
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
    }

    /// <summary>
    /// Modular scenery extensions
    /// </summary>
    public static class ModularSceneryExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<ModularScenery> AddComponentModularScenery(this Scene scene, ModularSceneryDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            ModularScenery component = null;

            await Task.Run(() =>
            {
                component = new ModularScenery(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
