using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Built-in materials
    /// </summary>
    public static class BuiltInMaterials
    {
        /// <summary>
        /// Emerald
        /// </summary>
        public static readonly BuiltInMaterial Emerald = new BuiltInMaterial { AmbientColor = new Color3(0.0215f, 0.1745f, 0.0215f), DiffuseColor = new Color4(0.07568f, 0.61424f, 0.07568f, 1f), SpecularColor = new Color3(0.633f, 0.727811f, 0.633f), Shininess = 0.6f };
        /// <summary>
        /// Jade
        /// </summary>
        public static readonly BuiltInMaterial Jade = new BuiltInMaterial { AmbientColor = new Color3(0.135f, 0.2225f, 0.1575f), DiffuseColor = new Color4(0.54f, 0.89f, 0.63f, 1f), SpecularColor = new Color3(0.316228f, 0.316228f, 0.316228f), Shininess = 0.1f };
        /// <summary>
        /// Obsidian
        /// </summary>
        public static readonly BuiltInMaterial Obsidian = new BuiltInMaterial { AmbientColor = new Color3(0.05375f, 0.05f, 0.06625f), DiffuseColor = new Color4(0.18275f, 0.17f, 0.22525f, 1f), SpecularColor = new Color3(0.332741f, 0.328634f, 0.346435f), Shininess = 0.3f };
        /// <summary>
        /// Pearl
        /// </summary>
        public static readonly BuiltInMaterial Pearl = new BuiltInMaterial { AmbientColor = new Color3(0.25f, 0.20725f, 0.20725f), DiffuseColor = new Color4(1f, 0.829f, 0.829f, 1f), SpecularColor = new Color3(0.296648f, 0.296648f, 0.296648f), Shininess = 0.088f };
        /// <summary>
        /// Ruby
        /// </summary>
        public static readonly BuiltInMaterial Ruby = new BuiltInMaterial { AmbientColor = new Color3(0.1745f, 0.01175f, 0.01175f), DiffuseColor = new Color4(0.61424f, 0.04136f, 0.04136f, 1f), SpecularColor = new Color3(0.727811f, 0.626959f, 0.626959f), Shininess = 0.6f };
        /// <summary>
        /// Turquoise
        /// </summary>
        public static readonly BuiltInMaterial Turquoise = new BuiltInMaterial { AmbientColor = new Color3(0.1f, 0.18725f, 0.1745f), DiffuseColor = new Color4(0.396f, 0.74151f, 0.69102f, 1f), SpecularColor = new Color3(0.297254f, 0.30829f, 0.306678f), Shininess = 0.1f };
        /// <summary>
        /// Brass
        /// </summary>
        public static readonly BuiltInMaterial Brass = new BuiltInMaterial { AmbientColor = new Color3(0.329412f, 0.223529f, 0.027451f), DiffuseColor = new Color4(0.780392f, 0.568627f, 0.113725f, 1f), SpecularColor = new Color3(0.992157f, 0.941176f, 0.807843f), Shininess = 0.21794872f };
        /// <summary>
        /// Bronze
        /// </summary>
        public static readonly BuiltInMaterial Bronze = new BuiltInMaterial { AmbientColor = new Color3(0.2125f, 0.1275f, 0.054f), DiffuseColor = new Color4(0.714f, 0.4284f, 0.18144f, 1f), SpecularColor = new Color3(0.393548f, 0.271906f, 0.166721f), Shininess = 0.2f };
        /// <summary>
        /// Chrome
        /// </summary>
        public static readonly BuiltInMaterial Chrome = new BuiltInMaterial { AmbientColor = new Color3(0.25f, 0.25f, 0.25f), DiffuseColor = new Color4(0.4f, 0.4f, 0.4f, 1f), SpecularColor = new Color3(0.774597f, 0.774597f, 0.774597f), Shininess = 0.6f };
        /// <summary>
        /// Copper
        /// </summary>
        public static readonly BuiltInMaterial Copper = new BuiltInMaterial { AmbientColor = new Color3(0.19125f, 0.0735f, 0.0225f), DiffuseColor = new Color4(0.7038f, 0.27048f, 0.0828f, 1f), SpecularColor = new Color3(0.256777f, 0.137622f, 0.086014f), Shininess = 0.1f };
        /// <summary>
        /// Gold
        /// </summary>
        public static readonly BuiltInMaterial Gold = new BuiltInMaterial { AmbientColor = new Color3(0.24725f, 0.1995f, 0.0745f), DiffuseColor = new Color4(0.75164f, 0.60648f, 0.22648f, 1f), SpecularColor = new Color3(0.628281f, 0.555802f, 0.366065f), Shininess = 0.4f };
        /// <summary>
        /// Silver
        /// </summary>
        public static readonly BuiltInMaterial Silver = new BuiltInMaterial { AmbientColor = new Color3(0.19225f, 0.19225f, 0.19225f), DiffuseColor = new Color4(0.50754f, 0.50754f, 0.50754f, 1f), SpecularColor = new Color3(0.508273f, 0.508273f, 0.508273f), Shininess = 0.4f };
        /// <summary>
        /// Black plastic
        /// </summary>
        public static readonly BuiltInMaterial BlackPlastic = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.0f, 0.0f), DiffuseColor = new Color4(0.01f, 0.01f, 0.01f, 1f), SpecularColor = new Color3(0.50f, 0.50f, 0.50f), Shininess = .25f };
        /// <summary>
        /// Cyan plastic
        /// </summary>
        public static readonly BuiltInMaterial CyanPlastic = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.1f, 0.06f), DiffuseColor = new Color4(0.0f, 0.50980392f, 0.50980392f, 1f), SpecularColor = new Color3(0.50196078f, 0.50196078f, 0.50196078f), Shininess = .25f };
        /// <summary>
        /// Green plastic
        /// </summary>
        public static readonly BuiltInMaterial GreenPlastic = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.0f, 0.0f), DiffuseColor = new Color4(0.1f, 0.35f, 0.1f, 1f), SpecularColor = new Color3(0.45f, 0.55f, 0.45f), Shininess = .25f };
        /// <summary>
        /// Red plastic
        /// </summary>
        public static readonly BuiltInMaterial RedPlastic = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.0f, 0.0f), DiffuseColor = new Color4(0.5f, 0.0f, 0.0f, 1f), SpecularColor = new Color3(0.7f, 0.6f, 0.6f), Shininess = .25f };
        /// <summary>
        /// White plastic
        /// </summary>
        public static readonly BuiltInMaterial WhitePlastic = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.0f, 0.0f), DiffuseColor = new Color4(0.55f, 0.55f, 0.55f, 1f), SpecularColor = new Color3(0.70f, 0.70f, 0.70f), Shininess = .25f };
        /// <summary>
        /// Yellow plastic
        /// </summary>
        public static readonly BuiltInMaterial YellowPlastic = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.0f, 0.0f), DiffuseColor = new Color4(0.5f, 0.5f, 0.0f, 1f), SpecularColor = new Color3(0.60f, 0.60f, 0.50f), Shininess = .25f };
        /// <summary>
        /// Black rubber
        /// </summary>
        public static readonly BuiltInMaterial BlackRubber = new BuiltInMaterial { AmbientColor = new Color3(0.02f, 0.02f, 0.02f), DiffuseColor = new Color4(0.01f, 0.01f, 0.01f, 1f), SpecularColor = new Color3(0.4f, 0.4f, 0.4f), Shininess = .078125f };
        /// <summary>
        /// Cyan rubber
        /// </summary>
        public static readonly BuiltInMaterial CyanRubber = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.05f, 0.05f), DiffuseColor = new Color4(0.4f, 0.5f, 0.5f, 1f), SpecularColor = new Color3(0.04f, 0.7f, 0.7f), Shininess = .078125f };
        /// <summary>
        /// Green rubber
        /// </summary>
        public static readonly BuiltInMaterial GreenRubber = new BuiltInMaterial { AmbientColor = new Color3(0.0f, 0.05f, 0.0f), DiffuseColor = new Color4(0.4f, 0.5f, 0.4f, 1f), SpecularColor = new Color3(0.04f, 0.7f, 0.04f), Shininess = .078125f };
        /// <summary>
        /// Red rubber
        /// </summary>
        public static readonly BuiltInMaterial RedRubber = new BuiltInMaterial { AmbientColor = new Color3(0.05f, 0.0f, 0.0f), DiffuseColor = new Color4(0.5f, 0.4f, 0.4f, 1f), SpecularColor = new Color3(0.7f, 0.04f, 0.04f), Shininess = .078125f };
        /// <summary>
        /// White rubber
        /// </summary>
        public static readonly BuiltInMaterial WhiteRubber = new BuiltInMaterial { AmbientColor = new Color3(0.05f, 0.05f, 0.05f), DiffuseColor = new Color4(0.5f, 0.5f, 0.5f, 1f), SpecularColor = new Color3(0.7f, 0.7f, 0.7f), Shininess = .078125f };
        /// <summary>
        /// Yellow rubber
        /// </summary>
        public static readonly BuiltInMaterial YellowRubber = new BuiltInMaterial { AmbientColor = new Color3(0.05f, 0.05f, 0.0f), DiffuseColor = new Color4(0.5f, 0.5f, 0.4f, 1f), SpecularColor = new Color3(0.7f, 0.7f, 0.04f), Shininess = .078125f };
    }
}
