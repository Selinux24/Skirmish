using System;

namespace Engine.Animation
{
    /// <summary>
    /// Animation path item
    /// </summary>
    public class AnimationPathItem : IHasGameState
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
                return Duration * Math.Max(1, Repeats);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Clip name</param>
        /// <param name="loop">Loop</param>
        /// <param name="repeats">Number of repeats</param>
        /// <param name="delta">Time delta</param>
        public AnimationPathItem(string name, bool loop, int repeats, float delta)
        {
            ClipName = name;
            Loop = loop;
            Repeats = repeats;
            TimeDelta = delta;
        }

        /// <summary>
        /// Updates internal state with specified skinning data
        /// </summary>
        /// <param name="skData">Skinning data</param>
        public void Update(ISkinningData skData)
        {
            if (skData == null)
            {
                return;
            }

            int clipIndex = skData.GetClipIndex(ClipName);
            Duration = skData.GetClipDuration(clipIndex);
        }
        /// <summary>
        /// Sets the item to finish current animation and end
        /// </summary>
        public void End()
        {
            Loop = false;
            Repeats = 1;
        }

        /// <summary>
        /// Creates a copy of the current path item
        /// </summary>
        /// <returns>Returns the path item copy instance</returns>
        public AnimationPathItem Clone()
        {
            return new AnimationPathItem(ClipName, Loop, Repeats, TimeDelta);
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new AnimationPathItemState
            {
                TimeDelta = TimeDelta,
                Loop = Loop,
                Repeats = Repeats,
                Duration = Duration,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (!(state is AnimationPathItemState animationPathItemState))
            {
                return;
            }

            TimeDelta = animationPathItemState.TimeDelta;
            Loop = animationPathItemState.Loop;
            Repeats = animationPathItemState.Repeats;
            Duration = animationPathItemState.Duration;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Repeats > 1)
            {
                return $"{ClipName}; Duration: {Duration:00.00}; Total Duration: {TotalDuration:00.00}; Loop {Loop}; Repeats: {Repeats}; Delta: {TimeDelta}";
            }
            else
            {
                return $"{ClipName}; Duration: {Duration:00.00}; Loop {Loop}; Delta: {TimeDelta}";
            }
        }
    }
}
