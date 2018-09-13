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
        private readonly TimerTick watch = new TimerTick();

        /// <summary>
        /// Elapsed time since last frame
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get
            {
                return this.watch.ElapsedAdjustedTime;
            }
        }
        /// <summary>
        /// Total time
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return this.watch.TotalTime;
            }
        }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        public float ElapsedSeconds
        {
            get
            {
                return (float)this.ElapsedTime.TotalSeconds;
            }
        }
        /// <summary>
        /// Total seconds
        /// </summary>
        public float TotalSeconds
        {
            get
            {
                return (float)this.TotalTime.TotalSeconds;
            }
        }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        public float ElapsedMilliseconds
        {
            get
            {
                return (float)this.ElapsedTime.TotalMilliseconds;
            }
        }
        /// <summary>
        /// Total seconds
        /// </summary>
        public float TotalMilliseconds
        {
            get
            {
                return (float)this.TotalTime.TotalMilliseconds;
            }
        }
        /// <summary>
        /// Total ticks
        /// </summary>
        public long Ticks
        {
            get
            {
                return this.TotalTime.Ticks;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameTime()
        {
            this.watch.Reset();
        }

        /// <summary>
        /// Starts the stop watch
        /// </summary>
        public void Start()
        {
            this.watch.Reset();
        }
        /// <summary>
        /// Resets the stop watch
        /// </summary>
        public void Reset()
        {
            this.watch.Reset();
        }
        /// <summary>
        /// Pauses the stop watch
        /// </summary>
        public void Pause()
        {
            this.watch.Pause();
        }
        /// <summary>
        /// Resumes the stop watch
        /// </summary>
        public void Resume()
        {
            this.watch.Resume();
        }
        /// <summary>
        /// Updates the stop watch counters
        /// </summary>
        public void Update()
        {
            this.watch.Tick();
        }
    }
}
