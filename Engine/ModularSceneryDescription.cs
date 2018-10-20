
namespace Engine
{
    /// <summary>
    /// Modular scenery description
    /// </summary>
    public class ModularSceneryDescription : GroundDescription
    {
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
