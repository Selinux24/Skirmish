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
        /// Gets the current path index in the animation path collection
        /// </summary>
        public int CurrentPathIndex { get; private set; } = 0;
        /// <summary>
        /// Current animation path
        /// </summary>
        public AnimationPath CurrentPath
        {
            get
            {
                return animationPaths.ElementAtOrDefault(CurrentPathIndex);
            }
        }
        /// <summary>
        /// Next animation path
        /// </summary>
        public AnimationPath NextPath
        {
            get
            {
                return animationPaths.ElementAtOrDefault(CurrentPathIndex + 1);
            }
        }
        /// <summary>
        /// Current path time
        /// </summary>
        public float CurrentPathTime
        {
            get
            {
                return CurrentPath?.PathElapsedTime ?? 0;
            }
        }
        /// <summary>
        /// Current path item duration
        /// </summary>
        public float CurrentPathItemDuration
        {
            get
            {
                return CurrentPath?.PartialPathItemInterpolationValue ?? 0;
            }
        }
        /// <summary>
        /// Gets the current path item clip name
        /// </summary>
        public string CurrentPathItemClipName
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
        public event AnimationControllerEventHandler PathEnding;
        /// <summary>
        /// On path changed event
        /// </summary>
        public event AnimationControllerEventHandler PathChanged;
        /// <summary>
        /// On path updated event
        /// </summary>
        public event AnimationControllerEventHandler PathUpdated;
        /// <summary>
        /// On animation offset changed
        /// </summary>
        public event AnimationControllerEventHandler AnimationOffsetChanged;

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
        /// <param name="clip">Clip name</param>
        /// <param name="planTime">Total time</param>
        /// <returns>Returns the created animation plan</returns>
        public AnimationPlan CalcAnimationPath(ISkinningData skData, string clip, float planTime)
        {
            if (skData == null)
            {
                return new AnimationPlan();
            }

            AnimationPath path = new AnimationPath()
            {
                Name = clip,
            };

            //Retrieve the clip data
            float clipTime = skData.GetClipDuration(skData.GetClipIndex(clip));

            if (clipTime >= planTime)
            {
                float delta = planTime / clipTime;

                path.Add(clip, delta);
            }
            else
            {
                float loopTime = planTime;
                int fullLoops = (int)Math.Ceiling(loopTime);
                float loopDelta = 1f;
                if (fullLoops - loopTime > 0f)
                {
                    fullLoops--;
                    loopDelta = loopTime / fullLoops;
                }

                path.AddRepeat(clip, Math.Max(1, fullLoops - 1), loopDelta);

                path.UpdateItems(skData);
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

                CurrentPathIndex = 0;

                return;
            }

            AnimationPath last;

            if (flags == AppendFlagTypes.ClearCurrent)
            {
                last = animationPaths.ElementAt(CurrentPathIndex);

                //Clear all paths
                animationPaths.Clear();
            }
            else if (flags == AppendFlagTypes.EndsCurrent && CurrentPathIndex < animationPaths.Count)
            {
                last = animationPaths.ElementAt(CurrentPathIndex);

                //Remove all paths from current to end
                if (CurrentPathIndex + 1 < animationPaths.Count)
                {
                    animationPaths.RemoveRange(
                        CurrentPathIndex + 1,
                        animationPaths.Count - (CurrentPathIndex + 1));
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

            if (CurrentPathIndex < 0)
            {
                CurrentPathIndex = 0;
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        /// <param name="skData">Skinning data</param>
        public void Update(ISkinningData skData, float elapsedSeconds)
        {
            if (skData == null)
            {
                return;
            }

            float tunedElapsedTime = elapsedSeconds * TimeDelta;
            if (tunedElapsedTime == 0f)
            {
                return;
            }

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
            if (playing)
            {
                MoveToNextPath();
            }

            //Updates current path
            bool updated = CurrentPath.Integrate(skData, tunedElapsedTime, out bool atEnd);
            if (updated)
            {
                //Updated internal animation path item index
                UpdatePath(atEnd);
            }

            if (GetAnimationOffset(skData, out uint newOffset))
            {
                //The animation offset in the animation palette was updated
                UpdateOffset(newOffset);
            }

            if (atEnd)
            {
                if (playing != CurrentPath.Playing && CurrentPathIndex >= animationPaths.Count - 1)
                {
                    //No paths to do
                    EndPath();
                }
                else
                {
                    //Moves to the next path
                    MoveToNextPath();
                }
            }
        }
        /// <summary>
        /// Updates the animation offset and fires the animation offset changed event
        /// </summary>
        /// <param name="newOffset">Animation offset</param>
        private void UpdateOffset(uint newOffset)
        {
            if (AnimationOffset == newOffset)
            {
                return;
            }

            uint prevOffset = AnimationOffset;
            AnimationOffset = newOffset;

            Logger.WriteTrace(this, $"Animation offset changed: {newOffset}");
            AnimationOffsetChanged?.Invoke(this, new AnimationControllerEventArgs()
            {
                CurrentOffset = AnimationOffset,
                CurrentIndex = CurrentPathIndex,
                CurrentPath = CurrentPath,
                PreviousOffset = prevOffset,
            });
        }
        /// <summary>
        /// Moves to the next animation path and fires the path changed event
        /// </summary>
        private void MoveToNextPath()
        {
            if (CurrentPathIndex >= animationPaths.Count - 1)
            {
                return;
            }

            int prevIndex = CurrentPathIndex;
            var prevPath = CurrentPath;

            CurrentPathIndex++;

            Logger.WriteTrace(this, $"Move to next animation path: {CurrentPath}");
            PathChanged?.Invoke(this, new AnimationControllerEventArgs()
            {
                CurrentOffset = AnimationOffset,
                CurrentIndex = CurrentPathIndex,
                CurrentPath = CurrentPath,
                PreviousIndex = prevIndex,
                PreviousPath = prevPath,
            });
        }
        /// <summary>
        /// Fires the end animation path event
        /// </summary>
        private void EndPath()
        {
            Logger.WriteTrace(this, "All animation paths done");
            PathEnding?.Invoke(this, new AnimationControllerEventArgs()
            {
                CurrentOffset = AnimationOffset,
                CurrentIndex = CurrentPathIndex,
                CurrentPath = CurrentPath,
                AtEnd = true,
            });
        }
        /// <summary>
        /// Fires the updated path event
        /// </summary>
        /// <param name="atEnd">Animation is at end</param>
        private void UpdatePath(bool atEnd)
        {
            Logger.WriteTrace(this, $"Current animation path changed: {CurrentPath}");
            PathUpdated?.Invoke(this, new AnimationControllerEventArgs()
            {
                CurrentOffset = AnimationOffset,
                CurrentIndex = CurrentPathIndex,
                CurrentPath = CurrentPath,
                AtEnd = atEnd,
            });
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

            if (skData == null)
            {
                return false;
            }

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
            if (skData == null)
            {
                return Enumerable.Empty<Matrix>();
            }

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
                    time = CurrentPath.PartialPathItemInterpolationValue;
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

            CurrentPathIndex = 0;

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
                CurrentIndex = CurrentPathIndex,
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
            CurrentPathIndex = animationControllerState.CurrentIndex;
            for (int i = 0; i < animationControllerState.AnimationPlan.Count(); i++)
            {
                var pathState = animationControllerState.AnimationPlan.ElementAt(i);
                animationPaths[i].SetState(pathState);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (CurrentPath == null)
            {
                return "Inactive";
            }

            StringBuilder res = new StringBuilder();

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
