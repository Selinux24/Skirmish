using SharpDX;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
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
        /// <param name="textures">Texture description</param>
        public static HeightmapDescription FromMap(NoiseMap heightmap, float cellsize, float maximumHeight, Curve heightCurve, HeightmapTexturesDescription textures)
        {
            return new HeightmapDescription
            {
                Heightmap = heightmap.Map,
                CellSize = cellsize,
                MaximumHeight = maximumHeight,
                HeightCurve = heightCurve,
                Textures = textures,
            };
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
        /// Textures
        /// </summary>
        public HeightmapTexturesDescription Textures { get; set; } = new HeightmapTexturesDescription();
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
        /// Transform matrix
        /// </summary>
        public Matrix Transform { get; set; } = Matrix.Identity;

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
        /// Generates a new content data from an height map description
        /// </summary>
        /// <returns>Returns a new model content</returns>
        public async Task<ContentData> ReadContentData()
        {
            var hm = HeightMap.FromDescription(this);
            var (Vertices, Indices) = await hm.BuildGeometry(
                CellSize,
                MaximumHeight,
                HeightCurve,
                Textures.Scale,
                Textures.Displacement);

            var modelContent = new ContentData();

            await Task.Run(() =>
            {
                var vertices = Vertices;
                var indices = Indices;

                if (!Transform.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, Transform);
                }

                string materialName = "material";
                string geoName = "geometry";

                var material = MaterialBlinnPhongContent.Default;

                var geo = new SubMeshContent(Topology.TriangleList, materialName, true, false, Matrix.Identity);
                geo.SetVertices(vertices);
                geo.SetIndices(indices);

                if ((Textures?.TexturesLR?.Length ?? 0) != 0)
                {
                    string diffuseTexureName = "diffuse";

                    material.DiffuseTexture = diffuseTexureName;

                    var diffuseImage = new FileArrayImageContent(Textures.ContentPath, Textures.TexturesLR);

                    modelContent.AddTextureContent(diffuseTexureName, diffuseImage);
                }

                if ((Textures?.NormalMaps?.Length ?? 0) != 0)
                {
                    string nmapTexureName = "normal";

                    material.NormalMapTexture = nmapTexureName;

                    var nmapImage = new FileArrayImageContent(Textures.ContentPath, Textures.NormalMaps);

                    modelContent.AddTextureContent(nmapTexureName, nmapImage);
                }

                modelContent.AddMaterialContent(materialName, material);
                modelContent.ImportMaterial(geoName, materialName, geo);
            });

            return modelContent;
        }
    }
}
