using System.Collections.Generic;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Scenery object state descriptor
    /// </summary>
    public class ObjectState
    {
        /// <summary>
        /// State name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Transitions list
        /// </summary>
        public IEnumerable<ObjectStateTransition> Transitions { get; set; } = [];
    }
}
