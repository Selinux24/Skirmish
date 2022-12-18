using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Instanced model state
    /// </summary>
    public class ModelInstancedState : BaseSceneObjectState
    {
        /// <summary>
        /// Maximum count
        /// </summary>
        public int MaximumCount { get; set; }
        /// <summary>
        /// Instance list
        /// </summary>
        public IEnumerable<IGameState> Instances { get; set; } = Enumerable.Empty<IGameState>();
    }
}
