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
            if (tasks.IsEmpty)
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
                if (activeTasks.Count == 0)
                {
                    continue;
                }

                List<Func<float, bool>> toDelete = [];

                foreach (var activeTask in activeTasks)
                {
                    if (activeTask(gameTime.ElapsedSeconds))
                    {
                        toDelete.Add(activeTask);
                    }
                }

                if (toDelete.Count > 0)
                {
                    toDelete.ForEach(t => task.Value.Remove(t));
                }
            }

            var emptyControls = tasks.Where(t => t.Value.Count == 0).Select(t => t.Key).ToList();
            if (emptyControls.Count > 0)
            {
                emptyControls.ForEach(c => tasks.TryRemove(c, out _));
            }
        }

        /// <inheritdoc/>
        public void AddTween(IUIControl item, Func<float, bool> tween)
        {
            var list = tasks.GetOrAdd(item, []);

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
