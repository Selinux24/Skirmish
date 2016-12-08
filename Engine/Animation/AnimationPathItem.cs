
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

        /// <summary>
        /// Creates a copy of the current path item
        /// </summary>
        /// <returns>Returns the path item copy instance</returns>
        public AnimationPathItem Clone()
        {
            return new AnimationPathItem()
            {
                ClipName = this.ClipName,
                Loop = this.Loop,
                Repeats = this.Repeats,
                TimeDelta = this.TimeDelta,
            };
        }
    }
}
