using Engine.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Tween
{
    /// <summary>
    /// Float tween manager
    /// </summary>
    public static class FloatTweenManager
    {
        /// <summary>
        /// Task list
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, Func<float, bool>> tasks = new ConcurrentDictionary<Guid, Func<float, bool>>();

        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="elapsedTime">Elapsed time</param>
        public static void Update(float elapsedTime)
        {
            if (!tasks.Any())
            {
                return;
            }

            // Copy active tasks
            var activeTasks = tasks.ToArray();

            List<Guid> toDelete = new List<Guid>();

            foreach (var task in activeTasks)
            {
                bool finished = task.Value.Invoke(elapsedTime);

                if (finished)
                {
                    toDelete.Add(task.Key);
                }
            }

            if (toDelete.Any())
            {
                toDelete.ForEach(i => tasks.TryRemove(i, out _));
            }
        }

        /// <summary>
        /// Scale up a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleUp(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenScale(control, 0, 1, duration, fnc);
        }
        /// <summary>
        /// Scales down a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleDown(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenScale(control, 1, 0, duration, fnc);
        }
        /// <summary>
        /// Scales a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScale(this UIControl control, float from, float to, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftScale = new FloatTween();

            ftScale.Start(from, to, duration, fnc);

            AddScaleTween(control, ftScale);
        }
        /// <summary>
        /// Adds a scale task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftScale">Scale tween</param>
        private static void AddScaleTween(UIControl control, FloatTween ftScale)
        {
            control.Scale = ftScale.StartValue;
            control.Active = true;
            control.Visible = true;

            tasks.TryAdd(Guid.NewGuid(), (d) =>
            {
                ftScale.Update(d);

                control.Scale = ftScale.CurrentValue;

                if (ftScale.CurrentValue == ftScale.EndValue)
                {
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Rotate a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="targetAngle">Target angle</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenRotate(this UIControl control, float targetAngle, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new FloatTween();

            ftRotate.Start(control.Rotation, targetAngle, duration, fnc);

            AddRotateTween(control, ftRotate);
        }
        /// <summary>
        /// Adds a rotation task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftRotate">Rotation tween</param>
        private static void AddRotateTween(UIControl control, FloatTween ftRotate)
        {
            control.Rotation = ftRotate.StartValue;
            control.Active = true;
            control.Visible = true;

            tasks.TryAdd(Guid.NewGuid(), (d) =>
            {
                ftRotate.Update(d);

                control.Rotation = ftRotate.CurrentValue;

                if (ftRotate.CurrentValue == ftRotate.EndValue)
                {
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Shows a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenShow(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenAlpha(control, 0, 1, duration, fnc);
        }
        /// <summary>
        /// Hides a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenHide(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenAlpha(control, 1, 0, duration, fnc);
        }
        /// <summary>
        /// Changes the alpha component of a control color
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenAlpha(this UIControl control, float from, float to, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftAlpha = new FloatTween();

            ftAlpha.Start(from, to, duration, fnc);

            AddAlphaTween(control, ftAlpha);
        }
        /// <summary>
        /// Adds an alpha task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftAlpha">Alpha tween</param>
        private static void AddAlphaTween(UIControl control, FloatTween ftAlpha)
        {
            control.Alpha = ftAlpha.StartValue;
            control.Active = true;
            control.Visible = true;

            tasks.TryAdd(Guid.NewGuid(), (d) =>
            {
                ftAlpha.Update(d);

                control.Alpha = ftAlpha.CurrentValue;

                if (ftAlpha.CurrentValue == ftAlpha.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
    }
}
