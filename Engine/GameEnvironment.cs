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
        public static float LODDistanceMedium { get; set; } = 200f;
        /// <summary>
        /// Maximum distance for low level of detail models
        /// </summary>
        public static float LODDistanceLow { get; set; } = 500f;
        /// <summary>
        /// Maximum distance for minimum level of detail models
        /// </summary>
        public static float LODDistanceMinimum { get; set; } = 1000f;

        /// <summary>
        /// Gets the level of detail
        /// </summary>
        /// <param name="origin">Origin</param>
        /// <param name="coarseBoundingSphere">Coarse bounding sphere</param>
        /// <param name="localTransform">Local transform</param>
        /// <param name="averagingScale">Averaging scale</param>
        /// <returns>Returns the level of detail</returns>
        public static LevelOfDetail GetLOD(Vector3 origin, BoundingSphere? coarseBoundingSphere, Matrix localTransform, float averagingScale)
        {
            Vector3 position = localTransform.TranslationVector;
            float radius = 0f;

            if (coarseBoundingSphere.HasValue)
            {
                position = coarseBoundingSphere.Value.Center;
                radius = coarseBoundingSphere.Value.Radius;
            }

            float dist = Vector3.Distance(position, origin) - radius;
            if (dist < LODDistanceHigh)
            {
                return LevelOfDetail.High;
            }
            else if (dist < LODDistanceMedium)
            {
                return LevelOfDetail.Medium;
            }
            else if (dist < LODDistanceLow)
            {
                return LevelOfDetail.Low;
            }
            else if (dist < LODDistanceMinimum)
            {
                return LevelOfDetail.Minimum;
            }
            else
            {
                return LevelOfDetail.None;
            }
        }
    }
}
