using Engine.Tween;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Tween collection for post processing parameters
    /// </summary>
    class BuiltInPostProcessStateTweenCollection : ITweenCollection<BuiltInPostProcessState>
    {
        /// <summary>
        /// Task list
        /// </summary>
        private readonly ConcurrentDictionary<BuiltInPostProcessState, List<Func<float, bool>>> taskList = new();

        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            if (!taskList.Any())
            {
                return;
            }

            // Copy active controls
            var activeControls = taskList
                .Where(task => task.Value.Any())
                .Select(task => task.Value)
                .ToArray();

            foreach (var tasks in activeControls)
            {
                // Copy active tasks
                var activeTasks = tasks.ToList();

                var toDelete = new List<Func<float, bool>>();

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
                    toDelete.ForEach(t => tasks.Remove(t));
                }
            }

            var emptyControls = taskList.Where(t => t.Value.Count == 0).Select(t => t.Key).ToList();
            if (emptyControls.Any())
            {
                emptyControls.ForEach(c => taskList.TryRemove(c, out _));
            }
        }

        /// <inheritdoc/>
        public void AddTween(BuiltInPostProcessState item, Func<float, bool> tween)
        {
            var list = taskList.GetOrAdd(item, new List<Func<float, bool>>());

            list.Add(tween);
        }
        /// <inheritdoc/>
        public void ClearTween(BuiltInPostProcessState item)
        {
            taskList.TryRemove(item, out _);
        }
        /// <inheritdoc/>
        public void Clear()
        {
            taskList.Clear();
        }
    }
}
