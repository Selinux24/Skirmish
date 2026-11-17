using Engine.Mathematics;
using System;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Animation clip
    /// </summary>
    public sealed class AnimationClip : IEquatable<AnimationClip>
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
            ArgumentNullException.ThrowIfNull(animations);

            Name = name;
            Animations = animations;

            if (animations.Length != 0)
            {
                Duration = animations.Max(a => a.Duration);
            }
        }

        /// <inheritdoc/>
        public bool Equals(AnimationClip other)
        {
            return
                Name == other.Name &&
                MathUtils.NearEqual(Duration, other.Duration) &&
                Helper.CompareEnumerables(Animations, other.Animations);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as AnimationClip);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Duration, Animations);
        }
    }
}
