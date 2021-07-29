
namespace Engine.Modular
{
    /// <summary>
    /// Scenery object state descriptor
    /// </summary>
    public class ModularSceneryObjectState
    {
        /// <summary>
        /// State name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Transitions list
        /// </summary>
        public ModularSceneryObjectStateTransition[] Transitions { get; set; } = null;
    }
}
