using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Built-in material data
    /// </summary>
    public struct BuiltInMaterial
    {
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color3 AmbientColor;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color3 SpecularColor;
        /// <summary>
        /// Shininess factor
        /// </summary>
        public float Shininess;

        /// <summary>
        /// Phong material operator
        /// </summary>
        /// <param name="value">Built-in material</param>
        public static implicit operator MaterialPhongContent(BuiltInMaterial value)
        {
            return new MaterialPhongContent
            {
                AmbientColor = value.AmbientColor,
                DiffuseColor = value.DiffuseColor,
                SpecularColor = value.SpecularColor,
                Shininess = value.Shininess * 128f * 4f,
            };
        }
        /// <summary>
        /// Blinn-Phong material operator
        /// </summary>
        /// <param name="value">Built-in material</param>
        public static implicit operator MaterialBlinnPhongContent(BuiltInMaterial value)
        {
            return new MaterialBlinnPhongContent
            {
                AmbientColor = value.AmbientColor,
                DiffuseColor = value.DiffuseColor,
                SpecularColor = value.SpecularColor,
                Shininess = value.Shininess * 128f,
            };
        }
        /// <summary>
        /// Cook-Torrance material operator
        /// </summary>
        /// <param name="value">Built-in material</param>
        public static implicit operator MaterialCookTorranceContent(BuiltInMaterial value)
        {
            return new MaterialCookTorranceContent
            {
                AmbientColor = value.AmbientColor,
                DiffuseColor = value.DiffuseColor,
                SpecularColor = value.SpecularColor,
                Roughness = value.Shininess,
                Metallic = MaterialConstants.Metallic,
            };
        }
    }
}
