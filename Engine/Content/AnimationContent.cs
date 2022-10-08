using System.Linq;

namespace Engine.Content
{
    using Engine.Animation;

    /// <summary>
    /// Animation content
    /// </summary>
    public class AnimationContent
    {
        /// <summary>
        /// Joint name
        /// </summary>
        public string JointName { get; set; }
        /// <summary>
        /// Transform type
        /// </summary>
        public string TransformType { get; set; }
        /// <summary>
        /// Keyframe list
        /// </summary>
        public Keyframe[] Keyframes { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Keyframes?.Any() == true)
            {
                return $"Start: {Keyframes.First()}; End: {Keyframes.Last()}; {JointName}";
            }
            else
            {
                return $"No animation; {JointName}";
            }
        }
    }
}
