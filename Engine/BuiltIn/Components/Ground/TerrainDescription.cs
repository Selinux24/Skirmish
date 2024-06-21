using Engine.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Ground
{
    /// <summary>
    /// Terrain description
    /// </summary>
    public class TerrainDescription : GroundDescription
    {
        /// <summary>
        /// Gets a ground description from data
        /// </summary>
        /// <param name="heightmap">Height map</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="maximumHeight">Maximum height</param>
        /// <param name="heightCurve">Height curve</param>
        /// <param name="textures">Heighmap textures</param>
        /// <param name="quadtreeDepth">Quadtree depth</param>
        public static TerrainDescription FromHeightmap(NoiseMap heightmap, float cellSize, float maximumHeight, Curve heightCurve, HeightmapTexturesDescription textures, int quadtreeDepth = 3)
        {
            return new()
            {
                Quadtree = QuadtreeDescription.Default(quadtreeDepth),
                Heightmap = HeightmapDescription.FromMap(heightmap, cellSize, maximumHeight, heightCurve, textures),
            };
        }
        /// <summary>
        /// Gets a ground description heightmap description
        /// </summary>
        /// <param name="description">Heightmap description</param>
        /// <param name="quadtreeDepth">Quadtree depth</param>
        public static TerrainDescription FromHeightmapDescription(HeightmapDescription description, int quadtreeDepth = 3)
        {
            return new()
            {
                Quadtree = QuadtreeDescription.Default(quadtreeDepth),
                Heightmap = description,
            };
        }
        /// <summary>
        /// Gets a ground description from a file
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        /// <param name="quadtreeDepth">Quadtree depth</param>
        public static TerrainDescription FromFile(string contentFolder, string fileName, int quadtreeDepth = 3)
        {
            return new()
            {
                Quadtree = QuadtreeDescription.Default(quadtreeDepth),
                Content = ContentDescription.FromFile(contentFolder, fileName),
            };
        }

        /// <summary>
        /// Heightmap description
        /// </summary>
        public HeightmapDescription Heightmap { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TerrainDescription()
            : base()
        {

        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ContentData>> ReadContentData()
        {
            if (Heightmap != null)
            {
                return [await Heightmap.ReadContentData()];
            }
            else
            {
                return await base.ReadContentData();
            }
        }
    }
}
