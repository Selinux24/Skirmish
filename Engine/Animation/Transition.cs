
namespace Engine.Animation
{
    /// <summary>
    /// Transition manager between two animation clips
    /// </summary>
    public class Transition
    {
        /// <summary>
        /// From clip
        /// </summary>
        public readonly int ClipFrom;
        /// <summary>
        /// To clip
        /// </summary>
        public readonly int ClipTo;
        /// <summary>
        /// Starting time of from clip
        /// </summary>
        public readonly float StartFrom;
        /// <summary>
        /// Starting time of to clip
        /// </summary>
        public readonly float StartTo;
        /// <summary>
        /// Animation duration
        /// </summary>
        public readonly float Duration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From clip</param>
        /// <param name="to">To clip</param>
        /// <param name="duration">Transition duration</param>
        /// <param name="fromStart">Starting time of from clip</param>
        /// <param name="toStart">Starting time of to clip</param>
        public Transition(int from, int to, float fromStart, float toStart, float duration)
        {
            this.ClipFrom = from;
            this.ClipTo = to;
            this.Duration = duration;
            this.StartFrom = fromStart;
            this.StartTo = toStart;
        }
    }
}
