
namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Modular scenery animation plan
    /// </summary>
    public class ModularSceneryObjectAnimationPlan
    {
        /// <summary>
        /// Plan name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Default animation
        /// </summary>
        public bool Default { get; set; } = false;
        /// <summary>
        /// Plan's animation paths
        /// </summary>
        public ModularSceneryObjectAnimationPath[] Paths { get; set; } = null;
    }
}
