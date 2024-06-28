
namespace Engine.PathFinding
{
    /// <summary>
    /// Agent group settings interface
    /// </summary>
    public interface IGroupAgentSettings
    {
        /// <summary>
        /// Maximum allowed acceleration.
        /// </summary>
        float MaxAcceleration { get; set; }
        /// <summary>
        /// Maximum allowed speed.
        /// </summary>
        float MaxSpeed { get; set; }

        /// <summary>
        /// Obstacle avoidance
        /// </summary>
        bool ObstacleAvoidance { get; set; }
        /// <summary>
        /// Avoidance quality
        /// </summary>
        int AvoidanceQuality { get; set; }
        /// <summary>
        /// Separation
        /// </summary>
        bool Separation { get; set; }
        /// <summary>
        /// Separation weight
        /// </summary>
        float SeparationWeight { get; set; }
    }
}
