using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio progress event arguments class
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="effect">Effect</param>
    /// <param name="duration">Duration</param>
    /// <param name="position">Current position</param>
    public class GameAudioProgressEventArgs(IGameAudioEffect effect, TimeSpan duration, TimeSpan position) : GameAudioEventArgs(effect)
    {
        /// <summary>
        /// Total duration
        /// </summary>
        public TimeSpan TotalDuration { get; } = duration;
        /// <summary>
        /// Position
        /// </summary>
        public TimeSpan Position { get; } = position;
        /// <summary>
        /// Time to end
        /// </summary>
        public TimeSpan TimeToEnd { get; } = duration - position;
        /// <summary>
        /// Audio progress
        /// </summary>
        public float Progress { get; } = (float)(position.TotalSeconds / duration.TotalSeconds);
    }
}
