using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Game time
    /// </summary>
    public class GameTime : IGameTime
    {
        /// <summary>
        /// Stop watch
        /// </summary>
        private readonly TimerTick watch = new();

        /// <inheritdoc/>
        public TimeSpan ElapsedTime
        {
            get
            {
                return watch.ElapsedAdjustedTime;
            }
        }
        /// <inheritdoc/>
        public TimeSpan TotalTime
        {
            get
            {
                return watch.TotalTime;
            }
        }
        /// <inheritdoc/>
        public float ElapsedSeconds
        {
            get
            {
                return (float)ElapsedTime.TotalSeconds;
            }
        }
        /// <inheritdoc/>
        public float TotalSeconds
        {
            get
            {
                return (float)TotalTime.TotalSeconds;
            }
        }
        /// <inheritdoc/>
        public float ElapsedMilliseconds
        {
            get
            {
                return (float)ElapsedTime.TotalMilliseconds;
            }
        }
        /// <inheritdoc/>
        public float TotalMilliseconds
        {
            get
            {
                return (float)TotalTime.TotalMilliseconds;
            }
        }
        /// <inheritdoc/>
        public long Ticks
        {
            get
            {
                return TotalTime.Ticks;
            }
        }
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Start()
        {
            watch.Reset();
        }
        /// <inheritdoc/>
        public void Reset(long ticks = 0)
        {
            watch.Reset(ticks);
        }
        /// <inheritdoc/>
        public void Pause()
        {
            while (!watch.IsPaused)
            {
                watch.Pause();
            }
        }
        /// <inheritdoc/>
        public void Resume()
        {
            while (watch.IsPaused)
            {
                watch.Resume();
            }
        }
        /// <inheritdoc/>
        public void Update()
        {
            watch.Tick();
        }
    }
}
