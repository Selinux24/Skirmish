
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
        /// Animation plan
        /// </summary>
        public IGameState[] AnimationPlan { get; set; } = [];
    }
}
