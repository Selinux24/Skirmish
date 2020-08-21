using SharpDX;
using System.Linq;

namespace Engine
{
    using Engine.Content;

    /// <summary>
    /// Heightmap description
    /// </summary>
    public class HeightmapDescription
    {
        /// <summary>
        /// Creates a height map descripton from map data
        /// </summary>
        /// <param name="heightmap">Height map</param>
        /// <param name="cellsize">Cell size</param>
        /// <param name="maximumHeight">Maximum height</param>
        /// <param name="heightCurve">Height curve</param>
        public static HeightmapDescription FromMap(NoiseMap heightmap, float cellsize, float maximumHeight, Curve heightCurve)
        {
            return new HeightmapDescription
            {
                Heightmap = heightmap.Map,
                CellSize = cellsize,
                MaximumHeight = maximumHeight,
                HeightCurve = heightCurve,
            };
        }

        /// <summary>
        /// Terrain textures
        /// </summary>
        public class TexturesDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath { get; set; } = "Textures";

            /// <summary>
            /// Normal maps
            /// </summary>
            public string[] NormalMaps { get; set; } = null;
            /// <summary>
            /// Specular maps
            /// </summary>
            public string[] SpecularMaps { get; set; } = null;

            /// <summary>
            /// Gets or sets if use alpha mapping or not
            /// </summary>
            public bool UseAlphaMapping { get; set; } = false;
            /// <summary>
            /// Alpha map
            /// </summary>
            public string AlphaMap { get; set; } = null;
            /// <summary>
            /// Color textures for alpha map
            /// </summary>
            public string[] ColorTextures { get; set; } = null;

            /// <summary>
            /// Gets or sets if use slope texturing or not
            /// </summary>
            public bool UseSlopes { get; set; } = false;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges { get; set; } = Vector2.Zero;
            /// <summary>
            /// High resolution textures
            /// </summary>
            public string[] TexturesHR { get; set; } = null;
            /// <summary>
            /// Low resolution textures
            /// </summary>
            public string[] TexturesLR { get; set; } = null;

            /// <summary>
            /// Lerping proportion between alpha mapping and slope texturing
            /// </summary>
            public float Proportion { get; set; } = 0f;
            /// <summary>
            /// UV texture scale
            /// </summary>
            public float Scale { get; set; } = 1;
            /// <summary>
            /// UV texture displacement
            /// </summary>
            public Vector2 Displacement { get; set; } = Vector2.Zero;
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Height map
        /// </summary>
        public float[,] Heightmap { get; set; } = null;
        /// <summary>
        /// Height map file name
        /// </summary>
        public string HeightmapFileName { get; set; } = null;
        /// <summary>
        /// Color map
        /// </summary>
        public Color4[,] Colormap { get; set; } = null;
        /// <summary>
        /// Color map file name
        /// </summary>
        public string ColormapFileName { get; set; } = null;
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { get; set; } = 1;
        /// <summary>
        /// Maximum height
        /// </summary>
        public float MaximumHeight { get; set; } = 1;
        /// <summary>
        /// Height curve
        /// </summary>
        public Curve HeightCurve { get; set; }
        /// <summary>
        /// Texture resolution
        /// </summary>
        public float TextureResolution { get; set; } = 10;
        /// <summary>
        /// Textures
        /// </summary>
        public TexturesDescription Textures { get; set; } = new TexturesDescription();
        /// <summary>
        /// Terrain material
        /// </summary>
        public MaterialDescription Material { get; set; } = new MaterialDescription();
        /// <summary>
        /// Use falloff map
        /// </summary>
        public bool UseFalloff { get; set; } = false;
        /// <summary>
        /// Falloff curve params
        /// </summary>
        /// <remarks>
        /// From https://www.youtube.com/watch?v=COmtTyLCd6I
        /// f(x) = (x pow a) / ((x pow a) + ((b - bx) pow a))
        /// Where a = FalloffCurve.X and b = FalloffCurve.Y
        /// </remarks>
        public Vector2 FalloffCurve { get; set; } = new Vector2(2, 2.7f);

        /// <summary>
        /// Constructor
        /// </summary>
        public HeightmapDescription()
        {
            HeightCurve = new Curve();
            HeightCurve.Keys.Add(new CurveKey(0f, 0f));
            HeightCurve.Keys.Add(new CurveKey(1f, 1f));
        }

        /// <summary>
        /// Generates a new model content from an height map description
        /// </summary>
        /// <returns>Returns a new model content</returns>
        public ModelContent ReadModelContent()
        {
            HeightMap hm;

            if (Heightmap != null)
            {
                hm = HeightMap.FromMap(Heightmap, Colormap, UseFalloff, FalloffCurve.X, FalloffCurve.Y);
            }
            else if (!string.IsNullOrEmpty(HeightmapFileName))
            {
                ImageContent heightMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(ContentPath, HeightmapFileName),
                };

                hm = HeightMap.FromStream(heightMapImage.Stream, null, UseFalloff, FalloffCurve.X, FalloffCurve.Y);
            }
            else
            {
                throw new EngineException("No heightmap found in description.");
            }

            ModelContent modelContent = new ModelContent();

            string materialName = "material";
            string geoName = "geometry";

            MaterialContent material = MaterialContent.Default;

            hm.BuildGeometry(
                CellSize,
                MaximumHeight,
                HeightCurve,
                Textures.Scale,
                Textures.Displacement,
                out var vertices, out var indices);

            SubMeshContent geo = new SubMeshContent(Topology.TriangleList, materialName, true, false);
            geo.SetVertices(vertices);
            geo.SetIndices(indices);

            if (Textures?.TexturesLR?.Any() == true)
            {
                string diffuseTexureName = "diffuse";

                material.DiffuseTexture = diffuseTexureName;

                ImageContent diffuseImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(ContentPath, Textures.TexturesLR),
                };

                modelContent.Images.Add(diffuseTexureName, diffuseImage);
            }

            if (Textures?.NormalMaps?.Any() == true)
            {
                string nmapTexureName = "normal";

                material.NormalMapTexture = nmapTexureName;

                ImageContent nmapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(ContentPath, Textures.NormalMaps),
                };

                modelContent.Images.Add(nmapTexureName, nmapImage);
            }

            modelContent.Materials.Add(materialName, material);
            modelContent.Geometry.Add(geoName, materialName, geo);

            return modelContent;
        }
    }
}
