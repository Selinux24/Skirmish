
namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Scenery assets file configuration
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Complex assets configuration
        /// </summary>
        public ModularSceneryAsset[] Assets { get; set; } = null;
        /// <summary>
        /// Maintain texture direction for ceilings and floors, avoiding asset map rotations
        /// </summary>
        public bool MaintainTextureDirection { get; set; } = true;
    }
}
