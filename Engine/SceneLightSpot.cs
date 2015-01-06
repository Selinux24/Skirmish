using SharpDX;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot
    {
        /// <summary>
        /// Ambient light
        /// </summary>
        public Color4 Ambient = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Diffuse absorption light
        /// </summary>
        public Color4 Diffuse = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Refraction light
        /// </summary>
        public Color4 Specular = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position = Vector3.Zero;
        /// <summary>
        /// Light range
        /// </summary>
        public float Range = 0.0f;
        /// <summary>
        /// Ligth direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Spot radious
        /// </summary>
        public float Spot = 0.0f;
        /// <summary>
        /// Light attributes
        /// </summary>
        public Vector3 Attributes = Vector3.Zero;
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        public bool Enabled = false;
    }
}
