using SharpDX;

namespace Engine
{
    /// <summary>
    /// Light point description
    /// </summary>
    public struct SceneLightPointDescription
    {
        /// <summary>
        /// Creates a point light description
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Point radius</param>
        /// <param name="intensity">Intensity</param>
        /// <returns>Returns the new point light description</returns>
        public static SceneLightPointDescription Create(Vector3 position, float radius, float intensity)
        {
            return new SceneLightPointDescription
            {
                Transform = Matrix.Translation(position),
                Radius = radius,
                Intensity = intensity,
            };
        }

        /// <summary>
        /// Light transform
        /// </summary>
        public Matrix Transform { get; set; }
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; set; }
    }
}

