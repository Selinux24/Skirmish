
namespace Engine.Modular
{
    /// <summary>
    /// Modular scenery object action
    /// </summary>
    public class ModularSceneryObjectAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// State from name
        /// </summary>
        public string StateFrom { get; set; }
        /// <summary>
        /// State to name
        /// </summary>
        public string StateTo { get; set; }
        /// <summary>
        /// Animation plan name
        /// </summary>
        public string AnimationPlan { get; set; }
        /// <summary>
        /// Triggered item list
        /// </summary>
        public ModularSceneryObjectActionItem[] Items { get; set; } = null;
    }
}
