using System.Collections.Generic;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Modular scenery animation plan
    /// </summary>
    public class ObjectAnimationPlan
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
        public IEnumerable<ObjectAnimationPath> Paths { get; set; } = [];
    }
}
