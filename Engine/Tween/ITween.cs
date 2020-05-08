
namespace Engine.Tween
{
    /// <summary>
    /// Interface for a tween object.
    /// </summary>
    public interface ITween
    {
        /// <summary>
        /// Gets the current state of the tween.
        /// </summary>
        TweenState State { get; }

        /// <summary>
        /// Pauses the tween.
        /// </summary>
        void Pause();
        /// <summary>
        /// Resumes the paused tween.
        /// </summary>
        void Resume();
        /// <summary>
        /// Stops the tween.
        /// </summary>
        /// <param name="stopBehavior">The behavior to use to handle the stop.</param>
        void Stop(StopBehavior stopBehavior);
        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time to add to the tween.</param>
        void Update(float elapsedTime);
    }

    /// <summary>
    /// Interface for a tween object that handles a specific type.
    /// </summary>
    /// <typeparam name="T">The type to tween.</typeparam>
    public interface ITween<T> : ITween where T : struct
    {
        /// <summary>
        /// Gets the current value of the tween.
        /// </summary>
        T CurrentValue { get; }

        /// <summary>
        /// Starts a tween.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="duration">The duration of the tween.</param>
        /// <param name="scaleFunc">A function used to scale progress over time.</param>
        void Start(T start, T end, float duration, ScaleFunc scaleFunc);
    }
}
