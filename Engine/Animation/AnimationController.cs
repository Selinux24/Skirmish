using SharpDX;
using System;

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
        private readonly AnimationPlan animationPaths = new AnimationPlan();
        /// <summary>
        /// Animation active flag
        /// </summary>
        private bool active = false;

        /// <summary>
        /// Time delta to aply to controller time
        /// </summary>
        public float TimeDelta { get; set; } = 1f;
        /// <summary>
        /// Gets wheter the controller is currently playing an animation
        /// </summary>
        public bool Playing
        {
            get
            {
                return this.CurrentIndex >= 0;
            }
        }
        /// <summary>
        /// Gets the current clip in the clip collection
        /// </summary>
        public int CurrentIndex { get; private set; } = -1;
        /// <summary>
        /// Current path time
        /// </summary>
        public float CurrentPathTime
        {
            get
            {
                if (this.CurrentIndex >= 0)
                {
                    var path = this.animationPaths[this.CurrentIndex];

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
                if (this.CurrentIndex >= 0)
                {
                    var path = this.animationPaths[this.CurrentIndex];

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
                if (this.CurrentIndex >= 0)
                {
                    var path = this.animationPaths[this.CurrentIndex];

                    var pathItem = path.GetCurrentItem();
                    if (pathItem != null)
                    {
                        return pathItem.ClipName;
                    }
                }

                return "None";
            }
        }
        /// <summary>
        /// Gets the path count in the controller
        /// </summary>
        public int PathCount
        {
            get
            {
                return this.animationPaths.Count;
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
        public void AddPath(AnimationPlan paths)
        {
            this.AppendPaths(AppendFlagTypes.None, paths);
        }
        /// <summary>
        /// Sets the specified past as current path list
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void SetPath(AnimationPlan paths)
        {
            this.AppendPaths(AppendFlagTypes.ClearCurrent, paths);
        }
        /// <summary>
        /// Adds clips to the controller clips list and ends the current animation
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void ContinuePath(AnimationPlan paths)
        {
            this.AppendPaths(AppendFlagTypes.EndsCurrent, paths);
        }
        /// <summary>
        /// Append animation paths to the controller
        /// </summary>
        /// <param name="flags">Append path flags</param>
        /// <param name="paths">Paths to append</param>
        private void AppendPaths(AppendFlagTypes flags, AnimationPlan paths)
        {
            var clonedPaths = new AnimationPath[paths.Count];

            for (int i = 0; i < paths.Count; i++)
            {
                clonedPaths[i] = paths[i].Clone();
            }

            if (this.animationPaths.Count > 0)
            {
                AnimationPath last = null;
                AnimationPath next = null;

                if (flags == AppendFlagTypes.ClearCurrent)
                {
                    last = this.animationPaths[this.animationPaths.Count - 1];
                    next = clonedPaths[0];

                    //Clear all paths
                    this.animationPaths.Clear();
                }
                else if (flags == AppendFlagTypes.EndsCurrent)
                {
                    last = this.animationPaths[this.CurrentIndex];
                    next = clonedPaths[0];

                    //Remove all paths from current to end
                    if (this.animationPaths.Count > this.CurrentIndex + 1)
                    {
                        this.animationPaths.RemoveRange(
                            this.CurrentIndex + 1,
                            this.animationPaths.Count - (this.CurrentIndex + 1));
                    }

                    //Mark current path for ending
                    last.End();
                }
                else
                {
                    last = this.animationPaths[this.animationPaths.Count - 1];
                    next = clonedPaths[0];
                }

                //Adds transitions from current path item to the first added item
                last.ConnectTo(next);
            }

            this.animationPaths.AddRange(clonedPaths);

            if (this.CurrentIndex < 0)
            {
                this.CurrentIndex = 0;
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="skData">Skinning data</param>
        public void Update(float time, SkinningData skData)
        {
            if (this.active && this.CurrentIndex >= 0)
            {
                var path = this.animationPaths[this.CurrentIndex];

                if (!path.Playing)
                {
                    this.CurrentIndex++;
                    if (this.CurrentIndex >= this.animationPaths.Count)
                    {
                        //No paths to do
                        this.CurrentIndex = -1;

                        this.PathEnding?.Invoke(this, new EventArgs());
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
            if (this.CurrentIndex >= 0)
            {
                //Get the path
                var path = this.animationPaths[this.CurrentIndex];

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
            if (this.CurrentIndex >= 0)
            {
                //Get the path
                var path = this.animationPaths[this.CurrentIndex];

                //Get the path item
                var pathItem = path.GetCurrentItem();
                if (pathItem != null)
                {
                    return skData.GetPoseAtTime(
                        path.ItemTime,
                        pathItem.ClipName);
                }
            }

            return new Matrix[] { };
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="time">At time</param>
        public void Start(float time = 0)
        {
            this.active = true;

            if (this.CurrentIndex >= 0)
            {
                var path = this.animationPaths[this.CurrentIndex];

                path.SetTime(time);
            }
        }
        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="time">At time</param>
        public void Stop(float time = 0)
        {
            this.active = false;

            if (this.CurrentIndex >= 0)
            {
                var path = this.animationPaths[this.CurrentIndex];

                path.SetTime(time);
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
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            string res = "Inactive";

            if (this.CurrentIndex >= 0)
            {
                res = string.Format("{0}", this.animationPaths[this.CurrentIndex].GetItemList().Join(", "));
                if (this.CurrentIndex + 1 < this.animationPaths.Count)
                {
                    res += string.Format(" {0}", this.animationPaths[this.CurrentIndex + 1].GetItemList().Join(", "));
                }
            }

            return res;
        }

        /// <summary>
        /// Animation paths append flags
        /// </summary>
        enum AppendFlagTypes
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
