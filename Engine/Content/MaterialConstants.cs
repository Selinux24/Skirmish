
namespace Engine.Content
{
    using Engine.Mathematics;

    /// <summary>
    /// Material constant values
    /// </summary>
    public static class MaterialConstants
    {
        /// <summary>
        /// Ambient color
        /// </summary>
        public static Vector3 AmbientColor { get; } = new Vector3(0.5f, 0.5f, 0.5f);
        /// <summary>
        /// Diffuse color
        /// </summary>
        public static Vector4 DiffuseColor { get; } = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
        /// <summary>
        /// Specular color
        /// </summary>
        public static Vector3 SpecularColor { get; } = new Vector3(0.25f, 0.25f, 0.25f);
        /// <summary>
        /// Emissive color
        /// </summary>
        public static Vector3 EmissiveColor { get; } = new Vector3(0.0f, 0.0f, 0.0f);

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
        public static float Metallic { get; set; } = 0.9f;
        /// <summary>
        /// Roughness
        /// </summary>
        public static float Roughness { get; set; } = 0.2f;

        #endregion
    }
}
