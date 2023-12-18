using Engine.Tween;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.UI.Tween
{
    /// <summary>
    /// Tween collection for UIControl
    /// </summary>
    class UIControlTweenCollection : ITweenCollection<IUIControl>
    {
        /// <summary>
        /// Task list
        /// </summary>
        private readonly ConcurrentDictionary<IUIControl, List<Func<float, bool>>> tasks = new();

        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
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

                List<Func<float, bool>> toDelete = new();

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

        /// <inheritdoc/>
        public void AddTween(IUIControl item, Func<float, bool> tween)
        {
            var list = tasks.GetOrAdd(item, new List<Func<float, bool>>());

            list.Add(tween);
        }
        /// <inheritdoc/>
        public void ClearTween(IUIControl item)
        {
            tasks.TryRemove(item, out _);
        }
        /// <inheritdoc/>
        public void Clear()
        {
            tasks.Clear();
        }
    }
}
