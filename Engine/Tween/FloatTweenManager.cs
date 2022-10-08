using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

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
        private static readonly ConcurrentBag<ITweenCollection> tweens = new ConcurrentBag<ITweenCollection>();

        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public static void Update(GameTime gameTime)
        {
            if (!tweens.Any())
            {
                return;
            }

            Parallel.ForEach(tweens.ToArray(), t =>
            {
                t?.Update(gameTime);
            });
        }

        /// <summary>
        /// Adds a new tween collection to the tween manager
        /// </summary>
        /// <param name="tweenCollection">Tween collection</param>
        public static void AddTweenCollection(ITweenCollection tweenCollection)
        {
            tweens.Add(tweenCollection);
        }
        /// <summary>
        /// Clears the tween manager
        /// </summary>
        public static void Clear()
        {
            while (!tweens.IsEmpty)
            {
                if (tweens.TryTake(out var tween))
                {
                    tween.Clear();
                }
            }
        }
    }
}
