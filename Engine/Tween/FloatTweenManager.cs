using Engine.UI;
using SharpDX;
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
                control.Visible = control.Scale != 0;

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
                control.Visible = control.Scale != 0;

                if (ftScale.CurrentValue == ftScale.EndValue)
                {
                    var newStart = ftScale.EndValue;
                    var newEnd = ftScale.StartValue;

                    ftScale.Restart(newStart, newEnd);
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
        /// Adds a color tweening task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftColorR">Red tween</param>
        /// <param name="ftColorG">Green tween</param>
        /// <param name="ftColorB">Blue tween</param>
        public static void AddColorTween(UIControl control, FloatTween ftColorR, FloatTween ftColorG, FloatTween ftColorB)
        {
            control.Color = new Color(ftColorR.StartValue, ftColorG.StartValue, ftColorB.StartValue);

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
            {
                ftColorR.Update(d);
                ftColorG.Update(d);
                ftColorB.Update(d);

                control.Color = new Color(ftColorR.CurrentValue, ftColorG.CurrentValue, ftColorB.CurrentValue);

                if (ftColorR.CurrentValue == ftColorR.EndValue && ftColorG.CurrentValue == ftColorG.EndValue && ftColorB.CurrentValue == ftColorB.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing color tweening task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftColorR">Red tween</param>
        /// <param name="ftColorG">Green tween</param>
        /// <param name="ftColorB">Blue tween</param>
        public static void AddColorBounce(UIControl control, FloatTween ftColorR, FloatTween ftColorG, FloatTween ftColorB)
        {
            control.Color = new Color(ftColorR.StartValue, ftColorG.StartValue, ftColorB.StartValue);

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
            {
                ftColorR.Update(d);
                ftColorG.Update(d);
                ftColorB.Update(d);

                control.Color = new Color(ftColorR.CurrentValue, ftColorG.CurrentValue, ftColorB.CurrentValue);

                if (ftColorR.CurrentValue == ftColorR.EndValue && ftColorG.CurrentValue == ftColorG.EndValue && ftColorB.CurrentValue == ftColorB.EndValue)
                {
                    var newStartR = ftColorR.EndValue;
                    var newStartG = ftColorG.EndValue;
                    var newStartB = ftColorB.EndValue;

                    var newEndR = ftColorR.StartValue;
                    var newEndG = ftColorG.StartValue;
                    var newEndB = ftColorB.StartValue;

                    ftColorR.Restart(newStartR, newEndR);
                    ftColorG.Restart(newStartG, newEndG);
                    ftColorB.Restart(newStartB, newEndB);
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
                control.Visible = control.Alpha != 0;

                if (ftAlpha.CurrentValue == ftAlpha.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing alpha task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftAlpha">Alpha tween</param>
        public static void AddAlphaBounce(UIControl control, FloatTween ftAlpha)
        {
            control.Alpha = ftAlpha.StartValue;

            var list = tasks.GetOrAdd(control, new List<Func<float, bool>>());
            list.Add((d) =>
            {
                ftAlpha.Update(d);

                control.Alpha = ftAlpha.CurrentValue;
                control.Visible = control.Alpha != 0;

                if (ftAlpha.CurrentValue == ftAlpha.EndValue)
                {
                    var newStart = ftAlpha.EndValue;
                    var newEnd = ftAlpha.StartValue;

                    ftAlpha.Restart(newStart, newEnd);
                }

                return false;
            });
        }
    }
}
