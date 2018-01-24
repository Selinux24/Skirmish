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
        /// Assets dictionary
        /// </summary>
        private Dictionary<string, SceneObject<ModelInstanced>> assets = new Dictionary<string, SceneObject<ModelInstanced>>();
        /// <summary>
        /// Objects dictionary
        /// </summary>
        private Dictionary<string, SceneObject<ModelInstanced>> objects = new Dictionary<string, SceneObject<ModelInstanced>>();
        /// <summary>
        /// Particle manager
        /// </summary>
        private SceneObject<ParticleManager> particleManager = null;
        /// <summary>
        /// Particle descriptors dictionary
        /// </summary>
        private Dictionary<string, ParticleSystemDescription> particleDescriptors = new Dictionary<string, ParticleSystemDescription>();
        /// <summary>
        /// Asset map
        /// </summary>
        private List<AssetMap> assetMap = new List<AssetMap>();

        /// <summary>
        /// Gets the assets description
        /// </summary>
        protected virtual ModularSceneryAssetConfiguration AssetConfiguration { get; set; }

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
            ModelContent content = null;

            if (!string.IsNullOrEmpty(description.Content.ModelContentFilename))
            {
                var contentDesc = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(description.Content.ContentFolder, description.Content.ModelContentFilename));

                var t = LoaderCOLLADA.Load(description.Content.ContentFolder, contentDesc);
                content = t[0];
            }
            else if (description.Content.ModelContentDescription != null)
            {
                var t = LoaderCOLLADA.Load(description.Content.ContentFolder, description.Content.ModelContentDescription);
                content = t[0];
            }
            else if (description.Content.HeightmapDescription != null)
            {
                content = ModelContent.FromHeightmap(
                    description.Content.HeightmapDescription.ContentPath,
                    description.Content.HeightmapDescription.HeightmapFileName,
                    description.Content.HeightmapDescription.Textures.TexturesLR,
                    description.Content.HeightmapDescription.CellSize,
                    description.Content.HeightmapDescription.MaximumHeight);
            }
            else if (description.Content.ModelContent != null)
            {
                content = description.Content.ModelContent;
            }

            if (description.AssetsConfiguration != null)
            {
                this.AssetConfiguration = description.AssetsConfiguration;
            }
            else if (!string.IsNullOrWhiteSpace(description.AssetsConfigurationFile))
            {
                this.AssetConfiguration = Helper.DeserializeFromFile<ModularSceneryAssetConfiguration>(Path.Combine(description.Content.ContentFolder, description.AssetsConfigurationFile));
            }

            this.InitializeParticles();

            this.InitializeAssets(content);
            this.InitializeObjects(content);

            this.ParseAssetsMap();
            this.BuildAssetsMap();
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        public override void Dispose()
        {

        }

        /// <summary>
        /// Initialize the particle system and the particle descriptions
        /// </summary>
        private void InitializeParticles()
        {
            if (this.AssetConfiguration.ParticleSystems != null && this.AssetConfiguration.ParticleSystems.Length > 0)
            {
                this.particleManager = this.Scene.AddComponent<ParticleManager>(
                    new ParticleManagerDescription()
                    {
                        Name = string.Format("{0}.{1}", this.Description.Name, "Particle Manager"),
                    },
                    SceneObjectUsageEnum.None,
                    98);

                foreach (var item in this.AssetConfiguration.ParticleSystems)
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
        private void InitializeAssets(ModelContent content)
        {
            // Get instance count for all single geometries from Map
            var instances = this.AssetConfiguration.GetMapInstanceCounters();

            // Load all single geometries into single instanced model components
            foreach (var assetName in instances.Keys)
            {
                var count = instances[assetName];
                if (count > 0)
                {
                    var modelContent = content.FilterMask(assetName);
                    if (modelContent != null)
                    {
                        var masks = this.AssetConfiguration.GetMasksForAsset(assetName);
                        var hasVolumes = modelContent.SetVolumeMark(true, masks) > 0;

                        var model = this.Scene.AddComponent<ModelInstanced>(
                            new ModelInstancedDescription()
                            {
                                Name = string.Format("{0}.{1}", this.Description.Name, assetName),
                                CastShadow = this.Description.CastShadow,
                                UseAnisotropicFiltering = this.Description.UseAnisotropic,
                                Instances = count,
                                Content = new ContentDescription()
                                {
                                    ModelContent = modelContent,
                                }
                            },
                            SceneObjectUsageEnum.Ground | (hasVolumes ? SceneObjectUsageEnum.CoarsePathFinding : SceneObjectUsageEnum.FullPathFinding));

                        this.assets.Add(assetName, model);
                    }
                }
            }
        }
        /// <summary>
        /// Initialize all objects into asset dictionary 
        /// </summary>
        /// <param name="content">Assets model content</param>
        private void InitializeObjects(ModelContent content)
        {
            // Get instance count for all single geometries from Map
            var instances = this.AssetConfiguration.GetObjectsInstanceCounters();

            // Load all single geometries into single instanced model components
            foreach (var assetName in instances.Keys)
            {
                var count = instances[assetName];
                if (count > 0)
                {
                    var modelContent = content.FilterMask(assetName);
                    if (modelContent != null)
                    {
                        var masks = this.AssetConfiguration.GetMasksForAsset(assetName);
                        var hasVolumes = modelContent.SetVolumeMark(true, masks) > 0;

                        var model = this.Scene.AddComponent<ModelInstanced>(
                            new ModelInstancedDescription()
                            {
                                Name = string.Format("{0}.{1}", this.Description.Name, assetName),
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
                        var objList = Array.FindAll(this.AssetConfiguration.Objects, o => string.Equals(o.AssetName, assetName, StringComparison.OrdinalIgnoreCase));

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
                                            var pointL = light as SceneLightPoint;
                                            if (pointL != null)
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
        /// Parse the assets map to set the assets transforms
        /// </summary>
        private void ParseAssetsMap()
        {
            var transforms = new Dictionary<string, List<Matrix>>();

            // Paser map for instance positioning
            foreach (var item in this.AssetConfiguration.Map)
            {
                var assetIndex = Array.FindIndex(this.AssetConfiguration.Assets, a => a.Name == item.AssetName);
                if (assetIndex < 0)
                {
                    throw new EngineException(string.Format("Modular Scenery asset not found: {0}", item.AssetName));
                }

                var complexAssetTransform = item.GetTransform();
                var complexAssetRotation = item.Rotation;

                AssetMap aMap = new AssetMap()
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
                                basicAssetType == ModularSceneryAssetTypeEnum.Ceiling ||
                                basicAssetType == ModularSceneryAssetTypeEnum.TrapFloor ||
                                basicAssetType == ModularSceneryAssetTypeEnum.TrapCeiling)
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
        }
        /// <summary>
        /// Builds the asset map
        /// </summary>
        private void BuildAssetsMap()
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
                        var aBbox = this.assets[assetName].Instance[assetIndex].GetBoundingBox();

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
                        var sourceConf = Array.Find(this.AssetConfiguration.Assets, (a => a.Name == source.Name));
                        if (sourceConf.Connections != null && sourceConf.Connections.Length > 0)
                        {
                            var targetConf = Array.Find(this.AssetConfiguration.Assets, (a => a.Name == target.Name));
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
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {

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
                for (int i = 0; i < this.objects[item].Instance.Count; i++)
                {
                    var bsph = this.objects[item].Instance[i].GetBoundingSphere();

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
                for (int i = 0; i < this.objects[item].Instance.Count; i++)
                {
                    var bbox = this.objects[item].Instance[i].GetBoundingBox();

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
                    triangles.AddRange(instance.GetTriangles());
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
                res.Add(item, new List<BoundingBox>());

                for (int i = 0; i < this.objects[item].Instance.Count; i++)
                {
                    res[item].Add(this.objects[item].Instance[i].GetBoundingBox());
                }
            }

            return res;
        }

        /// <summary>
        /// Gets a position array of the specified asset instances
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>Returns a position array of the specified asset instances</returns>
        public Vector3[] GetAssetPositionsByName(string assetName)
        {
            List<Vector3> res = new List<Vector3>();

            var assets = this.objects
                .Where(o => string.Equals(o.Key, assetName, StringComparison.OrdinalIgnoreCase))
                .Select(o => o.Value);

            foreach (var item in assets)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    res.Add(item.Instance[i].Manipulator.Position);
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        /// <remarks>By default, returns true and distance = float.MaxValue</remarks>
        public override bool Cull(BoundingFrustum frustum, out float? distance)
        {
            //TODO: Use asset map instead of default.
            return base.Cull(frustum, out distance);
        }

        /// <summary>
        /// Asset map item
        /// </summary>
        class AssetMap
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
