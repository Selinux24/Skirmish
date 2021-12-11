using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Animation controller
    /// </summary>
    public class AnimationController : IHasGameState
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
        /// Last item time
        /// </summary>
        private float lastItemTime = 0;
        /// <summary>
        /// Last clip name
        /// </summary>
        private string lastClipName = null;

        /// <summary>
        /// Time delta to aply to controller time
        /// </summary>
        public float TimeDelta { get; set; } = 1f;
        /// <summary>
        /// Gets whether the controller is currently playing an animation
        /// </summary>
        public bool Playing { get; private set; } = false;
        /// <summary>
        /// Gets the current clip in the clip collection
        /// </summary>
        public int CurrentIndex { get; private set; } = 0;
        /// <summary>
        /// Current animation path
        /// </summary>
        public AnimationPath CurrentPath
        {
            get
            {
                return animationPaths.ElementAtOrDefault(CurrentIndex);
            }
        }
        /// <summary>
        /// Next animation path
        /// </summary>
        public AnimationPath NextPath
        {
            get
            {
                return animationPaths.ElementAtOrDefault(CurrentIndex + 1);
            }
        }
        /// <summary>
        /// Current path time
        /// </summary>
        public float CurrentPathTime
        {
            get
            {
                return CurrentPath?.Time ?? 0;
            }
        }
        /// <summary>
        /// Current path item time
        /// </summary>
        public float CurrentPathItemTime
        {
            get
            {
                return CurrentPath?.ItemTime ?? 0;
            }
        }
        /// <summary>
        /// Gets the current path item clip name
        /// </summary>
        public string CurrentPathItemClip
        {
            get
            {
                return CurrentPath?.CurrentItem?.ClipName ?? "None";
            }
        }
        /// <summary>
        /// Animation offset in the animation palette
        /// </summary>
        public uint AnimationOffset { get; protected set; }
        /// <summary>
        /// Gets the path count in the controller
        /// </summary>
        public int PathCount
        {
            get
            {
                return animationPaths.Count;
            }
        }

        /// <summary>
        /// On path ending event
        /// </summary>
        public event EventHandler PathEnding;
        /// <summary>
        /// On path changed event
        /// </summary>
        public event EventHandler PathChanged;
        /// <summary>
        /// On path updated event
        /// </summary>
        public event EventHandler PathUpdated;
        /// <summary>
        /// On animation offset changed
        /// </summary>
        public event EventHandler AnimationOffsetChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationController()
        {

        }

        /// <summary>
        /// Calculates an animation plan with initial and end clips, and with a central looping clip
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <param name="initClip">Initial clip name</param>
        /// <param name="loopClip">Loop clip name</param>
        /// <param name="endClip">End clip name</param>
        /// <param name="planTime">Total time</param>
        /// <returns>Returns the created animation plan</returns>
        public AnimationPlan CalcPath(ISkinningData skData, string initClip, string loopClip, string endClip, float planTime)
        {
            AnimationPath path = new AnimationPath();

            //Retrieve the clip data
            float initTime = skData.GetClipDuration(skData.GetClipIndex(initClip));
            float loopTime = skData.GetClipDuration(skData.GetClipIndex(loopClip));
            float endTime = skData.GetClipDuration(skData.GetClipIndex(endClip));

            if (initTime + endTime >= planTime)
            {
                float delta = planTime / (initTime + endTime);

                path.Add(initClip, delta);
                path.Add(endClip, delta);
            }
            else if (initTime + loopTime + endTime >= planTime)
            {
                float delta = planTime / (initTime + loopTime + endTime);

                path.Add(initClip, delta);
                path.Add(loopClip, delta);
                path.Add(endClip, delta);
            }
            else if (initTime + (loopTime * 2) + endTime >= planTime)
            {
                float delta = planTime / (initTime + (loopTime * 2) + endTime);

                path.Add(initClip, delta);
                path.Add(loopClip, delta);
                path.Add(loopClip, delta);
                path.Add(endClip, delta);
            }
            else
            {
                float centralTime = (planTime - (initTime + (loopTime * 2) + endTime)) / loopTime;
                int centralLoops = (int)Math.Ceiling(centralTime);
                float time = planTime - (loopTime * centralLoops);

                float delta = time / (initTime + (loopTime * 2) + endTime);

                path.Add(initClip, delta);
                path.Add(loopClip, delta);
                path.AddRepeat(loopClip, centralLoops);
                path.Add(loopClip, delta);
                path.Add(endClip, delta);
            }

            return new AnimationPlan(path);
        }

        /// <summary>
        /// Adds clips to the controller clips list
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void AddPath(AnimationPlan paths)
        {
            AppendPaths(AppendFlagTypes.None, paths);
        }
        /// <summary>
        /// Sets the specified past as current path list
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void SetPath(AnimationPlan paths)
        {
            AppendPaths(AppendFlagTypes.ClearCurrent, paths);
        }
        /// <summary>
        /// Adds clips to the controller clips list and ends the current animation
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public void ContinuePath(AnimationPlan paths)
        {
            AppendPaths(AppendFlagTypes.EndsCurrent, paths);
        }
        /// <summary>
        /// Append animation paths to the controller
        /// </summary>
        /// <param name="flags">Append path flags</param>
        /// <param name="paths">Paths to append</param>
        private void AppendPaths(AppendFlagTypes flags, AnimationPlan paths)
        {
            var clonedPaths = paths?.Select(p => p.Clone()).ToArray();

            if (!animationPaths.Any())
            {
                animationPaths.AddRange(clonedPaths);

                CurrentIndex = 0;

                return;
            }

            AnimationPath last;

            if (flags == AppendFlagTypes.ClearCurrent)
            {
                last = animationPaths.Last();

                //Clear all paths
                animationPaths.Clear();
            }
            else if (flags == AppendFlagTypes.EndsCurrent && CurrentIndex < animationPaths.Count)
            {
                last = animationPaths.ElementAt(CurrentIndex);

                //Remove all paths from current to end
                if (CurrentIndex + 1 < animationPaths.Count)
                {
                    animationPaths.RemoveRange(
                        CurrentIndex + 1,
                        animationPaths.Count - (CurrentIndex + 1));
                }

                //Mark current path for ending
                last.End();
            }
            else
            {
                last = animationPaths.Last();
            }

            //Adds transitions from current path item to the first added item
            last.ConnectTo(clonedPaths.First());

            animationPaths.AddRange(clonedPaths);

            if (CurrentIndex < 0)
            {
                CurrentIndex = 0;
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="skData">Skinning data</param>
        public void Update(float time, ISkinningData skData)
        {
            if (!active)
            {
                if (GetAnimationOffset(skData, out uint offset))
                {
                    AnimationOffset = offset;
                }

                return;
            }

            if (CurrentPath == null)
            {
                return;
            }

            bool playing = CurrentPath.Playing;

            if (playing && CurrentIndex < animationPaths.Count - 1)
            {
                //Go to next path
                CurrentIndex++;
                Logger.WriteTrace(this, $"Move to next animation path: {CurrentPath}");
                PathChanged?.Invoke(this, new EventArgs());
            }

            //Update current path
            CurrentPath.Update(time * TimeDelta, skData, out bool updated, out bool atEnd);
            if (updated)
            {
                Logger.WriteTrace(this, $"Current animation path index changed: {CurrentPath}");
                PathUpdated?.Invoke(this, new EventArgs());
            }

            if (GetAnimationOffset(skData, out uint newOffset) && AnimationOffset != newOffset)
            {
                Logger.WriteTrace(this, $"Animation offset changed: {newOffset}");
                AnimationOffset = newOffset;
                AnimationOffsetChanged?.Invoke(this, new EventArgs());
            }

            if (atEnd && playing != CurrentPath.Playing && CurrentIndex == animationPaths.Count - 1)
            {
                //No paths to do
                Logger.WriteTrace(this, "All animation paths done");
                PathEnding?.Invoke(this, new EventArgs());
            }
        }
        /// <summary>
        /// Gets the current animation offset from skinning animation data
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <param name="offset">Returns the current animation offset in skinning animation data</param>
        /// <returns>Returns true if the offset was recovered</returns>
        protected bool GetAnimationOffset(ISkinningData skData, out uint offset)
        {
            offset = 0;

            if (GetClipAndTime(out float time, out string clipName))
            {
                lastClipName = clipName;
                lastItemTime = time;

                skData.GetAnimationOffset(time, clipName, out uint newOffset);

                offset = newOffset;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets the transformation matrix list at current time
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <returns>Returns the transformation matrix list at current time</returns>
        public IEnumerable<Matrix> GetCurrentPose(ISkinningData skData)
        {
            if (GetClipAndTime(out float time, out string clipName))
            {
                return skData.GetPoseAtTime(time, clipName);
            }
            else
            {
                return skData.GetPoseAtTime(lastItemTime, lastClipName);
            }
        }
        /// <summary>
        /// Gets the current clip name and time
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="time">Time</param>
        /// <returns>Returns true if the controller is currently playing a clip</returns>
        protected bool GetClipAndTime(out float time, out string clipName)
        {
            time = 0;
            clipName = null;

            //Get the path
            if (CurrentPath?.Playing == true)
            {
                //Get the path item
                var pathItem = CurrentPath.CurrentItem;
                if (pathItem != null)
                {
                    time = CurrentPath.ItemTime;
                    clipName = pathItem.ClipName;

                    Playing = true;

                    return true;
                }
            }

            Playing = false;

            return false;
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="time">At time</param>
        public void Start(float time = 0)
        {
            active = true;

            CurrentIndex = 0;

            CurrentPath?.SetTime(time);
        }
        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="time">At time</param>
        public void Stop(float time = 0)
        {
            active = false;

            CurrentPath?.SetTime(time);
        }
        /// <summary>
        /// Resume playback
        /// </summary>
        public void Resume()
        {
            active = true;
        }
        /// <summary>
        /// Pause playback
        /// </summary>
        public void Pause()
        {
            active = false;
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new AnimationControllerState
            {
                Active = active,
                LastItemTime = lastItemTime,
                LastClipName = lastClipName,
                TimeDelta = TimeDelta,
                Playing = Playing,
                CurrentIndex = CurrentIndex,
                AnimationPlan = animationPaths.Select(a => a.GetState()).ToArray(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (!(state is AnimationControllerState animationControllerState))
            {
                return;
            }

            active = animationControllerState.Active;
            lastItemTime = animationControllerState.LastItemTime;
            lastClipName = animationControllerState.LastClipName;
            TimeDelta = animationControllerState.TimeDelta;
            Playing = animationControllerState.Playing;
            CurrentIndex = animationControllerState.CurrentIndex;
            for (int i = 0; i < animationControllerState.AnimationPlan.Count(); i++)
            {
                var pathState = animationControllerState.AnimationPlan.ElementAt(i);
                animationPaths[i].SetState(pathState);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder("Inactive");

            res.Append(CurrentPath?.GetItemList() ?? string.Empty);
            res.Append(NextPath?.GetItemList() ?? string.Empty);

            return res.ToString();
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
