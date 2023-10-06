using System.Collections.Generic;
using System.Linq;

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
        public IEnumerable<ItemAction> Actions { get; set; } = Enumerable.Empty<ItemAction>();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {StateFrom} => {StateTo}; Animation: {AnimationPlan}; {Actions?.Count() ?? 0} actions.";
        }
    }
}
