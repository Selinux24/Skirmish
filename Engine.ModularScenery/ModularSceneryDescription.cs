using Engine.Content;
using System.Collections.Generic;

namespace Engine.Modular
{
    using Engine.Modular.Persistence;

    /// <summary>
    /// Modular scenery description
    /// </summary>
    public class ModularSceneryDescription : GroundDescription
    {
        /// <summary>
        /// Gets a modular scenery from a folder
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentFile">Content asset collection file</param>
        /// <param name="assetsMapFile">Assets map file</param>
        /// <param name="levelsMapFile">Levels map file</param>
        /// <remarks>
        /// The <paramref name="contentFolder"/> folder must contains the following files:
        /// - <paramref name="contentFile"/> for asset collection in the ContentDataFile format
        /// - <paramref name="assetsMapFile"/> for asset groups definition, like rooms or corridors, in the ModularSceneryAssetConfiguration format
        /// - <paramref name="levelsMapFile"/> for level description in the ModularSceneryLevels format
        /// </remarks>
        public static ModularSceneryDescription FromFolder(string contentFolder, string contentFile, string assetsMapFile, string levelsMapFile)
        {
            return new ModularSceneryDescription()
            {
                Content = ContentDescription.FromFile(contentFolder, contentFile),
                AssetsConfigurationFile = assetsMapFile,
                LevelsFile = levelsMapFile,

                UseAnisotropic = true,
                CastShadow = ShadowCastingAlgorihtms.All,
                ColliderType = ColliderTypes.None,
                PathFindingHull = PickingHullTypes.None,
                CullingVolumeType = CullingVolumeTypes.None,
                PickingHull = PickingHullTypes.None,
            };
        }

        /// <summary>
        /// Assets configuration file
        /// </summary>
        public string AssetsConfigurationFile { get; set; } = null;
        /// <summary>
        /// Assets configuration
        /// </summary>
        public AssetMap AssetsConfiguration { get; set; } = null;
        /// <summary>
        /// Levels file
        /// </summary>
        public string LevelsFile { get; set; } = null;
        /// <summary>
        /// Levels
        /// </summary>
        public LevelMap Levels { get; set; } = null;

        /// <summary>
        /// Gets the assets map
        /// </summary>
        public AssetMap GetAssetMap()
        {
            if (AssetsConfiguration != null)
            {
                return AssetsConfiguration;
            }

            if (!string.IsNullOrWhiteSpace(AssetsConfigurationFile))
            {
                AssetsConfiguration = AssetMap.FromFile(Content.ContentFolder ?? string.Empty, AssetsConfigurationFile);

                return AssetsConfiguration;
            }

            return default;
        }
        /// <summary>
        /// Gets the levels map
        /// </summary>
        public LevelMap GetLevelMap()
        {
            if (Levels != null)
            {
                return Levels;
            }

            if (!string.IsNullOrWhiteSpace(LevelsFile))
            {
                Levels = LevelMap.FromFile(Content.ContentFolder ?? string.Empty, LevelsFile);

                return Levels;
            }

            return default;
        }

        /// <summary>
        /// Creates a particle system description list
        /// </summary>
        public IEnumerable<(string Name, ParticleSystemDescription SystemDescription)> GetLevelParticleSystems()
        {
            var levelMap = GetLevelMap();

            foreach (var item in levelMap.ParticleSystems)
            {
                string contentPath = item.ContentPath ?? Content.ContentFolder;

                var desc = ParticleSystemDescription.Initialize(item, contentPath);

                yield return new(item.Name, desc);
            }
        }
        /// <summary>
        /// Creates a model description list for the assets of the specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="contentLibrary">Content library</param>
        public IEnumerable<(string Name, ModelInstancedDescription ModelDescription)> GetLevelAssets(Level level, ContentLibrary contentLibrary)
        {
            var assetMap = GetAssetMap();

            // Get instance count for all single geometries from Map
            var instances = level.GetMapInstanceCounters(assetMap.Assets);

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

                yield return InitializeAsset(assetName, count, level, modelContent, instances[assetName].PathFinding);
            }
        }
        /// <summary>
        /// Creates a new instanced model description for the asset
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="count">Instance count</param>
        /// <param name="level">Level</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="pathFinding">Path finding</param>
        private (string Name, ModelInstancedDescription ModelDescription) InitializeAsset(string assetName, int count, Level level, ContentData modelContent, PathFindingModes pathFinding)
        {
            var assetId = $"Asset.{assetName}.{level.Name}";

            var pf = pathFinding switch
            {
                PathFindingModes.None => PickingHullTypes.None,
                PathFindingModes.Coarse => PickingHullTypes.Coarse,
                PathFindingModes.Hull => PickingHullTypes.Hull,
                PathFindingModes.Geometry => PickingHullTypes.Geometry,
                _ => PickingHullTypes.None,
            };

            var desc = new ModelInstancedDescription()
            {
                CastShadow = CastShadow,
                UseAnisotropicFiltering = UseAnisotropic,
                Instances = count,
                LoadAnimation = false,
                BlendMode = BlendMode,
                PathFindingHull = pf,
                PickingHull = pf,
                Content = ContentDescription.FromContentData(modelContent),
            };

            return new(assetId, desc);
        }
        /// <summary>
        /// Creates a model description list for the objects of the specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="contentLibrary">Content library</param>
        public IEnumerable<(string Name, ModelInstancedDescription ModelDescription)> GetLevelObjects(Level level, ContentLibrary contentLibrary)
        {
            // Set auto-identifiers
            level.PopulateObjectIds();

            // Get instance count for all single geometries from Map
            var instances = level.GetObjectsInstanceCounters();

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

                yield return InitializeObject(assetName, count, level, modelContent, instances[assetName].PathFinding);
            }
        }
        /// <summary>
        /// Creates a new instanced model description for the object
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="count">Instance count</param>
        /// <param name="level">Level</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="pathFinding">Path finding enabled flag</param>
        private (string Name, ModelInstancedDescription ModelDescription) InitializeObject(string assetName, int count, Level level, ContentData modelContent, PathFindingModes pathFinding)
        {
            var modelId = $"{assetName}.{level.Name}";

            var pf = pathFinding switch
            {
                PathFindingModes.None => PickingHullTypes.None,
                PathFindingModes.Coarse => PickingHullTypes.Coarse,
                PathFindingModes.Hull => PickingHullTypes.Hull,
                PathFindingModes.Geometry => PickingHullTypes.Geometry,
                _ => PickingHullTypes.None,
            };

            var desc = new ModelInstancedDescription()
            {
                CastShadow = CastShadow,
                UseAnisotropicFiltering = UseAnisotropic,
                Instances = count,
                BlendMode = BlendMode,
                PathFindingHull = pf,
                PickingHull = pf,
                Content = ContentDescription.FromContentData(modelContent),
            };

            return new(modelId, desc);
        }
    }
}
