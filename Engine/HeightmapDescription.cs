using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Heightmap description
    /// </summary>
    public class HeightmapDescription
    {
        /// <summary>
        /// Terrain textures
        /// </summary>
        public class TexturesDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Textures";

            /// <summary>
            /// Normal maps
            /// </summary>
            public string[] NormalMaps = null;
            /// <summary>
            /// Specular maps
            /// </summary>
            public string[] SpecularMaps = null;

            /// <summary>
            /// Gets or sets if use alpha mapping or not
            /// </summary>
            public bool UseAlphaMapping = false;
            /// <summary>
            /// Alpha map
            /// </summary>
            public string AlphaMap = null;
            /// <summary>
            /// Color textures for alpha map
            /// </summary>
            public string[] ColorTextures = null;

            /// <summary>
            /// Gets or sets if use slope texturing or not
            /// </summary>
            public bool UseSlopes = false;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges = Vector2.Zero;
            /// <summary>
            /// High resolution textures
            /// </summary>
            public string[] TexturesHR = null;
            /// <summary>
            /// Low resolution textures
            /// </summary>
            public string[] TexturesLR = null;

            /// <summary>
            /// Lerping proportion between alpha mapping and slope texturing
            /// </summary>
            public float Proportion = 0f;
        }
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

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Height map file name
        /// </summary>
        public string HeightmapFileName = null;
        /// <summary>
        /// Color map file name
        /// </summary>
        public string ColormapFileName = null;
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize = 1;
        /// <summary>
        /// Maximum height
        /// </summary>
        public float MaximumHeight = 1;
        /// <summary>
        /// Textures
        /// </summary>
        public TexturesDescription Textures = new TexturesDescription();
        /// <summary>
        /// Terrain material
        /// </summary>
        public MaterialDescription Material = new MaterialDescription();
    }
}
