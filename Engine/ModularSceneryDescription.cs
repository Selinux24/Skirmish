using Engine.Content;

namespace Engine
{
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
                CastShadow = true,
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
        public ModularSceneryAssetConfiguration AssetsConfiguration { get; set; } = null;
        /// <summary>
        /// Levels file
        /// </summary>
        public string LevelsFile { get; set; } = null;
        /// <summary>
        /// Levels
        /// </summary>
        public ModularSceneryLevels Levels { get; set; } = null;
    }
}
