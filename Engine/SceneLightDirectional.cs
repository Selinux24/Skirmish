using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public class SceneLightDirectional
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
        /// Light direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        public bool Enabled = false;
    }
}
