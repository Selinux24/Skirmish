using SharpDX;
using System.Collections.Generic;

namespace Engine.Animation
{
    /// <summary>
    /// Animation controller
    /// </summary>
    public class AnimationController
    {
        /// <summary>
        /// Controller clips
        /// </summary>
        private List<AnimationControllerClip> clips = new List<AnimationControllerClip>();
        /// <summary>
        /// Current clip index in the animation controller clips list
        /// </summary>
        private int currentIndex = -1;
        /// <summary>
        /// Time counter for previous clips in the controller
        /// </summary>
        private float previousTime;
        /// <summary>
        /// Controller time
        /// </summary>
        public float Time = 0;
        /// <summary>
        /// Gets wheter the controller is currently playing an animation
        /// </summary>
        public bool Playing
        {
            get
            {
                return this.currentIndex >= 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationController()
        {

        }

        /// <summary>
        /// Adds a clip to the controller clips list
        /// </summary>
        /// <param name="index">Clip index in the skinning data clip list</param>
        /// <param name="loop">Loops animation</param>
        /// <param name="duration">Total animation clip duration in the controller. It passes to the next clip when reached</param>
        public void AddClip(int index, bool loop, float duration)
        {
            this.clips.Add(new AnimationControllerClip()
            {
                Index = index,
                Loop = loop,
                Duration = duration,
            });
        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="delta">Time delta</param>
        public void Update(float delta)
        {
            this.Time += delta;

            this.previousTime = 0;
            for (int i = 0; i < this.clips.Count; i++)
            {
                float t = this.clips[i].Duration;

                if (Time < this.previousTime + t)
                {
                    if (this.currentIndex != i)
                    {
                        this.currentIndex = i;
                    }

                    break;
                }

                this.previousTime += t;
            }
        }
        /// <summary>
        /// Gets the current animation clip index from skinning animation data
        /// </summary>
        /// <returns>Returns the current animation clip index in skinning animation data</returns>
        public int GetAnimationIndex()
        {
            if (this.currentIndex >= 0)
            {
                return this.clips[this.currentIndex].Index;
            }
            else
            {
                return -1;
            }
        }
        /// <summary>
        /// Gets the current animation offset from skinning animation data
        /// </summary>
        /// <param name="skData"></param>
        /// <returns>Returns the current animation offset in skinning animation data</returns>
        public int GetAnimationOffset(SkinningData skData)
        {
            int offset = 0;
            if (this.currentIndex >= 0)
            {
                float animationTime = this.Time - this.previousTime;

                skData.GetAnimationOffset(
                    animationTime,
                    this.clips[this.currentIndex].Index,
                    out offset);
            }

            return offset;
        }

        public Matrix[] GetPose(SkinningData skData)
        {
            var clipIndex = this.GetAnimationIndex();

            return skData.GetPoseAtTime(this.Time, skData.GetClip(clipIndex));
        }
    }
}
