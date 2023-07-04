using System;
using System.Collections.Generic;

namespace Engine.Modular
{
    /// <summary>
    /// Modular scenery trigger event arguments
    /// </summary>
    public class TriggerEventArgs : EventArgs
    {
        /// <summary>
        /// Starting trigger
        /// </summary>
        public ItemTrigger StarterTrigger { get; set; }
        /// <summary>
        /// Starting item
        /// </summary>
        public Item StarterItem { get; set; }
        /// <summary>
        /// Affected items by the trigger
        /// </summary>
        public IEnumerable<Item> Items { get; set; }
    }
}
