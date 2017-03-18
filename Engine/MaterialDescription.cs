using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Material description
    /// </summary>
    public class MaterialDescription
    {
        /// <summary>
        /// Emission color
        /// </summary>
        public Color4 EmissionColor { get; set; }
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color4 AmbientColor { get; set; }
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor { get; set; }
        /// <summary>
        /// Shininess factor
        /// </summary>
        public float Shininess { get; set; }

        /// <summary>
        /// Get material from description
        /// </summary>
        /// <returns>Returns the generated material from the description</returns>
        public Material GetMaterial()
        {
            return new Material()
            {
                EmissiveColor = this.EmissionColor,
                AmbientColor = this.AmbientColor,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                Shininess = this.Shininess,
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialDescription()
        {
            var m = Common.Material.Default;

            this.EmissionColor = m.EmissiveColor;
            this.AmbientColor = m.AmbientColor;
            this.DiffuseColor = m.DiffuseColor;
            this.SpecularColor = m.SpecularColor;
            this.Shininess = m.Shininess;
        }
    }

}
