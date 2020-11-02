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
        /// Joint
        /// </summary>
        public string Joint { get; set; }
        /// <summary>
        /// Keyframe list
        /// </summary>
        public Keyframe[] Keyframes { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Keyframes?.Any() == true)
            {
                return $"Start: {Keyframes.First()}; End: {Keyframes.Last()};";
            }
            else
            {
                return "No animation;";
            }
        }
    }
}
