
using System;

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
        public string ClipName { get; private set; }
        /// <summary>
        /// Time delta
        /// </summary>
        public float TimeDelta { get; private set; }
        /// <summary>
        /// Animation loops
        /// </summary>
        public bool Loop { get; private set; }
        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Repeats { get; private set; }
        /// <summary>
        /// Is transition
        /// </summary>
        public bool IsTranstition { get; private set; }
        /// <summary>
        /// Clip duration
        /// </summary>
        public float Duration { get; private set; }
        /// <summary>
        /// Path item total duration
        /// </summary>
        /// <remarks>Gets the total clip duration applying number of repeats and time delta</remarks>
        public float TotalDuration
        {
            get
            {
                return this.Duration * this.Repeats / this.TimeDelta;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Clip name</param>
        /// <param name="loop">Loop</param>
        /// <param name="repeats">Number of repeats</param>
        /// <param name="delta">Time delta</param>
        /// <param name="isTransition">Is transition</param>
        public AnimationPathItem(string name, bool loop, int repeats, float delta, bool isTransition)
        {
            this.ClipName = name;
            this.Loop = loop;
            this.Repeats = repeats;
            this.TimeDelta = delta;
            this.IsTranstition = isTransition;
        }

        /// <summary>
        /// Updates internal state with specified skinning data
        /// </summary>
        /// <param name="skData">Skinning data</param>
        public void UpdateSkinningData(SkinningData skData)
        {
            int clipIndex = skData.GetClipIndex(this.ClipName);
            this.Duration = skData.GetClipDuration(clipIndex);
        }
        /// <summary>
        /// Sets the item to finish current animation and end
        /// </summary>
        public void End()
        {
            this.Loop = false;
            this.Repeats = 1;
        }

        /// <summary>
        /// Creates a copy of the current path item
        /// </summary>
        /// <returns>Returns the path item copy instance</returns>
        public AnimationPathItem Clone()
        {
            return new AnimationPathItem(this.ClipName, this.Loop, this.Repeats, this.TimeDelta, this.IsTranstition);
        }
        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}; Loop {2}; Repeats: {3}; Delta: {4}",
                this.IsTranstition ? "Transition" : "Clip",
                this.ClipName,
                this.Loop,
                this.Repeats,
                this.TimeDelta);
        }
    }
}
