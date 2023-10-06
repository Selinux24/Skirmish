using Engine.Content;

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
                return AssetMap.FromFile(Content.ContentFolder ?? string.Empty, AssetsConfigurationFile);
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
                return LevelMap.FromFile(Content.ContentFolder ?? string.Empty, LevelsFile);
            }

            return default;
        }
    }
}
