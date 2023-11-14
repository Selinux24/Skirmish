using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio progress event arguments class
    /// </summary>
    public class GameAudioProgressEventArgs : GameAudioEventArgs
    {
        /// <summary>
        /// Total duration
        /// </summary>
        public TimeSpan TotalDuration { get; }
        /// <summary>
        /// Position
        /// </summary>
        public TimeSpan Position { get; }
        /// <summary>
        /// Time to end
        /// </summary>
        public TimeSpan TimeToEnd { get; }
        /// <summary>
        /// Audio progress
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="duration">Duration</param>
        /// <param name="position">Current position</param>
        public GameAudioProgressEventArgs(IGameAudioEffect effect, TimeSpan duration, TimeSpan position) : base(effect)
        {
            TotalDuration = duration;
            Position = position;
            TimeToEnd = duration - position;
            Progress = (float)(position.TotalSeconds / duration.TotalSeconds);
        }
    }
}
