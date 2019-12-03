using SharpDX;
using System;

namespace Engine.Content
{
    /// <summary>
    /// Light content
    /// </summary>
    public class LightContent
    {
        /// <summary>
        /// Light type
        /// </summary>
        public LightContentTypes LightType { get; set; } = LightContentTypes.Unknown;
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Initial light transform
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return Matrix.Transformation(Vector3.Zero, Quaternion.Identity, this.Scale, Vector3.Zero, this.Rotation, this.Position);
            }
            set
            {
                value.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);

                this.Position = translation;
                this.Rotation = rotation;
                this.Scale = scale;
            }
        }
        /// <summary>
        /// Initial position
        /// </summary>
        public Vector3 Position { get; private set; }
        /// <summary>
        /// Initial rotation
        /// </summary>
        public Quaternion Rotation { get; private set; }
        /// <summary>
        /// Initial scale
        /// </summary>
        public Vector3 Scale { get; private set; }
        /// <summary>
        /// Light color
        /// </summary>
        public Color4 Color { get; set; }
        /// <summary>
        /// Constant attenuation
        /// </summary>
        public float ConstantAttenuation { get; set; }
        /// <summary>
        /// Linear attenuation
        /// </summary>
        public float LinearAttenuation { get; set; }
        /// <summary>
        /// Quadratic attenuation
        /// </summary>
        public float QuadraticAttenuation { get; set; }
        /// <summary>
        /// Falloff angle
        /// </summary>
        public float FallOffAngle { get; set; }
        /// <summary>
        /// Falloff exponent
        /// </summary>
        public float FallOffExponent { get; set; }

        /// <summary>
        /// Creates a new spot light from content
        /// </summary>
        /// <returns>Returns the new generate spot light</returns>
        public SceneLightSpot CreateSpotLight()
        {
            Vector3 direction = Vector3.TransformNormal(Vector3.Up, Matrix.RotationQuaternion(this.Rotation));
            float radius = this.GetRadius(this.QuadraticAttenuation, 0.5f);
            float intensity = this.ConstantAttenuation * radius;

            var desc = new SceneLightSpotDescription
            {
                Position = this.Position,
                Direction = direction,
                Angle = this.FallOffAngle,
                Radius = radius,
                Intensity = intensity,
            };

            return new SceneLightSpot(this.Name, true, this.Color, this.Color, true, desc);
        }
        /// <summary>
        /// Creates a new point light from content
        /// </summary>
        /// <returns>Returns the new generate point light</returns>
        public SceneLightPoint CreatePointLight()
        {
            float radius = this.GetRadius(this.QuadraticAttenuation, 0.5f);
            float intensity = this.ConstantAttenuation * radius;

            var desc = new SceneLightPointDescription
            {
                Transform = this.Transform,
                Radius = radius,
                Intensity = intensity,
            };

            return new SceneLightPoint(this.Name, true, this.Color, this.Color, true, desc);
        }

        /// <summary>
        /// Get light radius
        /// </summary>
        /// <param name="quadraticAtt">Quadratic attenuation value</param>
        /// <param name="minLight">Minimum light value</param>
        /// <returns>Returns the light radius</returns>
        private float GetRadius(float quadraticAtt, float minLight)
        {
            return (float)Math.Sqrt(1.0f / (quadraticAtt * minLight));
        }
    }
}
