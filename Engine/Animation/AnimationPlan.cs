using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Animation plan for animation controller
    /// </summary>
    public class AnimationPlan
    {
        private readonly List<AnimationPath> animationPaths = new List<AnimationPath>();

        /// <summary>
        /// Last item time
        /// </summary>
        private float lastItemTime = 0;
        /// <summary>
        /// Last clip name
        /// </summary>
        private string lastClipName = null;

        /// <summary>
        /// Animation offset in the animation palette
        /// </summary>
        public uint AnimationOffset { get; private set; }
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
        /// Gets whether the animation plan was initialized or not
        /// </summary>
        public bool Initialized
        {
            get
            {
                return lastClipName != null;
            }
        }
        /// <summary>
        /// Plan is at end
        /// </summary>
        public bool AtEnd { get; private set; }
        /// <summary>
        /// Plan is active
        /// </summary>
        public bool Active
        {
            get
            {
                return Initialized && animationPaths.Any() && !AtEnd;
            }
        }

        /// <summary>
        /// Creates a new animation plan with an unique clip
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="timeDelta">Time delta</param>
        /// <returns>Returns a new animation plan</returns>
        public static AnimationPlan Create(string clipName, float timeDelta = 1f)
        {
            AnimationPath path = new AnimationPath();
            path.Add(clipName, timeDelta);

            return new AnimationPlan(path);
        }
        /// <summary>
        /// Creates a new animation plan with an unique clip and n repeats
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="repeats">Number of repeats</param>
        /// <param name="timeDelta">Time delta</param>
        /// <returns>Returns a new animation plan</returns>
        public static AnimationPlan CreateRepeat(string clipName, int repeats, float timeDelta = 1f)
        {
            AnimationPath path = new AnimationPath();
            path.AddRepeat(clipName, repeats, timeDelta);

            return new AnimationPlan(path);
        }
        /// <summary>
        /// Creates a new animation plan with an unique looping clip
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="timeDelta">Time delta</param>
        /// <returns>Returns a new animation plan</returns>
        public static AnimationPlan CreateLoop(string clipName, float timeDelta = 1f)
        {
            AnimationPath path = new AnimationPath();
            path.AddLoop(clipName, timeDelta);

            return new AnimationPlan(path);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationPlan() : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Animation path</param>
        public AnimationPlan(AnimationPath path)
        {
            animationPaths.Add(path);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public AnimationPlan(params AnimationPath[] paths)
        {
            animationPaths.AddRange(paths);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="paths">Animation path list</param>
        public AnimationPlan(IEnumerable<AnimationPath> paths)
        {
            animationPaths.AddRange(paths);
        }

        /// <summary>
        /// Gets whether the plan contains paths or not
        /// </summary>
        public bool Any()
        {
            return animationPaths.Any();
        }
        /// <summary>
        /// Clears the plan
        /// </summary>
        public void Clear()
        {
            animationPaths.Clear();
            CurrentPathIndex = 0;
            AnimationOffset = 0;
        }
        /// <summary>
        /// Adds a new plan to the current plan
        /// </summary>
        /// <param name="plan">Plan</param>
        /// <param name="updateState">Upate internal state with new Plan's internal state</param>
        public void Add(AnimationPlan plan, bool updateState = false)
        {
            animationPaths.AddRange(plan.animationPaths);

            if (updateState)
            {
                CurrentPathIndex = plan.CurrentPathIndex;
                AnimationOffset = plan.AnimationOffset;
            }
        }
        /// <summary>
        /// Adds a new path to the plan
        /// </summary>
        /// <param name="path">Animation path</param>
        public void AddItem(AnimationPath path)
        {
            animationPaths.Add(path);
        }
        /// <summary>
        /// Adds a new path list to the plan
        /// </summary>
        /// <param name="paths">Animation paths</param>
        public void AddItemRange(IEnumerable<AnimationPath> paths)
        {
            animationPaths.AddRange(paths);
        }
        /// <summary>
        /// Removes all paths from current to end
        /// </summary>
        public void CutFromCurrent()
        {
            if (CurrentPathIndex < animationPaths.Count - 1)
            {
                animationPaths.RemoveRange(CurrentPathIndex + 1, animationPaths.Count - (CurrentPathIndex + 1));
            }
        }

        /// <summary>
        /// Gets the current clip name and time
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="time">Time</param>
        /// <returns>Returns true if the controller is currently playing a clip</returns>
        public bool GetClipAndTime(out float time, out string clipName)
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

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the current animation offset from skinning animation data
        /// </summary>
        /// <param name="plan">Animation plan</param>
        /// <param name="offset">Returns the current animation offset in skinning animation data</param>
        /// <returns>Returns true if the offset was recovered</returns>
        public bool GetAnimationOffset(ISkinningData skinningData, out uint offset)
        {
            offset = 0;

            if (skinningData == null)
            {
                return false;
            }

            if (GetClipAndTime(out float time, out string clipName))
            {
                lastClipName = clipName;
                lastItemTime = time;

                skinningData.GetAnimationOffset(time, clipName, out uint newOffset);

                offset = newOffset;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets the transformation matrix list at current time
        /// </summary>
        /// <returns>Returns the transformation matrix list at current time</returns>
        public IEnumerable<Matrix> GetCurrentPose(ISkinningData skinningData)
        {
            if (skinningData == null)
            {
                return Enumerable.Empty<Matrix>();
            }

            if (GetClipAndTime(out float time, out string clipName))
            {
                return skinningData.GetPoseAtTime(time, clipName);
            }
            else
            {
                return skinningData.GetPoseAtTime(lastItemTime, lastClipName);
            }
        }

        /// <summary>
        /// Updates the plan state
        /// </summary>
        /// <param name="skinningData">Skinning data</param>
        /// <param name="elapsedTime">Elapsed time</param>
        public AnimationPlanIntegrationResults Update(ISkinningData skinningData, float elapsedTime)
        {
            AtEnd = false;

            if (CurrentPath == null)
            {
                AtEnd = true;

                return AnimationPlanIntegrationResults.None;
            }

            AnimationPlanIntegrationResults results = AnimationPlanIntegrationResults.None;

            bool playing = CurrentPath.Playing;

            if (playing && UpdatePathIndex()) results |= AnimationPlanIntegrationResults.PathChanged;

            //Updates current path
            bool updated = CurrentPath.Integrate(skinningData, elapsedTime, out bool atPathEnd);
            if (updated)
            {
                //Updated internal animation path item index
                results |= AnimationPlanIntegrationResults.UpdatedPath;
            }

            //Refresh the animation offset in the animation palette
            if (UpdateOffset(skinningData)) results |= AnimationPlanIntegrationResults.UpdatedOffset;

            if (!atPathEnd)
            {
                return results;
            }

            if (CurrentPathIndex >= animationPaths.Count - 1)
            {
                //No paths to do
                if (playing != CurrentPath.Playing)
                {
                    results |= AnimationPlanIntegrationResults.EndPlan;
                }

                AtEnd = true;
            }
            else
            {
                //Moves to the next path
                if (UpdatePathIndex()) results |= AnimationPlanIntegrationResults.PathChanged;
            }

            return results;
        }
        /// <summary>
        /// Updates the current path index
        /// </summary>
        private bool UpdatePathIndex()
        {
            if (CurrentPathIndex >= animationPaths.Count - 1)
            {
                return false;
            }

            CurrentPathIndex++;

            return true;
        }
        /// <summary>
        /// Updates the current animation offset
        /// </summary>
        /// <param name="skinningData">Skinning data</param>
        private bool UpdateOffset(ISkinningData skinningData)
        {
            if (GetAnimationOffset(skinningData, out uint newOffset))
            {
                if (AnimationOffset == newOffset)
                {
                    return false;
                }

                AnimationOffset = newOffset;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets the animation plan
        /// </summary>
        public void Reset()
        {
            AtEnd = !animationPaths.Any();
            CurrentPathIndex = 0;
            AnimationOffset = 0;
        }
        /// <summary>
        /// Sets the current path time
        /// </summary>
        /// <param name="time">Time value</param>
        public void SetTime(float time)
        {
            CurrentPath?.SetTime(time);
        }

        /// <summary>
        /// Clones the animation plan
        /// </summary>
        public AnimationPlan Clone()
        {
            return new AnimationPlan(animationPaths.Select(p => p.Clone()).ToArray());
        }

        /// <summary>
        /// Gets the plan state for persistence
        /// </summary>
        public IEnumerable<IGameState> GetState()
        {
            return animationPaths.Select(a => a.GetState()).ToArray();
        }
        /// <summary>
        /// Sets the plan stat from persistence
        /// </summary>
        public void SetState(IEnumerable<IGameState> state)
        {
            for (int i = 0; i < state.Count(); i++)
            {
                var pathState = state.ElementAt(i);

                animationPaths.ElementAtOrDefault(i)?.SetState(pathState);
            }
        }
    }

    /// <summary>
    /// Integration results
    /// </summary>
    [Flags]
    public enum AnimationPlanIntegrationResults
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// The path has change to another path
        /// </summary>
        PathChanged = 1,
        /// <summary>
        /// The internal state of the current path has changed
        /// </summary>
        UpdatedPath = 2,
        /// <summary>
        /// The animation offset has changed
        /// </summary>
        UpdatedOffset = 4,
        /// <summary>
        /// The has ended
        /// </summary>
        EndPlan = 8,
    }
}
