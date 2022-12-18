using System;
using System.Collections.Generic;

namespace Engine.Modular
{
    /// <summary>
    /// Modular scenery trigger event arguments
    /// </summary>
    public class ModularSceneryTriggerEventArgs : EventArgs
    {
        /// <summary>
        /// Starting trigger
        /// </summary>
        public ModularSceneryTrigger StarterTrigger { get; set; }
        /// <summary>
        /// Starting item
        /// </summary>
        public ModularSceneryItem StarterItem { get; set; }
        /// <summary>
        /// Affected items by the trigger
        /// </summary>
        public IEnumerable<ModularSceneryItem> Items { get; set; }
    }
}
