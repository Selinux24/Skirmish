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
        private List<AnimationPath> animationPaths = new List<AnimationPath>();
        /// <summary>
        /// Current clip index in the animation controller clips list
        /// </summary>
        private int currentIndex = -1;
        /// <summary>
        /// Animation active flag
        /// </summary>
        private bool active = false;
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
        /// Gets the current clip in the clip collection
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                return this.currentIndex;
            }
        }
        /// <summary>
        /// Current path time
        /// </summary>
        public float CurrentPathTime
        {
            get
            {
                if (this.currentIndex >= 0)
                {
                    var path = this.animationPaths[this.currentIndex];

                    return path.Time;
                }

                return 0;
            }
        }
        /// <summary>
        /// Current path item time
        /// </summary>
        public float CurrentPathItemTime
        {
            get
            {
                if (this.currentIndex >= 0)
                {
                    var path = this.animationPaths[this.currentIndex];

                    return path.ItemTime;
                }

                return 0;
            }
        }

        public string CurrentPathClip
        {
            get
            {
                if (this.currentIndex >= 0)
                {
                    var path = this.animationPaths[this.currentIndex];

                    return path.GetCurrentItem().ClipName;
                }

                return "None";
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
        public void AddClip(AnimationPath path)
        {
            this.animationPaths.Add(path);

            if (this.currentIndex < 0)
            {
                this.currentIndex = 0;
            }
        }
        /// <summary>
        /// Adds clips to the controller clips list
        /// </summary>
        /// <param name="indices">Clip indices in the skinning data clip list</param>
        public void AddClip(params AnimationPath[] paths)
        {
            this.animationPaths.AddRange(paths);

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
            if (this.active && this.currentIndex >= 0)
            {
                //Get the path
                var path = this.animationPaths[this.currentIndex];

                //Modify controller current time
                path.Update(delta * this.TimeDelta, skData);
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
                //Get the path
                var path = this.animationPaths[this.currentIndex];

                //Get the path item
                var pathItem = path.GetCurrentItem();
                if (pathItem != null)
                {
                    skData.GetAnimationOffset(
                        path.ItemTime,
                        pathItem.ClipName,
                        out offset);
                }
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

            //Get the path
            if (this.currentIndex >= 0)
            {
                var path = this.animationPaths[this.currentIndex];

                return skData.GetPoseAtTime(path.Time, clipIndex);
            }
            else
            {
                return skData.GetPoseAtTime(0, clipIndex);
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="time">At time</param>
        public void Start(float time = 0)
        {
            this.active = true;

            if (this.currentIndex >= 0)
            {
                var path = this.animationPaths[this.currentIndex];

                path.Time = time;
                path.ItemTime = time;
            }
        }
        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="time">At time</param>
        public void Stop(float time = 0)
        {
            this.active = false;

            if (this.currentIndex >= 0)
            {
                var path = this.animationPaths[this.currentIndex];

                path.Time = time;
                path.ItemTime = time;
            }
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
