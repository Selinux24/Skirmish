using SharpDX;
using System;

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
        /// <param name="angle">Fall-of angle</param>
        /// <param name="radius">Cone radius</param>
        /// <param name="intensity">Intensity</param>
        /// <returns>Returns the new description</returns>
        public static SceneLightSpotDescription Create(Vector3 position, Vector3 direction, float angle, float radius, float intensity)
        {
            float f = Math.Abs(Vector3.Dot(direction, Vector3.Up));
            var transform = Helper.CreateWorld(position, direction, f == 1 ? Vector3.ForwardLH : Vector3.Up);

            return new SceneLightSpotDescription
            {
                Transform = transform,
                Angle = angle,
                Radius = radius,
                Intensity = intensity,
            };
        }

        /// <summary>
        /// Light transform
        /// </summary>
        public Matrix Transform { get; set; }
        /// <summary>
        /// Fall-of angle
        /// </summary>
        public float Angle { get; set; }
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
