using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene ligth
    /// </summary>
    public abstract class SceneLight
    {
        /// <summary>
        /// Light name
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        public bool Enabled = false;
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
        /// Gets the text representation of the light
        /// </summary>
        /// <returns>Returns the text representation of the light</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0}; Enabled: {1}", this.Name, this.Enabled);
            }
            else
            {
                return string.Format("{0}; Enabled {1}", this.GetType(), this.Enabled);
            }
        }
    }
}
