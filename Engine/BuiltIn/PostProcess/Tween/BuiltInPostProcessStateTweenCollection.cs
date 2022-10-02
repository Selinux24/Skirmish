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
        private readonly ConcurrentDictionary<BuiltInPostProcessState, List<Func<float, bool>>> taskList = new ConcurrentDictionary<BuiltInPostProcessState, List<Func<float, bool>>>();

        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
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
                    toDelete.ForEach(t => tasks.Remove(t));
                }
            }

            var emptyControls = taskList.Where(t => t.Value.Count == 0).Select(t => t.Key).ToList();
            if (emptyControls.Any())
            {
                emptyControls.ForEach(c => taskList.TryRemove(c, out _));
            }
        }

        /// <summary>
        /// Adds a new tween to the specified item
        /// </summary>
        /// <param name="item">Tween item</param>
        /// <param name="tween">Tween funcion</param>
        public void AddTween(BuiltInPostProcessState item, Func<float, bool> tween)
        {
            var list = taskList.GetOrAdd(item, new List<Func<float, bool>>());

            list.Add(tween);
        }
        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="item">Tween item</param>
        public void ClearTween(BuiltInPostProcessState item)
        {
            taskList.TryRemove(item, out _);
        }
        /// <summary>
        /// Clears all the tween tasks
        /// </summary>
        public void Clear()
        {
            taskList.Clear();
        }
    }
}
