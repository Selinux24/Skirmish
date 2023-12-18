using System;

namespace Engine.Tween
{
    /// <summary>
    /// Tween collection
    /// </summary>
    public interface ITweenCollection
    {
        /// <summary>
        /// Updates the task list
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(IGameTime gameTime);
        /// <summary>
        /// Clears all the tween tasks
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Tween collection
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface ITweenCollection<T> : ITweenCollection where T : class
    {
        /// <summary>
        /// Adds a new tween to the specified item
        /// </summary>
        /// <param name="item">Tween item</param>
        /// <param name="tween">Tween funcion</param>
        void AddTween(T item, Func<float, bool> tween);
        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="item">Tween item</param>
        void ClearTween(T item);
    }
}
