using System;
using System.Diagnostics;

namespace Engine
{
    /// <summary>
    /// Game time
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// Stop watch
        /// </summary>
        private Stopwatch watch = new Stopwatch();
        /// <summary>
        /// Previous elapsed time for per frame calculations
        /// </summary>
        private TimeSpan previousElapsedTime;

        /// <summary>
        /// Elapsed time since last frame
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }
        /// <summary>
        /// Total time
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return this.watch.Elapsed;
            }
        }
        /// <summary>
        /// Elapsed seconds since last frame
        /// </summary>
        public float ElapsedSeconds
        {
            get{
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
        /// Constructor
        /// </summary>
        public GameTime()
        {
            this.Start();
        }

        /// <summary>
        /// Resets the stop watch
        /// </summary>
        public void Reset()
        {
            this.previousElapsedTime = this.ElapsedTime = TimeSpan.Zero;

            this.watch.Reset();
        }
        /// <summary>
        /// Starts the stop watch
        /// </summary>
        public void Start()
        {
            this.previousElapsedTime = this.ElapsedTime = TimeSpan.Zero;

            this.watch.Start();
        }
        /// <summary>
        /// Stops the stop watch
        /// </summary>
        public void Stop()
        {
            this.watch.Stop();

            this.Update();
        }
        /// <summary>
        /// Updates the stop watch counters
        /// </summary>
        public void Update()
        {
            TimeSpan elapsedTime = this.watch.Elapsed;

            this.ElapsedTime = elapsedTime - this.previousElapsedTime;

            this.previousElapsedTime = elapsedTime;
        }
    }
}
