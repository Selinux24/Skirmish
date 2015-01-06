using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Material content
    /// </summary>
    public class MaterialContent
    {
        /// <summary>
        /// Default material content
        /// </summary>
        public static MaterialContent Default
        {
            get
            {
                return new MaterialContent()
                {
                    EmissionColor = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                    AmbientColor = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                    DiffuseColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    SpecularColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
                    ReflectiveColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
                    Transparent = new Color4(0.0f, 0.0f, 0.0f, 0.0f),

                    IndexOfRefraction = 1.0f,
                    Reflectivity = 0.0f,
                    Shininess = 50.0f,
                    Transparency = 0.0f,
                };
            }
        }

        /// <summary>
        /// Algorithm name
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Emission texture name
        /// </summary>
        public string EmissionTexture { get; set; }
        /// <summary>
        /// Emission color
        /// </summary>
        public Color4 EmissionColor { get; set; }
        /// <summary>
        /// Ambient texture name
        /// </summary>
        public string AmbientTexture { get; set; }
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color4 AmbientColor { get; set; }
        /// <summary>
        /// Diffuse texture name
        /// </summary>
        public string DiffuseTexture { get; set; }
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor { get; set; }
        /// <summary>
        /// Specular texture name
        /// </summary>
        public string SpecularTexture { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor { get; set; }
        /// <summary>
        /// Reflectuve texture name
        /// </summary>
        public string ReflectiveTexture { get; set; }
        /// <summary>
        /// Reflective color
        /// </summary>
        public Color4 ReflectiveColor { get; set; }
        /// <summary>
        /// Shininess factor
        /// </summary>
        public float Shininess { get; set; }
        /// <summary>
        /// Reflectivity factor
        /// </summary>
        public float Reflectivity { get; set; }
        /// <summary>
        /// Transparency
        /// </summary>
        public float Transparency { get; set; }
        /// <summary>
        /// Index of refraction
        /// </summary>
        public float IndexOfRefraction { get; set; }
        /// <summary>
        /// Transparent color
        /// </summary>
        public Color4 Transparent { get; set; }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            return string.Format("Algorithm: {0}; ", this.Algorithm);
        }
    }
}
