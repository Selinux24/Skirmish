using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Material constant values
    /// </summary>
    public static class MaterialConstants
    {
        /// <summary>
        /// Ambient color
        /// </summary>
        public static Color3 AmbientColor { get; } = new Color3(0.2f, 0.2f, 0.1f);
        /// <summary>
        /// Diffuse color
        /// </summary>
        public static Color4 DiffuseColor { get; } = new Color4(0.8f, 0.8f, 0.8f, 1.0f);
        /// <summary>
        /// Specular color
        /// </summary>
        public static Color3 SpecularColor { get; } = new Color3(0.25f, 0.25f, 0.25f);
        /// <summary>
        /// Emissive color
        /// </summary>
        public static Color3 EmissiveColor { get; } = new Color3(0.0f, 0.0f, 0.0f);

        #region Phong / Blinn-Phong specs

        /// <summary>
        /// Shininess factor
        /// </summary>
        public static float Shininess { get; set; } = 32f;

        #endregion

        #region Cook-Torrance specs

        /// <summary>
        /// Metallic
        /// </summary>
        public static float Metallic { get; set; } = 0.0f;
        /// <summary>
        /// Roughness
        /// </summary>
        public static float Roughness { get; set; } = 0.0f;

        #endregion
    }
}
