
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
        public string AssetsConfigurationFile = null;
        /// <summary>
        /// Assets configuration
        /// </summary>
        public ModularSceneryAssetConfiguration AssetsConfiguration = null;
        /// <summary>
        /// Levels file
        /// </summary>
        public string LevelsFile = null;
        /// <summary>
        /// Levels
        /// </summary>
        public ModularSceneryLevels Levels = null;
    }
}
