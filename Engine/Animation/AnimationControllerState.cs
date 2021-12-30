using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Animation controller state
    /// </summary>
    public class AnimationControllerState : IGameState
    {
        /// <summary>
        /// Animation active flag
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Time delta to aply to controller time
        /// </summary>
        public float TimeDelta { get; set; }
        /// <summary>
        /// Gets whether the controller is currently playing an animation
        /// </summary>
        public bool Playing { get; set; }
        /// <summary>
        /// Animation plan
        /// </summary>
        public IEnumerable<IGameState> AnimationPlan { get; set; } = Enumerable.Empty<IGameState>();
    }
}
