using System.Collections.Generic;

namespace Engine.Content
{
    /// <summary>
    /// Animation content data
    /// </summary>
    public class AnimationLibContentData
    {
        /// <summary>
        /// Animation list
        /// </summary>
        private readonly List<IDictionary<string, IEnumerable<AnimationContent>>> animationList = new List<IDictionary<string, IEnumerable<AnimationContent>>>();

        /// <summary>
        /// Animation list
        /// </summary>
        public IEnumerable<IDictionary<string, IEnumerable<AnimationContent>>> Animations
        {
            get
            {
                return animationList.ToArray();
            }
        }

        /// <summary>
        /// Adds a new animation to the animation list
        /// </summary>
        /// <param name="animations">Animation dictionary</param>
        public void AddAnimation(IDictionary<string, IEnumerable<AnimationContent>> animations)
        {
            animationList.Add(animations);
        }
    }
}
