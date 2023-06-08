
namespace Engine.Modular
{
    using Engine.Content;
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
        /// <remarks>
        /// The folder must contains the following files:
        /// - assets.xml for asset collection in the ContentDataFile format
        /// - assetsmap.xml for asset groups definition, like rooms or corridors, in the ModularSceneryAssetConfiguration format
        /// - levels.xml for level description in the ModularSceneryLevels format
        /// </remarks>
        public static ModularSceneryDescription FromFolder(string contentFolder)
        {
            return new ModularSceneryDescription()
            {
                UseAnisotropic = true,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(contentFolder, "assets.json"),
                AssetsConfigurationFile = "assetsmap.json",
                LevelsFile = "levels.json",
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
