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
        /// Spot exponent used in the spotlight calculation to control the cone
        /// </summary>
        public float Spot = 0.0f;
        /// <summary>
        /// Stores the three attenuation constants in the format (a0, a1, a2) that control how light intensity falls off with distance
        /// </summary>
        /// <remarks>
        /// Constant weaken (1,0,0)
        /// Inverse distance weaken (0,1,0)
        /// Inverse square law (0,0,1)
        /// </remarks>
        public Vector3 Attenuation = Vector3.Zero;
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        public bool Enabled = false;

        /// <summary>
        /// Sets default light configuration
        /// </summary>
        public void SetDefault()
        {
            this.Ambient = Color.Yellow;
            this.Diffuse = Color.White;
            this.Specular = Color.LightBlue;
            this.Range = 10f;
            this.Spot = 5;
            this.Attenuation = new Vector3(1f, 0f, 0f);
        }
    }
}
