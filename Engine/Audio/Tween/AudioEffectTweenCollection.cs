using Engine.Tween;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Audio.Tween
{
    /// <summary>
    /// Audio effect tween collection
    /// </summary>
    class AudioEffectTweenCollection : ITweenCollection<IAudioEffect>
    {
        private readonly ConcurrentDictionary<IAudioEffect, List<Func<float, bool>>> tasks = new();

        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (!tasks.Any())
            {
                return;
            }

            // Copy active controls
            var activeControls = tasks.ToArray();

            foreach (var task in activeControls)
            {
                if (task.Key.State != Audio.AudioState.Playing)
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

        /// <summary>
        /// Adds a new tween to the specified item
        /// </summary>
        /// <param name="item">Tween item</param>
        /// <param name="tween">Tween funcion</param>
        public void AddTween(IAudioEffect item, Func<float, bool> tween)
        {
            var list = tasks.GetOrAdd(item, new List<Func<float, bool>>());

            list.Add(tween);
        }
        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="item">Tween item</param>
        public void ClearTween(IAudioEffect item)
        {
            tasks.TryRemove(item, out _);
        }
        /// <summary>
        /// Clears all the tween tasks
        /// </summary>
        public void Clear()
        {
            tasks.Clear();
        }
    }
}
