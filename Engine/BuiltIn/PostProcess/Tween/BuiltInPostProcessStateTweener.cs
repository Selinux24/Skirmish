using Engine.Tween;
using System;

namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Post processing parameters tween extensions
    /// </summary>
    public class BuiltInPostProcessStateTweener
    {
        /// <summary>
        /// Tweener
        /// </summary>
        private readonly Tweener tweener;
        /// <summary>
        /// Tween collection
        /// </summary>
        private readonly BuiltInPostProcessStateTweenCollection collection = new();

        /// <summary>
        /// constructor
        /// </summary>
        public BuiltInPostProcessStateTweener(Tweener tweener)
        {
            this.tweener = tweener;

            // Register the collection into the tween manager
            this.tweener.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public void ClearTween(BuiltInPostProcessState control)
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
        public void Tween(BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new();

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
        public void TweenBounce(BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new();

            ft.Start(from, to, duration, fnc);

            AddBounce(control, propertyUpdater, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public void AddTween(BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, FloatTween ft)
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
        public void AddBounce(BuiltInPostProcessState control, Func<BuiltInPostProcessState, float, float> propertyUpdater, FloatTween ft)
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

    /// <summary>
    /// Tweener extensions
    /// </summary>
    public static class BuiltInPostProcessStateTweenerExtensions
    {
        /// <summary>
        /// Creates a new tweener component
        /// </summary>
        /// <param name="scene">Scene</param>
        public static BuiltInPostProcessStateTweener AddBuiltInPostProcessStateTweener(this Scene scene)
        {
            var tweener = scene.Components.First<Tweener>() ?? throw new EngineException($"{nameof(Tweener)} scene component not present.");

            return new BuiltInPostProcessStateTweener(tweener);
        }
    }
}
