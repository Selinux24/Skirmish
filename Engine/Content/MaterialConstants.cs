using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Material constant values
    /// </summary>
    public static class MaterialConstants
    {
        /// <summary>
        /// Diffuse color
        /// </summary>
        public static Color4 DiffuseColor { get; } = new Color4(0.8f, 0.8f, 0.8f, 1.0f);
        /// <summary>
        /// Emissive color
        /// </summary>
        public static Color3 EmissiveColor { get; } = new Color3(0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Ambient color
        /// </summary>
        public static Color3 AmbientColor { get; } = new Color3(0.1f, 0.1f, 0.1f);
        /// <summary>
        /// Specular color
        /// </summary>
        public static Color3 SpecularColor { get; } = new Color3(0.25f, 0.25f, 0.25f);

        #region Phong / Blinn-Phong specs

        /// <summary>
        /// Shininess factor
        /// </summary>
        public static float Shininess { get; set; } = 50f;

        #endregion

        #region Cook-Torrance specs

        /// <summary>
        /// F0
        /// </summary>
        public static float F0 { get; set; } = 0.8f;
        /// <summary>
        /// Roughness
        /// </summary>
        public static float Roughness { get; set; } = 0.1f;
        /// <summary>
        /// K
        /// </summary>
        public static float K { get; set; } = 0.2f;

        #endregion
    }
}
