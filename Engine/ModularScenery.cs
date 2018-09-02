using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
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
        private readonly Dictionary<string, SceneObject<ModelInstanced>> assets = new Dictionary<string, SceneObject<ModelInstanced>>();
        /// <summary>
        /// Object models dictionary
        /// </summary>
        private readonly Dictionary<string, SceneObject<ModelInstanced>> objects = new Dictionary<string, SceneObject<ModelInstanced>>();
        /// <summary>
        /// Particle descriptors dictionary
        /// </summary>
        private readonly Dictionary<string, ParticleSystemDescription> particleDescriptors = new Dictionary<string, ParticleSystemDescription>();
        /// <summary>
        /// Particle manager
        /// </summary>
        private SceneObject<ParticleManager> particleManager = null;
        /// <summary>
        /// Asset map
        /// </summary>
        private AssetMap assetMap = null;
        /// <summary>
        /// Scenery entities
        /// </summary>
        private List<ModularSceneryItem> entities = new List<ModularSceneryItem>();

        /// <summary>
        /// Gets the assets description
        /// </summary>
        protected ModularSceneryAssetConfiguration AssetConfiguration { get; set; }
        /// <summary>
        /// Gets the level list
        /// </summary>
        protected ModularSceneryLevels Levels { get; set; }
      
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

            this.InitializeParticles();

            this.CurrentLevel = this.Levels.Levels[0];

            this.LoadLevel(this.CurrentLevel);
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
        public void LoadLevel(string levelName)
        {
            //Removes previous level components from scene
            this.Scene.RemoveComponents(this.assets.Select(a => a.Value));
            this.Scene.RemoveComponents(this.objects.Select(o => o.Value));

            //Clear internal lists and data
            this.assets.Clear();
            this.objects.Clear();
            this.entities.Clear();
            this.assetMap = null;
            this.particleManager.Instance.Clear();

            //Find the level
            var level = this.Levels.Levels
                .Where(l => string.Equals(l.Name, levelName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (level != null)
            {
                this.CurrentLevel = level;

                //Load the level
                this.LoadLevel(level);
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
                var loader = contentDesc.GetLoader();
                var t = loader.Load(Description.Content.ContentFolder, contentDesc);
                content = t[0];
            }
            else if (Description.Content.ModelContentDescription != null)
            {
                var loader = Description.Content.ModelContentDescription.GetLoader();
                var t = loader.Load(Description.Content.ContentFolder, Description.Content.ModelContentDescription);
                content = t[0];
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
        private void LoadLevel(ModularSceneryLevel level)
        {
            ModelContent content = LoadModelContent();

            this.InitializeAssets(level, content);
            this.InitializeObjects(level, content);

            this.ParseAssetsMap(level);

            this.InitializeEntities(level);
        }
        /// <summary>
        /// Initialize the particle system and the particle descriptions
        /// </summary>
        private void InitializeParticles()
        {
            if (this.Levels.ParticleSystems != null && this.Levels.ParticleSystems.Length > 0)
            {
                this.particleManager = this.Scene.AddComponent<ParticleManager>(
                    new ParticleManagerDescription()
                    {
                        Name = string.Format("{0}.{1}", this.Description.Name, "Particle Manager"),
                    },
                    SceneObjectUsageEnum.None,
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
        private void InitializeAssets(ModularSceneryLevel level, ModelContent content)
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

                        var model = this.Scene.AddComponent<ModelInstanced>(
                            new ModelInstancedDescription()
                            {
                                Name = string.Format("{0}.{1}.{2}", this.Description.Name, assetName, level.Name),
                                CastShadow = this.Description.CastShadow,
                                UseAnisotropicFiltering = this.Description.UseAnisotropic,
                                Instances = count,
                                Content = new ContentDescription()
                                {
                                    ModelContent = modelContent,
                                }
                            },
                            SceneObjectUsageEnum.Ground | (hasVolumes ? SceneObjectUsageEnum.CoarsePathFinding : SceneObjectUsageEnum.FullPathFinding));

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
        private void InitializeObjects(ModularSceneryLevel level, ModelContent content)
        {
            // Set auto-identifiers
            level.PopulateObjectIds();

            // Get instance count for all single geometries from Map
            var instances = level.GetObjectsInstanceCounters();

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

                        var model = this.Scene.AddComponent<ModelInstanced>(
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
                            SceneObjectUsageEnum.None | (hasVolumes ? SceneObjectUsageEnum.CoarsePathFinding : SceneObjectUsageEnum.FullPathFinding));

                        //Get the object list to process
                        var objList = Array.FindAll(level.Objects, o => string.Equals(o.AssetName, assetName, StringComparison.OrdinalIgnoreCase));

                        //Positioning
                        model.Instance.SetTransforms(objList.Select(o => o.GetTransform()).ToArray());

                        for (int i = 0; i < model.Instance.Count; i++)
                        {
                            if (objList[i].LoadLights)
                            {
                                var trn = model.Instance[i].Manipulator.LocalTransform;

                                var lights = model.Instance[i].Lights;
                                if (lights != null && lights.Length > 0)
                                {
                                    var emitterDesc = objList[i].ParticleLight;

                                    foreach (var light in lights)
                                    {
                                        light.CastShadow = objList[i].CastShadows;

                                        if (emitterDesc != null)
                                        {
                                            if (light is SceneLightPoint pointL)
                                            {
                                                var pos = Vector3.TransformCoordinate(pointL.Position, trn);

                                                var emitter = new ParticleEmitter(emitterDesc)
                                                {
                                                    Position = pos,
                                                };

                                                this.particleManager.Instance.AddParticleSystem(
                                                    ParticleSystemTypes.CPU,
                                                    this.particleDescriptors[emitterDesc.Name],
                                                    emitter);
                                            }
                                        }
                                    }

                                    this.Scene.Lights.AddRange(lights);
                                }
                            }
                        }

                        this.objects.Add(assetName, model);
                    }
                }
            }
        }
        /// <summary>
        /// Initialize scenery entities proxy list
        /// </summary>
        private void InitializeEntities(ModularSceneryLevel level)
        {
            foreach (var obj in level.Objects)
            {
                if (string.IsNullOrEmpty(obj.AssetName))
                {
                    // Adding object with referenced geometry
                    var instance = this.FindAssetInstance(obj.AssetMapId, obj.AssetId);
                    if (instance != null)
                    {
                        this.entities.Add(new ModularSceneryItem(obj, instance));
                    }
                }
                else
                {
                    // Adding object with it's own geometry
                    var instance = this.FindObjectInstance(obj.AssetName, obj.Id);
                    if (instance != null)
                    {
                        this.entities.Add(new ModularSceneryItem(obj, instance));
                    }
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
                            if (basicAssetType == ModularSceneryAssetTypeEnum.Floor ||
                                basicAssetType == ModularSceneryAssetTypeEnum.Ceiling)
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

            foreach (var assetName in transforms.Keys)
            {
                this.assets[assetName].Instance.SetTransforms(transforms[assetName].ToArray());
            }

            this.assetMap.Build(this.AssetConfiguration, this.assets);
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
                    return this.assets[res.AssetName].Instance[index];
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
                return this.objects[assetName].Instance[index];
            }

            return null;
        }

        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.assetMap.Update(context.CameraVolume);
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {

        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public override BoundingSphere GetBoundingSphere()
        {
            var res = new BoundingSphere();

            foreach (var item in this.objects.Keys)
            {
                var model = this.objects[item];
                if (model != null)
                {
                    for (int i = 0; i < model.Instance.Count; i++)
                    {
                        var bsph = model.Instance[i].GetBoundingSphere();

                        if (res == new BoundingSphere())
                        {
                            res = bsph;
                        }
                        else
                        {
                            res = BoundingSphere.Merge(res, bsph);
                        }
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

            foreach (var item in this.objects.Keys)
            {
                var model = this.objects[item];
                if (model != null)
                {
                    for (int i = 0; i < model.Instance.Count; i++)
                    {
                        var bbox = model.Instance[i].GetBoundingBox();

                        if (res == new BoundingBox())
                        {
                            res = bbox;
                        }
                        else
                        {
                            res = BoundingBox.Merge(res, bbox);
                        }
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
        public override Triangle[] GetVolume(bool full)
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (var asset in this.assets.Values)
            {
                foreach (var instance in asset.Instance.GetInstances())
                {
                    if (instance.Visible)
                    {
                        triangles.AddRange(instance.GetTriangles());
                    }
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

                for (int i = 0; i < this.assets[item].Instance.Count; i++)
                {
                    res[item].Add(this.assets[item].Instance[i].GetBoundingBox());
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

                    for (int i = 0; i < model.Instance.Count; i++)
                    {
                        res[item].Add(model.Instance[i].GetBoundingBox());
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Gets a position array of the specified object instances
        /// </summary>
        /// <param name="name">Object name</param>
        /// <returns>Returns a position array of the specified object instances</returns>
        public Vector3[] GetObjectsPositionsByAssetName(string name)
        {
            var assets = this.GetObjectsByName(name);

            return assets.Select(a => a.Manipulator.Position).ToArray();
        }
        /// <summary>
        /// Get objects by name
        /// </summary>
        /// <param name="name">Object name</param>
        /// <returns>Returns a list of objects by name</returns>
        public ModelInstance[] GetObjectsByName(string name)
        {
            var objs = this.entities
                .Where(o => string.Equals(o.Object.AssetName, name, StringComparison.OrdinalIgnoreCase))
                .Select(o => o.Item);

            return objs.ToArray();
        }
        /// <summary>
        /// Gets objects by type
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <returns>Returns a list of objects of the specified type</returns>
        public ModelInstance[] GetObjectsByType(ModularSceneryObjectTypeEnum objectType)
        {
            var objs = this.entities
                .Where(o => o.Object.Type == objectType)
                .Select(o => o.Item);

            return objs.ToArray();
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public ModularSceneryItem[] GetObjectsInVolume(BoundingBox bbox, bool sortByDistance)
        {
            return GetObjects(bbox, null, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public ModularSceneryItem[] GetObjectsInVolume(BoundingBox bbox, ModularSceneryObjectTypeEnum filter, bool sortByDistance)
        {
            return GetObjects(bbox, filter, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        private ModularSceneryItem[] GetObjects(BoundingBox bbox, ModularSceneryObjectTypeEnum? filter, bool sortByDistance)
        {
            List<ModularSceneryItem> res = new List<ModularSceneryItem>();

            for (int i = 0; i < entities.Count; i++)
            {
                if (!filter.HasValue || filter.Value.HasFlag(entities[i].Object.Type))
                {
                    if (bbox.Intersects(entities[i].Item.GetBoundingBox()))
                    {
                        res.Add(entities[i]);
                    }
                }
            }

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
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public ModularSceneryItem[] GetObjectsInVolume(BoundingSphere sphere, bool sortByDistance)
        {
            return GetObjects(sphere, null, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        public ModularSceneryItem[] GetObjectsInVolume(BoundingSphere sphere, ModularSceneryObjectTypeEnum filter, bool sortByDistance)
        {
            return GetObjects(sphere, filter, sortByDistance);
        }
        /// <summary>
        /// Gets objects into the specified volume
        /// </summary>
        /// <param name="sphere">Bounding sphere</param>
        /// <param name="filter">Filter by entity type</param>
        /// <param name="sortByDistance">Sorts the resulting array by distance</param>
        /// <returns>Gets an array of objects into the specified volume</returns>
        private ModularSceneryItem[] GetObjects(BoundingSphere sphere, ModularSceneryObjectTypeEnum? filter, bool sortByDistance)
        {
            List<ModularSceneryItem> res = new List<ModularSceneryItem>();

            for (int i = 0; i < entities.Count; i++)
            {
                if (!filter.HasValue || filter.Value.HasFlag(entities[i].Object.Type))
                {
                    if (sphere.Intersects(entities[i].Item.GetBoundingSphere()))
                    {
                        res.Add(entities[i]);
                    }
                }
            }

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
            public void Build(ModularSceneryAssetConfiguration assetConfiguration, Dictionary<string, SceneObject<ModelInstanced>> assets)
            {
                //Fill per complex asset bounding boxex
                for (int i = 0; i < this.assetMap.Count; i++)
                {
                    var item = this.assetMap[i];

                    BoundingBox bbox = new BoundingBox();

                    foreach (var assetName in item.Assets.Keys)
                    {
                        foreach (int assetIndex in item.Assets[assetName])
                        {
                            var aBbox = assets[assetName].Instance[assetIndex].GetBoundingBox();

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
                            var sourceConf = Array.Find(assetConfiguration.Assets, (a => a.Name == source.Name));
                            if (sourceConf.Connections != null && sourceConf.Connections.Length > 0)
                            {
                                var targetConf = Array.Find(assetConfiguration.Assets, (a => a.Name == target.Name));
                                if (targetConf.Connections != null && targetConf.Connections.Length > 0)
                                {
                                    //Transform connection positions and directions
                                    var sourcePositions = sourceConf.Connections.Select(i => Vector3.TransformCoordinate(i.Position, source.Transform));
                                    var targetPositions = targetConf.Connections.Select(i => Vector3.TransformCoordinate(i.Position, target.Transform));

                                    if (sourcePositions.Count(p1 => targetPositions.Contains(p1)) > 0)
                                    {
                                        source.Connections.Add(t);
                                        target.Connections.Add(s);
                                    }
                                }
                            }
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
    }
}
