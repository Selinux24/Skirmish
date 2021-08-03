
namespace Engine.Animation
{
    /// <summary>
    /// Animation path item state
    /// </summary>
    public class AnimationPathItemState : IGameState
    {
        /// <summary>
        /// Time delta
        /// </summary>
        public float TimeDelta { get; set; }
        /// <summary>
        /// Animation loops
        /// </summary>
        public bool Loop { get; set; }
        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Repeats { get; set; }
        /// <summary>
        /// Is transition
        /// </summary>
        public bool IsTranstition { get; set; }
        /// <summary>
        /// Clip duration
        /// </summary>
        public float Duration { get; set; }
    }
}
