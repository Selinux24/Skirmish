using SharpDX;

namespace Engine
{
    /// <summary>
    /// Global variables 
    /// </summary>
    public static class GameEnvironment
    {
        /// <summary>
        /// Background color
        /// </summary>
        public static Color4 Background { get; set; } = Color.Black.ToColor4();
        /// <summary>
        /// Gravity
        /// </summary>
        public static Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);

        /// <summary>
        /// Maximum distance for high level of detail models
        /// </summary>
        public static float LODDistanceHigh { get; set; } = 100f;
        /// <summary>
        /// Maximum distance for medium level of detail models
        /// </summary>
        public static float LODDistanceMedium { get; set; } = 150f;
        /// <summary>
        /// Maximum distance for low level of detail models
        /// </summary>
        public static float LODDistanceLow { get; set; } = 300f;
        /// <summary>
        /// Maximum distance for minimum level of detail models
        /// </summary>
        public static float LODDistanceMinimum { get; set; } = 1000f;
    }
}
