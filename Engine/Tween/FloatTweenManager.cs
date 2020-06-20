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
        private static readonly ConcurrentDictionary<UIControl, List<Func<float, bool>>> tasks = new ConcurrentDictionary<UIControl, List<Func<float, bool>>>();

        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public static void Update(GameTime gameTime)
        {
            if (!tasks.Any())
            {
                return;
            }

            // Copy active controls
            var activeControls = tasks.ToArray();

            foreach (var task in activeControls)
            {
                if (!task.Key.Active)
                {
                    continue;
                }

                // Copy active tasks
                var activeTasks = task.Value.ToList();
                if (!activeTasks.Any())
                {
                    continue;
                }

                List<Func<float, bool>> toDelete = new List<Func<float, bool>>();

                activeTasks.ForEach(t =>
                {
                    bool finished = t.Invoke(gameTime.ElapsedSeconds);
                    if (finished)
                    {
                        toDelete.Add(t);
                    }
                });

                if (toDelete.Any())
                {
                    toDelete.ForEach(t => task.Value.Remove(t));
                }
            }

            var emptyControls = tasks.Where(t => t.Value.Count == 0).Select(t => t.Key).ToList();
            if (emptyControls.Any())
            {
                emptyControls.ForEach(c => tasks.TryRemove(c, out _));
            }
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public static void ClearTween(UIControl control)
        {
            tasks.TryRemove(control, out _);
        }

        /// <summary>
        /// Adds a scale task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftScale">Scale tween</param>
        public static void AddScaleTween(UIControl control, FloatTween ftScale)
        {
            control.Scale = ftScale.StartValue;

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
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
        /// Adds a bouncing scale task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftScale">Scale tween</param>
        public static void AddScaleBounce(UIControl control, FloatTween ftScale)
        {
            control.Scale = ftScale.StartValue;

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
            {
                ftScale.Update(d);

                control.Scale = ftScale.CurrentValue;

                if (ftScale.CurrentValue == ftScale.EndValue)
                {
                    var start = ftScale.StartValue;
                    var end = ftScale.EndValue;

                    ftScale.Restart(end, start);
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a rotation task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftRotate">Rotation tween</param>
        public static void AddRotateTween(UIControl control, FloatTween ftRotate)
        {
            control.Rotation = ftRotate.StartValue;

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
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
        /// Adds an alpha task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftAlpha">Alpha tween</param>
        public static void AddAlphaTween(UIControl control, FloatTween ftAlpha)
        {
            control.Alpha = ftAlpha.StartValue;

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
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
