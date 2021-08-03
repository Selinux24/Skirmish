using System;
using System.Diagnostics;

namespace Engine.Common
{
    /// <summary>
    /// This provides timing information similar to <see cref="Stopwatch"/> but an update occurring only on a <see cref="Tick"/> method.
    /// </summary>
    public class TimerTick
    {
        private long startRawTime;
        private long lastRawTime;
        private int pauseCount;
        private long pauseStartTime;
        private long timePaused;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerTick"/> class.
        /// </summary>
        public TimerTick()
        {
            Reset();
        }

        /// <summary>
        /// Gets the total time elapsed since the last reset or when this timer was created.
        /// </summary>
        public TimeSpan TotalTime { get; private set; }
        /// <summary>
        /// Gets the elapsed adjusted time since the previous call to <see cref="Tick"/> taking into account <see cref="Pause"/> time.
        /// </summary>
        public TimeSpan ElapsedAdjustedTime { get; private set; }
        /// <summary>
        /// Gets the elapsed time since the previous call to <see cref="Tick"/>.
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is paused.
        /// </summary>
        /// <value><c>true</c> if this instance is paused; otherwise, <c>false</c>.</value>
        public bool IsPaused
        {
            get
            {
                return pauseCount > 0;
            }
        }

        /// <summary>
        /// Resets this instance. <see cref="TotalTime"/> is set to zero or ticks (when specified).
        /// </summary>
        /// <param name="ticks">Ticks to set</param>
        public void Reset(long ticks = 0)
        {
            TotalTime = TimeSpan.FromTicks(ticks);
            startRawTime = Stopwatch.GetTimestamp() - ticks;
            lastRawTime = startRawTime;
        }
        /// <summary>
        /// Resumes this instance, only if a call to <see cref="Pause"/> has been already issued.
        /// </summary>
        public void Resume()
        {
            pauseCount--;
            if (pauseCount <= 0)
            {
                timePaused += Stopwatch.GetTimestamp() - pauseStartTime;
                pauseStartTime = 0L;
            }
        }
        /// <summary>
        /// Update the <see cref="TotalTime"/> and <see cref="ElapsedTime"/>,
        /// </summary>
        /// <remarks>
        /// This method must be called on a regular basis at every *tick*.
        /// </remarks>
        public void Tick()
        {
            // Don't tick when this instance is paused.
            if (IsPaused)
            {
                return;
            }

            var rawTime = Stopwatch.GetTimestamp();
            TotalTime = ConvertRawToTimestamp(rawTime - startRawTime);
            ElapsedTime = ConvertRawToTimestamp(rawTime - lastRawTime);
            ElapsedAdjustedTime = ConvertRawToTimestamp(rawTime - (lastRawTime + timePaused));

            if (ElapsedAdjustedTime < TimeSpan.Zero)
            {
                ElapsedAdjustedTime = TimeSpan.Zero;
            }

            timePaused = 0;
            lastRawTime = rawTime;
        }
        /// <summary>
        /// Pauses this instance.
        /// </summary>
        public void Pause()
        {
            pauseCount++;
            if (pauseCount == 1)
            {
                pauseStartTime = Stopwatch.GetTimestamp();
            }
        }

        /// <summary>
        /// Converts a <see cref="Stopwatch" /> raw time to a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="delta">The delta.</param>
        /// <returns>The <see cref="TimeSpan" />.</returns>
        private static TimeSpan ConvertRawToTimestamp(long delta)
        {
            return TimeSpan.FromTicks((delta * 10000000) / Stopwatch.Frequency);
        }
    }
}
