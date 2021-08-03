using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Animation path state
    /// </summary>
    public class AnimationPathState : IGameState
    {
        /// <summary>
        /// Current item index
        /// </summary>
        public int CurrentIndex { get; set; }
        /// <summary>
        /// Gets if the animation path is running
        /// </summary>
        public bool Playing { get; set; }
        /// <summary>
        /// Path time
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Total item time
        /// </summary>
        public float TotalItemTime { get; set; }
        /// <summary>
        /// Path items state
        /// </summary>
        public IEnumerable<IGameState> PathItems { get; set; } = Enumerable.Empty<IGameState>();
    }
}
