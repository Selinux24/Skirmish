using Engine.Tween;

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
        public static void TweenEffect1Intensity(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTweenEffect1Intensity(control, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect1IntensityBounce(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddEffect1IntensityBounce(control, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTweenEffect1Intensity(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect1Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect1Intensity = ft.CurrentValue;

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
        public static void AddEffect1IntensityBounce(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect1Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect1Intensity = ft.CurrentValue;

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
        /// Tweens the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect2Intensity(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTweenEffect2Intensity(control, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect2IntensityBounce(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddEffect2IntensityBounce(control, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTweenEffect2Intensity(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect2Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect2Intensity = ft.CurrentValue;

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
        public static void AddEffect2IntensityBounce(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect2Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect2Intensity = ft.CurrentValue;

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
        /// Tweens the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect3Intensity(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTweenEffect3Intensity(control, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect3IntensityBounce(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddEffect3IntensityBounce(control, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTweenEffect3Intensity(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect3Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect3Intensity = ft.CurrentValue;

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
        public static void AddEffect3IntensityBounce(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect3Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect3Intensity = ft.CurrentValue;

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
        /// Tweens the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect4Intensity(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTweenEffect4Intensity(control, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenEffect4IntensityBounce(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddEffect4IntensityBounce(control, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTweenEffect4Intensity(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect4Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect4Intensity = ft.CurrentValue;

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
        public static void AddEffect4IntensityBounce(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.Effect4Intensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.Effect4Intensity = ft.CurrentValue;

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
        /// Tweens the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenBloomIntensity(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddTweenBloomIntensity(control, ft);
        }
        /// <summary>
        /// Bouncing the effect intensity
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenBloomIntensityBounce(this BuiltInPostProcessState control, float from, float to, long duration, ScaleFunc fnc)
        {
            FloatTween ft = new FloatTween();

            ft.Start(from, to, duration, fnc);

            AddBloomIntensityBounce(control, ft);
        }
        /// <summary>
        /// Adds a tween task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ft">Tween</param>
        public static void AddTweenBloomIntensity(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.BloomIntensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.BloomIntensity = ft.CurrentValue;

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
        public static void AddBloomIntensityBounce(this BuiltInPostProcessState control, FloatTween ft)
        {
            control.BloomIntensity = ft.StartValue;

            collection.AddTween(control, (d) =>
            {
                ft.Update(d);

                control.BloomIntensity = ft.CurrentValue;

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
