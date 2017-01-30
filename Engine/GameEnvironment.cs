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
        public static Color4 Background = Color.Black.ToColor4();
        /// <summary>
        /// Gravity
        /// </summary>
        public static readonly Vector3 Gravity = new Vector3(0, -9.8f, 0);

        /// <summary>
        /// Maximum distance for high level of detail models
        /// </summary>
        public static float LODDistanceHigh = 10f;
        /// <summary>
        /// Maximum distance for medium level of detail models
        /// </summary>
        public static float LODDistanceMedium = 50f;
        /// <summary>
        /// Maximum distance for low level of detail models
        /// </summary>
        public static float LODDistanceLow = 250f;
        /// <summary>
        /// Maximum distance for minimum level of detail models
        /// </summary>
        public static float LODDistanceMinimum = 1250f;
    }
}
