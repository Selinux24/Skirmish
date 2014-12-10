using SharpDX;

namespace Engine.Content
{
    public class MaterialContent
    {
        public string Algorithm { get; set; }

        public string EmissionTexture { get; set; }
        public Color4 EmissionColor { get; set; }
        public string AmbientTexture { get; set; }
        public Color4 AmbientColor { get; set; }
        public string DiffuseTexture { get; set; }
        public Color4 DiffuseColor { get; set; }
        public string SpecularTexture { get; set; }
        public Color4 SpecularColor { get; set; }
        public string ReflectiveTexture { get; set; }
        public Color4 ReflectiveColor { get; set; }

        public float Shininess { get; set; }
        public float Reflectivity { get; set; }
        public float Transparency { get; set; }
        public float IndexOfRefraction { get; set; }

        public Color4 Transparent { get; set; }

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

        public override string ToString()
        {
            return string.Format("Algorithm: {0}; ", this.Algorithm);
        }
    }
}
