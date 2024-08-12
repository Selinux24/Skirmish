using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Common;
using Engine.BuiltIn.Drawers.Deferred;
using Engine.BuiltIn.Format;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Ground
{
    /// <summary>
    /// Terrain class
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class Terrain(Scene scene, string id, string name) : Ground<TerrainDescription>(scene, id, name), IUseMaterials
    {
        /// <summary>
        /// Grid
        /// </summary>
        private TerrainGrid mapGrid = null;
        /// <summary>
        /// Height map
        /// </summary>
        private HeightMap heightMap = null;

        /// <summary>
        /// Height-map texture resolution
        /// </summary>
        private float textureResolution;
        /// <summary>
        /// Terrain material
        /// </summary>
        private IMeshMaterial terrainMaterial;
        /// <summary>
        /// Gets or sets the terrain drawing mode
        /// </summary>
        private BuiltInTerrainModes terrainMode;
        /// <summary>
        /// Lerp proportion between alpha mapping and slope texturing
        /// </summary>
        private float proportion;
        /// <summary>
        /// Terrain low res textures
        /// </summary>
        private EngineShaderResourceView terrainTexturesLR = null;
        /// <summary>
        /// Terrain high res textures
        /// </summary>
        private EngineShaderResourceView terrainTexturesHR = null;
        /// <summary>
        /// Terrain normal maps
        /// </summary>
        private EngineShaderResourceView terrainNormalMaps = null;
        /// <summary>
        /// Color textures for alpha map
        /// </summary>
        private EngineShaderResourceView colorTextures = null;
        /// <summary>
        /// Alpha map
        /// </summary>
        private EngineShaderResourceView alphaMap = null;
        /// <summary>
        /// Use anisotropic
        /// </summary>
        private bool useAnisotropic = false;
        /// <summary>
        /// Slope ranges
        /// </summary>
        private Vector2 slopeRanges = Vector2.Zero;

        /// <summary>
        /// Destructor
        /// </summary>
        ~Terrain()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mapGrid?.Dispose();
                mapGrid = null;

                heightMap?.Dispose();
                heightMap = null;

                terrainTexturesLR?.Dispose();
                terrainTexturesLR = null;
                terrainTexturesHR?.Dispose();
                terrainTexturesHR = null;
                terrainNormalMaps?.Dispose();
                terrainNormalMaps = null;
                colorTextures?.Dispose();
                colorTextures = null;
                alphaMap?.Dispose();
                alphaMap = null;
            }
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(TerrainDescription description)
        {
            await base.ReadAssets(description);

            if (Description.Heightmap == null)
            {
                throw new EngineException($"Terrain initialization error. Height-map description not found.");
            }

            useAnisotropic = Description.UseAnisotropic;

            // Read height-map
            heightMap = HeightMap.FromDescription(Description.Heightmap);
            float heightMapCellSize = Description.Heightmap.CellSize;
            float heightMapHeight = Description.Heightmap.MaximumHeight;
            Curve heightMapCurve = Description.Heightmap.HeightCurve;
            float uvScale = 1;
            Vector2 uvDisplacement = Vector2.Zero;

            if (Description.Heightmap.Textures != null)
            {
                // Read texture data
                uvScale = Description.Heightmap.Textures.Scale;
                uvDisplacement = Description.Heightmap.Textures.Displacement;
                proportion = Description.Heightmap.Textures.Proportion;
                textureResolution = Description.Heightmap.Textures.Resolution;
                slopeRanges = Description.Heightmap.Textures.SlopeRanges;

                bool useAlphaMap = Description.Heightmap.Textures.UseAlphaMapping;
                bool useSlopes = Description.Heightmap.Textures.UseSlopes;
                terrainMode = BuiltInTerrainModes.AlphaMap;
                if (useAlphaMap && useSlopes) { terrainMode = BuiltInTerrainModes.Full; }
                if (useSlopes) { terrainMode = BuiltInTerrainModes.Slopes; }

                ReadHeightmapTextures(Description.Heightmap.ContentPath, Description.Heightmap.Textures);
            }

            // Read material
            terrainMaterial = MeshMaterial.FromMaterial(MaterialBlinnPhong.Default);

            // Get vertices and indices from height-map
            var (Vertices, Indices) = await heightMap.BuildGeometry(
                heightMapCellSize,
                heightMapHeight,
                heightMapCurve,
                uvScale,
                uvDisplacement);

            // Compute triangles for ray - mesh picking
            var tris = Triangle.ComputeTriangleList(
                Vertices.Select(v => v.Position.Value).ToArray(),
                Indices.ToArray());

            // Initialize quad-tree for ray picking
            GroundPickingQuadtree = Description.ReadQuadTree(tris);

            //Initialize map
            int trianglesPerNode = heightMap.CalcTrianglesPerNode(TerrainGrid.LODLevels);
            mapGrid = await TerrainGrid.Create(Game, $"Terrain.{Name}", Vertices, trianglesPerNode);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            mapGrid?.Update(Scene.Camera.Position);
        }
        /// <inheritdoc/>
        public override bool DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return false;
            }

            if (mapGrid == null)
            {
                return false;
            }

            var shadowDrawer = context.ShadowMap?.GetDrawer<VertexTerrain>(false, false);
            if (shadowDrawer == null)
            {
                return false;
            }

            shadowDrawer.UpdateCastingLight(context);

            var meshState = new BuiltInDrawerMeshState
            {
                Local = Matrix.Identity,
            };
            shadowDrawer.UpdateMesh(context.DeviceContext, meshState);

            return mapGrid.DrawShadows(context, shadowDrawer);
        }
        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (mapGrid == null)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var terrainDrawer = GetDrawer(context);
            if (terrainDrawer == null)
            {
                return false;
            }

            return mapGrid.Draw(context, terrainDrawer);
        }
        /// <summary>
        /// Gets the terrain drawer, based on the drawing context
        /// </summary>
        /// <param name="context">Drawing context</param>
        private IDrawer GetDrawer(DrawContext context)
        {
            if (context.DrawerMode.HasFlag(DrawerModes.Forward))
            {
                var dr = BuiltInShaders.GetDrawer<Drawers.Forward.BuiltInTerrain>();
                dr.Update(context.DeviceContext, GetTerrainState());
                return dr;
            }

            if (context.DrawerMode.HasFlag(DrawerModes.Deferred))
            {
                var dr = BuiltInShaders.GetDrawer<BuiltInTerrain>();
                dr.Update(context.DeviceContext, GetTerrainState());
                return dr;
            }

            return null;
        }
        /// <summary>
        /// Gets the terrain state
        /// </summary>
        private BuiltInTerrainState GetTerrainState()
        {
            return new BuiltInTerrainState
            {
                TintColor = Color.White,
                MaterialIndex = terrainMaterial.ResourceIndex,
                Mode = terrainMode,
                TextureResolution = textureResolution,
                Proportion = proportion,
                SlopeRanges = slopeRanges,
                AlphaMap = alphaMap,
                MormalMap = terrainNormalMaps,
                ColorTexture = colorTextures,
                LowResolutionTexture = terrainTexturesLR,
                HighResolutionTexture = terrainTexturesHR,
                UseAnisotropic = useAnisotropic,
            };
        }

        /// <summary>
        /// Reads texture data
        /// </summary>
        /// <param name="baseContentPath">Base content path</param>
        /// <param name="description">Textures description</param>
        private void ReadHeightmapTextures(string baseContentPath, HeightmapTexturesDescription description)
        {
            string tContentPath = Path.Combine(baseContentPath, description.ContentPath);

            var normalMapTextures = new FileArrayImageContent(tContentPath, description.NormalMaps);
            terrainNormalMaps = Game.ResourceManager.RequestResource(normalMapTextures);

            if (description.UseSlopes)
            {
                var texturesLR = new FileArrayImageContent(tContentPath, description.TexturesLR);
                var texturesHR = new FileArrayImageContent(tContentPath, description.TexturesHR);

                terrainTexturesLR = Game.ResourceManager.RequestResource(texturesLR);
                terrainTexturesHR = Game.ResourceManager.RequestResource(texturesHR);
            }

            if (description.UseAlphaMapping)
            {
                var colors = new FileArrayImageContent(tContentPath, description.ColorTextures);
                var aMap = new FileArrayImageContent(tContentPath, description.AlphaMap);

                colorTextures = Game.ResourceManager.RequestResource(colors);
                alphaMap = Game.ResourceManager.RequestResource(aMap);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return terrainMaterial != null ? [terrainMaterial] : [];
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            return terrainMaterial;
        }
        /// <inheritdoc/>
        public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            if (terrainMaterial == material)
            {
                return false;
            }

            terrainMaterial = material;

            return true;
        }
    }
}
