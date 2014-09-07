using SharpDX;

namespace Common.Utils
{
    public struct Material
    {
        public static readonly Material Default = new Material()
        {
            Emission = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
            Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
            Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
            Specular = new Color4(0.0f, 0.0f, 0.0f, 50.0f),
            Reflective = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            Transparent = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            IndexOfRefraction = 1,
        };

        public static Material CreateTextured(string texture)
        {
            Material mat = Material.Default;

            mat.Diffuse = new Color4(1f);
            mat.Texture = new TextureDescription()
            {
                Name = texture,
                TextureArray = new string[] { texture },
            };

            return mat;
        }
        public static Material CreateTextured(string arrayName, string[] textures)
        {
            Material mat = Material.Default;

            mat.Diffuse = new Color4(1f);
            mat.Texture = new TextureDescription()
            {
                Name = arrayName,
                TextureArray = textures,
            };

            return mat;
        }

        public Color4 Emission;
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Color4 Reflective;
        public Color4 Transparent;
        public float IndexOfRefraction;
        public TextureDescription Texture;
        public bool Textured
        {
            get
            {
                return (this.Texture != null);
            }
        }
    };
}
