using Engine.Tween;

namespace Engine.PostProcessing.Tween
{
    /// <summary>
    /// Post processing parameters tween extensions
    /// </summary>
    public static class DrawerPostProcessParamsTweenExtensions
    {
        /// <summary>
        /// Tween collection
        /// </summary>
        private static readonly DrawerPostProcessParamsTweenCollection collection = new DrawerPostProcessParamsTweenCollection();

        /// <summary>
        /// Static constructor
        /// </summary>
        static DrawerPostProcessParamsTweenExtensions()
        {
            // Register the collection into the tween manager
            FloatTweenManager.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public static void ClearTween(this IDrawerPostProcessParams control)
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
        public static void TweenIntensity(this IDrawerPostProcessParams control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTweenIntensity(control, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenIntensityBounce(this IDrawerPostProcessParams control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddIntensityBounce(control, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTweenIntensity(this IDrawerPostProcessParams control, FloatTween ft)
        {
            control.EffectIntensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.EffectIntensity = ft.CurrentValue;

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
        public static void AddIntensityBounce(this IDrawerPostProcessParams control, FloatTween ft)
        {
            control.EffectIntensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.EffectIntensity = ft.CurrentValue;

                if (ft.CurrentValue == ft.EndValue)
                {
                    var newStart = ft.EndValue;
                    var newEnd = ft.StartValue;

                    ft.Restart(newStart, newEnd);
                }

                return false;
            });
        }

        /// <summary>
        /// Tweens a property
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="name">Property name</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void Tween(this IDrawerPostProcessParams control, string name, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTween(control, name, ft);
        }
        /// <summary>
        /// Bouncing a property
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="name">Property name</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenBounce(this IDrawerPostProcessParams control, string name, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddBounce(control, name, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="name">Property name</param>
        /// <param name="ft">Tween</param>
        public static void AddTween(this IDrawerPostProcessParams control, string name, FloatTween ft)
        {
            control.SetProperty(name, ft.StartValue);

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.SetProperty(name, ft.CurrentValue);

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
        /// <param name="name">Property name</param>
        /// <param name="ft">Tween</param>
        public static void AddBounce(this IDrawerPostProcessParams control, string name, FloatTween ft)
        {
            control.SetProperty(name, ft.StartValue);

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.SetProperty(name, ft.CurrentValue);

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
