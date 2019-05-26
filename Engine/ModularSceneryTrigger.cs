using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Modular scenery trigger
    /// </summary>
    public class ModularSceneryTrigger
    {
        /// <summary>
        /// Trigger name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of actions referenced by the trigger
        /// </summary>
        public List<ModularSceneryAction> Actions { get; set; } = new List<ModularSceneryAction>();

        /// <summary>
        /// Activates the trigger
        /// </summary>
        public void Activate()
        {
            if (Actions?.Count > 0)
            {
                foreach (var action in Actions)
                {
                    action.Start();
                }
            }
        }
    }
}
