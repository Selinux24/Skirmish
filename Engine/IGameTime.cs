using System;

namespace Engine
{
    /// <summary>
    /// Game time interface
    /// </summary>
    public interface IGameTime
    {
        /// <summary>
        /// Elapsed time since last frame
        /// </summary>
        TimeSpan ElapsedTime { get; }
        /// <summary>
        /// Total time
        /// </summary>
        TimeSpan TotalTime { get; }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        float ElapsedSeconds { get; }
        /// <summary>
        /// Total seconds
        /// </summary>
        float TotalSeconds { get; }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        float ElapsedMilliseconds { get; }
        /// <summary>
        /// Total seconds
        /// </summary>
        float TotalMilliseconds { get; }
        /// <summary>
        /// Total ticks
        /// </summary>
        long Ticks { get; }
        /// <summary>
        /// Game time paused
        /// </summary>
        bool Paused { get; }

        /// <summary>
        /// Starts the stop watch
        /// </summary>
        void Start();
        /// <summary>
        /// Resets the stop watch
        /// </summary>
        /// <param name="ticks">Ticks to add</param>
        void Reset(long ticks = 0);
        /// <summary>
        /// Pauses the stop watch
        /// </summary>
        void Pause();
        /// <summary>
        /// Resumes the stop watch
        /// </summary>
        void Resume();
        /// <summary>
        /// Updates the stop watch counters
        /// </summary>
        void Update();
    }
}
