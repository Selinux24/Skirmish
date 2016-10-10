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
        /// Animation active flag
        /// </summary>
        private bool active = false;
        /// <summary>
        /// Controller time
        /// </summary>
        public float Time = 0;
        /// <summary>
        /// Time delta to aply to controller time
        /// </summary>
        public float TimeDelta = 1f;
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
        public void AddClip(int index, bool loop, float duration = 0)
        {
            this.clips.Add(new AnimationControllerClip()
            {
                Index = index,
                Loop = loop,
                Duration = duration,
            });

            if (this.currentIndex < 0)
            {
                this.currentIndex = 0;
            }
        }
        /// <summary>
        /// Adds clips to the controller clips list
        /// </summary>
        /// <param name="indices">Clip indices in the skinning data clip list</param>
        public void AddClip(params int[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                this.clips.Add(new AnimationControllerClip()
                {
                    Index = indices[i],
                    Loop = false,
                    Duration = 0,
                });
            }

            if (this.currentIndex < 0)
            {
                this.currentIndex = 0;
            }
        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="delta">Time delta</param>
        /// <param name="skData">Skinning data</param>
        public void Update(float delta, SkinningData skData)
        {
            if (this.active)
            {
                this.Time += delta * this.TimeDelta;

                this.previousTime = 0;
                for (int i = 0; i < this.clips.Count; i++)
                {
                    float t = this.clips[i].Duration;
                    if (t == 0)
                    {
                        t = skData.GetClip(this.clips[i].Index).Duration;
                    }

                    if (this.Time < this.previousTime + t)
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
        }
        /// <summary>
        /// Gets the current animation clip index from skinning animation data
        /// </summary>
        /// <returns>Returns the current animation clip index in skinning animation data</returns>
        public int GetAnimationIndex()
        {
            if (this.currentIndex >= 0)
            {
                //TODO: Only one animation set for now, with all clips in one line. Return always 0
                //return this.clips[this.currentIndex].Index;
                return 0;
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
        /// <summary>
        /// Gets the transformation matrix list at current time
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <returns>Returns the transformation matrix list at current time</returns>
        public Matrix[] GetPose(SkinningData skData)
        {
            var clipIndex = this.GetAnimationIndex();

            return skData.GetPoseAtTime(this.Time, clipIndex);
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="time">At time</param>
        public void Start(float time = 0)
        {
            this.active = true;
            this.Time = time;
        }
        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="time">At time</param>
        public void Stop(float time = 0)
        {
            this.active = false;
            this.Time = time;
        }
        /// <summary>
        /// Resume playback
        /// </summary>
        public void Resume()
        {
            this.active = true;
        }
        /// <summary>
        /// Pause playback
        /// </summary>
        public void Pause()
        {
            this.active = false;
        }
    }
}
