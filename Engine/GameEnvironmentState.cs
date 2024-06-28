using Engine.Content;

namespace Engine
{
    /// <summary>
    /// Game environment state
    /// </summary>
    public class GameEnvironmentState : IGameState
    {
        /// <summary>
        /// Background color
        /// </summary>
        public ColorRgba Background { get; set; }
        /// <summary>
        /// Gravity
        /// </summary>
        public Direction3 Gravity { get; set; }

        /// <summary>
        /// Maximum distance for high level of detail models
        /// </summary>
        public float LodDistanceHigh { get; set; }
        /// <summary>
        /// Maximum distance for medium level of detail models
        /// </summary>
        public float LodDistanceMedium { get; set; }
        /// <summary>
        /// Maximum distance for low level of detail models
        /// </summary>
        public float LodDistanceLow { get; set; }
        /// <summary>
        /// Maximum distance for minimum level of detail models
        /// </summary>
        public float LodDistanceMinimum { get; set; }

        /// <summary>
        /// The engine will discard all lights where: Distance / light radius < threshold
        /// </summary>
        public float ShadowRadiusDistanceThreshold { get; set; }

        /// <summary>
        /// Maximum distance for High level detailed shadows
        /// </summary>
        public float ShadowDistanceHigh { get; set; }
        /// <summary>
        /// Maximum distance for Medium level detailed shadows
        /// </summary>
        public float ShadowDistanceMedium { get; set; }
        /// <summary>
        /// Maximum distance for Low level detailed shadows
        /// </summary>
        public float ShadowDistanceLow { get; set; }

        /// <summary>
        /// Time of day controller
        /// </summary>
        public TimeOfDay TimeOfDay { get; set; }
    }
}
