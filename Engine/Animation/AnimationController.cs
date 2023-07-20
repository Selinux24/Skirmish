using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Animation controller
    /// </summary>
    public class AnimationController : IHasGameState
    {
        /// <summary>
        /// Skinning data object
        /// </summary>
        private readonly IUseSkinningData skinningDataObject;
        /// <summary>
        /// Animation plan
        /// </summary>
        private readonly AnimationPlan animationPlan = new();
        /// <summary>
        /// Transition animation plan
        /// </summary>
        private readonly AnimationPlan transitionPlan = new();
        /// <summary>
        /// Animation active flag
        /// </summary>
        private bool active = false;

        /// <summary>
        /// Gets the current skinning data
        /// </summary>
        protected ISkinningData SkinningData
        {
            get
            {
                return skinningDataObject?.SkinningData;
            }
        }

        /// <summary>
        /// Time delta to aply to controller time
        /// </summary>
        public float TimeDelta { get; set; } = 1f;
        /// <summary>
        /// Gets whether the controller is currently playing an animation
        /// </summary>
        public bool Playing
        {
            get
            {
                return animationPlan.Active || (transitionPlan?.Active ?? false);
            }
        }

        /// <summary>
        /// Animation offset in the animation palette
        /// </summary>
        public uint AnimationOffset
        {
            get
            {
                return animationPlan.AnimationOffset;
            }
        }
        /// <summary>
        /// Transition offset in the animation palette
        /// </summary>
        public uint TransitionOffset
        {
            get
            {
                return transitionPlan.AnimationOffset;
            }
        }
        /// <summary>
        /// Interpolation value between current animation and transition
        /// </summary>
        public float TransitionInterpolationAmount { get; protected set; }

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
        /// On animation plan ending event
        /// </summary>
        public event AnimationControllerEventHandler PlanEnding;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Skinning data object</param>
        public AnimationController(IUseSkinningData obj)
        {
            skinningDataObject = obj;
        }

        /// <summary>
        /// Calculates an animation plan with initial and end clips, and with a central looping clip
        /// </summary>
        /// <param name="clip">Clip name</param>
        /// <param name="planTime">Total time</param>
        /// <returns>Returns the created animation plan</returns>
        public AnimationPlan CalcAnimationPlan(string clip, float planTime)
        {
            if (SkinningData == null)
            {
                return new AnimationPlan();
            }

            var path = new AnimationPath()
            {
                Name = clip,
            };

            //Retrieve the clip data
            float clipTime = SkinningData.GetClipDuration(SkinningData.GetClipIndex(clip));

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

                path.AddRepeat(clip, Math.Max(1, fullLoops), loopDelta);

                path.UpdateItems(SkinningData);
            }

            return new AnimationPlan(path);
        }

        /// <summary>
        /// Sets the specified plan as current plan
        /// </summary>
        /// <param name="plan">Animation plan</param>
        public void ReplacePlan(AnimationPlan plan)
        {
            AppendPlan(AppendFlagTypes.Replace, plan);
        }
        /// <summary>
        /// Appends the specified plan to de the plan list, at the end
        /// </summary>
        /// <param name="plan">Animation plan</param>
        public void AppendPlan(AnimationPlan plan)
        {
            AppendPlan(AppendFlagTypes.Append, plan);
        }
        /// <summary>
        /// Appens a new plan as transition of the current plan.
        /// </summary>
        /// <param name="plan">Animation plan</param>
        public void TransitionToPlan(AnimationPlan plan)
        {
            AppendPlan(AppendFlagTypes.Transition, plan);
        }
        /// <summary>
        /// Appends an animation plan to the controller
        /// </summary>
        /// <param name="flags">Append plan flags</param>
        /// <param name="plan">Plan to append</param>
        private void AppendPlan(AppendFlagTypes flags, AnimationPlan plan)
        {
            if (SkinningData == null)
            {
                return;
            }

            if (plan?.Any() != true)
            {
                return;
            }

            var clonedPlan = plan.Clone();

            if (!animationPlan.Any())
            {
                animationPlan.Add(clonedPlan);

                animationPlan.Reset();

                return;
            }

            if (flags == AppendFlagTypes.Replace)
            {
                //Clear current plan
                animationPlan.Clear();

                //Add new plan
                animationPlan.Add(clonedPlan);
            }
            else if (flags == AppendFlagTypes.Append)
            {
                //Add new plan at the end of the current plan
                animationPlan.Add(clonedPlan);
            }
            else if (flags == AppendFlagTypes.Transition)
            {
                //Remove all paths from current to end
                animationPlan.CutFromCurrent();

                //Sets the transition plan
                TransitionInterpolationAmount = 0f;
                transitionPlan.Clear();
                transitionPlan.Add(clonedPlan);
            }
        }

        /// <summary>
        /// Gets the current animation plan
        /// </summary>
        public AnimationPlan GetCurrentPlan()
        {
            return animationPlan;
        }
        /// <summary>
        /// Gets the current transition plan
        /// </summary>
        public AnimationPlan GetTransitionPlan()
        {
            return transitionPlan;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        public void Update(float elapsedSeconds)
        {
            if (SkinningData == null)
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
                animationPlan.Update(SkinningData, 0);

                TransitionInterpolationAmount = 0f;

                return;
            }

            var animRes = animationPlan.Update(SkinningData, tunedElapsedTime);

            if (!transitionPlan.Any())
            {
                FireEvents(animRes, animationPlan, false);

                TransitionInterpolationAmount = 0f;

                return;
            }

            var tranRes = transitionPlan.Update(SkinningData, tunedElapsedTime);

            //Update transition interpolation
            TransitionInterpolationAmount += tunedElapsedTime;

            if (TransitionInterpolationAmount >= 1f)
            {
                animationPlan.Clear();
                animationPlan.Add(transitionPlan, true);
                transitionPlan.Clear();

                TransitionInterpolationAmount = 0f;
            }

            FireEvents(tranRes, transitionPlan, true);
        }
        /// <summary>
        /// Fires integration results
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="plan">Animation plan</param>
        /// <param name="isTransition">Sets whether then plan is transition or not</param>
        private void FireEvents(AnimationPlanIntegrationResults result, AnimationPlan plan, bool isTransition)
        {
            if (result.HasFlag(AnimationPlanIntegrationResults.PathChanged))
            {
                Logger.WriteTrace(this, $"Moved to next animation path: {plan.CurrentPath}");
                PathChanged?.Invoke(this, new AnimationControllerEventArgs()
                {
                    CurrentOffset = plan.AnimationOffset,
                    CurrentIndex = plan.CurrentPathIndex,
                    CurrentPath = plan.CurrentPath,
                    IsTransition = isTransition,
                });
            }

            if (result.HasFlag(AnimationPlanIntegrationResults.UpdatedPath))
            {
                Logger.WriteTrace(this, $"Current animation path updated: {plan.CurrentPath}");
                PathUpdated?.Invoke(this, new AnimationControllerEventArgs()
                {
                    CurrentOffset = plan.AnimationOffset,
                    CurrentIndex = plan.CurrentPathIndex,
                    CurrentPath = plan.CurrentPath,
                    IsTransition = isTransition,
                });
            }

            if (result.HasFlag(AnimationPlanIntegrationResults.UpdatedOffset))
            {
                Logger.WriteTrace(this, $"Animation offset changed: {plan.AnimationOffset}");
                AnimationOffsetChanged?.Invoke(this, new AnimationControllerEventArgs()
                {
                    CurrentOffset = plan.AnimationOffset,
                    CurrentIndex = plan.CurrentPathIndex,
                    CurrentPath = plan.CurrentPath,
                    IsTransition = isTransition,
                });
            }

            if (result.HasFlag(AnimationPlanIntegrationResults.EndPlan))
            {
                Logger.WriteTrace(this, "All animation paths done");
                PlanEnding?.Invoke(this, new AnimationControllerEventArgs()
                {
                    CurrentOffset = plan.AnimationOffset,
                    CurrentIndex = plan.CurrentPathIndex,
                    CurrentPath = plan.CurrentPath,
                    AtEnd = true,
                    IsTransition = isTransition,
                });
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="time">At time</param>
        public void Start(float time = 0)
        {
            if (animationPlan == null)
            {
                return;
            }

            active = true;

            animationPlan.Reset();
            animationPlan.SetTime(time);
        }
        /// <summary>
        /// Start
        /// </summary>
        /// <param name="plan">Animation plan</param>
        /// <param name="time">At time</param>
        public void Start(AnimationPlan plan, float time = 0)
        {
            AppendPlan(plan);

            Start(time);
        }
        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="time">At time</param>
        public void Stop(float time = 0)
        {
            if (animationPlan == null)
            {
                return;
            }

            active = false;

            animationPlan.CurrentPath?.SetTime(time);
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

        /// <summary>
        /// Gets the transformation matrix list at current time
        /// </summary>
        /// <returns>Returns the transformation matrix list at current time</returns>
        public IEnumerable<Matrix> GetCurrentPose()
        {
            return animationPlan.GetCurrentPose(SkinningData);
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new AnimationControllerState
            {
                Active = active,
                TimeDelta = TimeDelta,
                AnimationPlan = animationPlan.GetState(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not AnimationControllerState animationControllerState)
            {
                return;
            }

            active = animationControllerState.Active;
            TimeDelta = animationControllerState.TimeDelta;
            animationPlan.SetState(animationControllerState.AnimationPlan);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (animationPlan.CurrentPath == null)
            {
                return "Inactive";
            }

            var res = new StringBuilder();

            res.AppendLine(animationPlan.CurrentPath.GetItemList() ?? string.Empty);
            res.AppendLine(animationPlan.NextPath?.GetItemList() ?? string.Empty);

            if (transitionPlan.Any())
            {
                res.AppendLine($"Transition to {transitionPlan.CurrentPath.GetItemList()}. {TransitionInterpolationAmount:0.0000}");
            }

            return res.ToString();
        }

        /// <summary>
        /// Animation paths append flags
        /// </summary>
        enum AppendFlagTypes
        {
            /// <summary>
            /// Replaces current
            /// </summary>
            Replace,
            /// <summary>
            /// Appends as last item, current plays until it's end
            /// </summary>
            Append,
            /// <summary>
            /// Adds the item as transition. Current plays until it's end, interpolating with the new item
            /// </summary>
            Transition,
        }
    }
}
