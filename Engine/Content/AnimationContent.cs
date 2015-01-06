
namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Animation content
    /// </summary>
    public class AnimationContent
    {
        /// <summary>
        /// Target
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// Joint
        /// </summary>
        public string Joint
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Target))
                {
                    string[] bits = this.Target.Split("/".ToCharArray());

                    return bits != null && bits.Length > 0 ? bits[0] : null;
                }

                return null;
            }
        }
        /// <summary>
        /// Keyframe list
        /// </summary>
        public Keyframe[] Keyframes { get; set; }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            if (this.Keyframes != null && this.Keyframes.Length > 0)
            {
                return string.Format("Start: {0}; End: {1};", this.Keyframes[0], this.Keyframes[this.Keyframes.Length - 1]);
            }
            else
            {
                return string.Format("No animation;");
            }
        }
    }
}
