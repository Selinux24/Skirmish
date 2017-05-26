using SharpDX;
using System;
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
        /// <summary>
        /// Gets the current path item clip name
        /// </summary>
        public string CurrentPathItemClip
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
        /// On path ending event
        /// </summary>
        public event EventHandler PathEnding;

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationController()
        {

        }

        /// <summary>
        /// Adds clips to the controller clips list
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void AddPath(params AnimationPath[] paths)
        {
            this.AppendPaths(AppendFlags.None, paths);
        }
        /// <summary>
        /// Sets the specified past as current path list
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void SetPath(params AnimationPath[] paths)
        {
            this.AppendPaths(AppendFlags.ClearCurrent, paths);
        }
        /// <summary>
        /// Adds clips to the controller clips list and ends the current animation
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void ContinuePath(params AnimationPath[] paths)
        {
            this.AppendPaths(AppendFlags.EndsCurrent, paths);
        }
        /// <summary>
        /// Append animation paths to the controller
        /// </summary>
        /// <param name="flags">Append path flags</param>
        /// <param name="paths">Paths to append</param>
        private void AppendPaths(AppendFlags flags, params AnimationPath[] paths)
        {
            var clonedPaths = new AnimationPath[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                clonedPaths[i] = paths[i].Clone();
            }

            if (this.animationPaths.Count > 0)
            {
                var currentLast = this.animationPaths[this.animationPaths.Count - 1];
                var newFirst = clonedPaths[0];

                //Adds transitions
                currentLast.ConnectTo(newFirst);

                if (flags == AppendFlags.EndsCurrent)
                {
                    currentLast.End();
                }
            }

            if (flags == AppendFlags.ClearCurrent)
            {
                this.animationPaths.Clear();
            }

            this.animationPaths.AddRange(clonedPaths);

            if (this.currentIndex < 0)
            {
                this.currentIndex = 0;
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="skData">Skinning data</param>
        public void Update(float time, SkinningData skData)
        {
            if (this.active && this.currentIndex >= 0)
            {
                var path = this.animationPaths[this.currentIndex];

                if (!path.Playing)
                {
                    this.currentIndex++;
                    if (this.currentIndex >= this.animationPaths.Count)
                    {
                        //No paths to do
                        this.currentIndex = -1;

                        if (this.PathEnding != null)
                        {
                            this.PathEnding(this, new EventArgs());
                        }
                    }
                }

                //Update current path
                path.Update(time * this.TimeDelta, skData);
            }
        }
        /// <summary>
        /// Gets the current animation offset from skinning animation data
        /// </summary>
        /// <param name="skData"></param>
        /// <returns>Returns the current animation offset in skinning animation data</returns>
        public uint GetAnimationOffset(SkinningData skData)
        {
            uint offset = 0;
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
        public Matrix[] GetCurrentPose(SkinningData skData)
        {
            if (this.currentIndex >= 0)
            {
                //Get the path
                var path = this.animationPaths[this.currentIndex];

                //Get the path item
                var pathItem = path.GetCurrentItem();
                if (pathItem != null)
                {
                    return skData.GetPoseAtTime(
                        path.ItemTime,
                        pathItem.ClipName);
                }
            }

            return null;
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

        /// <summary>
        /// Animation paths append flags
        /// </summary>
        enum AppendFlags
        {
            /// <summary>
            /// None
            /// </summary>
            None,
            /// <summary>
            /// Ends current clip
            /// </summary>
            EndsCurrent,
            /// <summary>
            /// Clear all clips
            /// </summary>
            ClearCurrent,
        }
    }
}
