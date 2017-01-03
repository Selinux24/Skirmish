using System;

namespace Engine.Animation
{
    /// <summary>
    /// Animation clip
    /// </summary>
    public class AnimationClip : IEquatable<AnimationClip>
    {
        /// <summary>
        /// Clip name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Clip duration
        /// </summary>
        public float Duration { get; private set; }
        /// <summary>
        /// Animation collection
        /// </summary>
        public JointAnimation[] Animations { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Clip name</param>
        /// <param name="animations">Animation list</param>
        public AnimationClip(string name, JointAnimation[] animations)
        {
            this.Name = name;
            this.Animations = animations;
            if (animations != null && animations.Length > 0)
            {
                float max = float.MinValue;
                for (int i = 0; i < animations.Length; i++)
                {
                    if (animations[i].Duration > max)
                    {
                        max = animations[i].Duration;
                    }
                }

                this.Duration = max;
            }
        }

        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(AnimationClip other)
        {
            return
                this.Name == other.Name &&
                this.Duration == other.Duration &&
                Helper.ListIsEqual(this.Animations, other.Animations);
        }
    }
}
