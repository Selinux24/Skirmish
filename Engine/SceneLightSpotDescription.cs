using SharpDX;

namespace Engine
{
    /// <summary>
    /// Light spot description
    /// </summary>
    public struct SceneLightSpotDescription
    {
        /// <summary>
        /// Creates a new spot light description
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="direction">Direction</param>
        /// <param name="fallOffAngle">Fall-of angle in degrees</param>
        /// <param name="radius">Cone radius</param>
        /// <param name="intensity">Intensity</param>
        /// <returns>Returns the new description</returns>
        public static SceneLightSpotDescription Create(Vector3 position, Vector3 direction, float fallOffAngle, float radius, float intensity)
        {
            return new SceneLightSpotDescription
            {
                Position = position,
                Direction = direction,
                FallOffAngle = fallOffAngle,
                Radius = radius,
                Intensity = intensity,
            };
        }

        /// <summary>
        /// Cone apex position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Cone direction
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// Fall-of angle in degrees
        /// </summary>
        public float FallOffAngle { get; set; }
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Light intensity
        /// </summary>
        public float Intensity { get; set; }
    }
}
