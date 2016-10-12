
namespace Engine.Animation
{
    /// <summary>
    /// Animation path item
    /// </summary>
    public class AnimationPathItem
    {
        /// <summary>
        /// Clip name
        /// </summary>
        public string ClipName = null;
        /// <summary>
        /// Time delta
        /// </summary>
        public float TimeDelta = 1f;
        /// <summary>
        /// Animation loops
        /// </summary>
        public bool Loop = false;
        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Repeats = 1;
    }
}
