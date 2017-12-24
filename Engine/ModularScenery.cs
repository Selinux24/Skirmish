using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

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

            ModularSceneryAssetConfiguration assetsConfiguration = null;

            if (description.AssetsConfiguration != null)
            {
                assetsConfiguration = description.AssetsConfiguration;
            }
            else if (!string.IsNullOrWhiteSpace(description.AssetsConfigurationFile))
            {
                assetsConfiguration = Helper.DeserializeFromFile<ModularSceneryAssetConfiguration>(Path.Combine(description.Content.ContentFolder, description.AssetsConfigurationFile));
            }

            this.InitializeAssets(content, assetsConfiguration);

            this.ParseAssetsMap(assetsConfiguration);
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        public override void Dispose()
        {

        }

        /// <summary>
        /// Initialize all assets into asset dictionary 
        /// </summary>
        /// <param name="content">Assets model content</param>
        /// <param name="assetsConfiguration">Assets configuration</param>
        private void InitializeAssets(ModelContent content, ModularSceneryAssetConfiguration assetsConfiguration)
        {
            // Get instance count for all single geometries from Map
            var instanceCounter = assetsConfiguration.GetInstanceCounter();

            // Load all single geometries into single instanced model components
            foreach (var assetName in instanceCounter.Keys)
            {
                var instances = instanceCounter[assetName];

                var model = this.Scene.AddComponent<ModelInstanced>(
                    new ModelInstancedDescription()
                    {
                        Name = string.Format("{0}.{1}", this.Description.Name, assetName),
                        CastShadow = this.Description.CastShadow,
                        UseAnisotropicFiltering = this.Description.UseAnisotropic,
                        Instances = instances,
                        Content = new ContentDescription()
                        {
                            ModelContent = content.FilterMask(assetName),
                        }
                    },
                    SceneObjectUsageEnum.Ground | SceneObjectUsageEnum.FullPathFinding);

                this.assets.Add(assetName, model);
            }
        }
        /// <summary>
        /// Parse the assets map to set the assets transforms
        /// </summary>
        /// <param name="assetsConfiguration">Assets configuration</param>
        private void ParseAssetsMap(ModularSceneryAssetConfiguration assetsConfiguration)
        {
            var transforms = new Dictionary<string, List<Matrix>>();

            // Paser map for instance positioning
            foreach (var item in assetsConfiguration.Map)
            {
                var asset = Array.Find(assetsConfiguration.Assets, a => a.Name == item.AssetName);

                var complexAssetTransform = item.GetTransform();
                var complexAssetRotation = item.Rotation;

                var assetTransforms = asset.GetInstanceTransforms();

                foreach (var basicAsset in assetTransforms.Keys)
                {
                    //Get basic asset type
                    var basicAssetType = Array.Find(asset.Assets, a => a.AssetName == basicAsset).Type;

                    if (!transforms.ContainsKey(basicAsset))
                    {
                        transforms.Add(basicAsset, new List<Matrix>());
                    }

                    var trnList = new List<Matrix>();
                    Array.ForEach(assetTransforms[basicAsset], t =>
                    {
                        var basicTrn = t;

                        if (assetsConfiguration.MaintainTextureDirection)
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

                        trnList.Add(basicTrn * complexAssetTransform);
                    });

                    transforms[basicAsset].AddRange(trnList);
                }
            }

            foreach (var assetName in transforms.Keys)
            {
                this.assets[assetName].Instance.SetTransforms(transforms[assetName].ToArray());
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
    }
}
