using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Game time
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// Stop watch
        /// </summary>
        private readonly TimerTick watch = new();

        /// <summary>
        /// Elapsed time since last frame
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get
            {
                return watch.ElapsedAdjustedTime;
            }
        }
        /// <summary>
        /// Total time
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return watch.TotalTime;
            }
        }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        public float ElapsedSeconds
        {
            get
            {
                return (float)ElapsedTime.TotalSeconds;
            }
        }
        /// <summary>
        /// Total seconds
        /// </summary>
        public float TotalSeconds
        {
            get
            {
                return (float)TotalTime.TotalSeconds;
            }
        }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        public float ElapsedMilliseconds
        {
            get
            {
                return (float)ElapsedTime.TotalMilliseconds;
            }
        }
        /// <summary>
        /// Total seconds
        /// </summary>
        public float TotalMilliseconds
        {
            get
            {
                return (float)TotalTime.TotalMilliseconds;
            }
        }
        /// <summary>
        /// Total ticks
        /// </summary>
        public long Ticks
        {
            get
            {
                return TotalTime.Ticks;
            }
        }
        /// <summary>
        /// Game time paused
        /// </summary>
        public bool Paused
        {
            get
            {
                return watch.IsPaused;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameTime()
        {
            watch.Reset();
        }

        /// <summary>
        /// Starts the stop watch
        /// </summary>
        public void Start()
        {
            watch.Reset();
        }
        /// <summary>
        /// Resets the stop watch
        /// </summary>
        /// <param name="ticks">Ticks to add</param>
        public void Reset(long ticks = 0)
        {
            watch.Reset(ticks);
        }
        /// <summary>
        /// Pauses the stop watch
        /// </summary>
        public void Pause()
        {
            while (!watch.IsPaused)
            {
                watch.Pause();
            }
        }
        /// <summary>
        /// Resumes the stop watch
        /// </summary>
        public void Resume()
        {
            while (watch.IsPaused)
            {
                watch.Resume();
            }
        }
        /// <summary>
        /// Updates the stop watch counters
        /// </summary>
        public void Update()
        {
            watch.Tick();
        }
    }
}
