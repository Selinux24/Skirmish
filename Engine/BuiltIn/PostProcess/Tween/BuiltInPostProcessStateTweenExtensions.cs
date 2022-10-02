using Engine.Tween;
using System;

namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Post processing parameters tween extensions
    /// </summary>
    public static class BuiltInPostProcessStateTweenExtensions
    {
        /// <summary>
        /// Tween collection
        /// </summary>
        private static readonly BuiltInPostProcessStateTweenCollection collection = new BuiltInPostProcessStateTweenCollection();

        /// <summary>
        /// Static constructor
        /// </summary>
        static BuiltInPostProcessStateTweenExtensions()
        {
            // Register the collection into the tween manager
            FloatTweenManager.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public static void ClearTween(this BuiltInPostProcessState control)
        {
            collection.ClearTween(control);
        }

        /// <summary>
        /// Tweens the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void Tween(this BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTween(control, propertyUpdater, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenBounce(this BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddBounce(control, propertyUpdater, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTween(this BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, FloatTween ft)
        {
            propertyUpdater(control, ft.StartValue);

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                propertyUpdater(control, ft.CurrentValue);

                if (ft.CurrentValue == ft.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddBounce(this BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, FloatTween ft)
        {
            propertyUpdater(control, ft.StartValue);

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                propertyUpdater(control, ft.CurrentValue);

                if (ft.CurrentValue == ft.EndValue)
                {
                    var newStart = ft.EndValue;
                    var newEnd = ft.StartValue;

                    ft.Restart(newStart, newEnd);
                }

                return false;
            });
        }
    }
}
