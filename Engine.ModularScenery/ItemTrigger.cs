using System.Collections.Generic;

namespace Engine.Modular
{
    /// <summary>
    /// Modular scenery trigger
    /// </summary>
    public class ItemTrigger
    {
        /// <summary>
        /// Trigger name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// State from
        /// </summary>
        public string StateFrom { get; set; }
        /// <summary>
        /// State to
        /// </summary>
        public string StateTo { get; set; }
        /// <summary>
        /// Animation plan
        /// </summary>
        public string AnimationPlan { get; set; }
        /// <summary>
        /// List of actions referenced by the trigger
        /// </summary>
        public List<ItemAction> Actions { get; set; } = new List<ItemAction>();
    }
}
