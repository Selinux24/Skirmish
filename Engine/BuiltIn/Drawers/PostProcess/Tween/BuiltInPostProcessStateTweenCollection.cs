using Engine.Tween;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn.Drawers.PostProcess.Tween
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
            if (taskList.IsEmpty)
            {
                return;
            }

            // Copy active controls
            var activeControls = taskList
                .Where(task => task.Value.Count != 0)
                .Select(task => task.Value)
                .ToArray();

            foreach (var tasks in activeControls)
            {
                // Copy active tasks
                var activeTasks = tasks.ToList();

                activeTasks.ForEach(t =>
                {
                    bool finished = t.Invoke(gameTime.ElapsedSeconds);
                    if (finished)
                    {
                        tasks.Remove(t);
                    }
                });
            }

            var emptyControls = taskList.Where(t => t.Value.Count == 0).Select(t => t.Key).ToList();
            if (emptyControls.Count != 0)
            {
                emptyControls.ForEach(c => taskList.TryRemove(c, out _));
            }
        }

        /// <inheritdoc/>
        public void AddTween(BuiltInPostProcessState item, Func<float, bool> tween)
        {
            var list = taskList.GetOrAdd(item, []);

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
