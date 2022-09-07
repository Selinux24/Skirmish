
namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.ShadowCascade;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Cascaded shadow map
    /// </summary>
    public class ShadowMapCascade : ShadowMap
    {
        /// <summary>
        /// Map size
        /// </summary>
        protected int Size { get; private set; }
        /// <summary>
        /// Cascade matrix set
        /// </summary>
        protected ShadowMapCascadeSet MatrixSet { get; set; }

        /// <inheritdoc/>
        public override bool HighResolutionMap
        {
            get
            {
                return base.HighResolutionMap;
            }
            set
            {
                base.HighResolutionMap = value;

                if (MatrixSet != null)
                {
                    MatrixSet.AntiFlicker = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="size">Map size</param>
        /// <param name="mapCount">Map count</param>
        /// <param name="cascades">Cascade far clip distances</param>
        public ShadowMapCascade(Scene scene, string name, int size, int mapCount, int arraySize, float[] cascades) : base(scene, name, size, size, cascades.Length)
        {
            Size = size;

            var (DepthStencils, ShaderResource) = scene.Game.Graphics.CreateShadowMapTextureArrays(name, size, size, mapCount, arraySize);

            DepthMap = DepthStencils;
            Texture = ShaderResource;

            MatrixSet = new ShadowMapCascadeSet(size, 1, cascades);
        }

        /// <inheritdoc/>
        public override void UpdateFromLightViewProjection(Camera camera, ISceneLight light)
        {
            if (light is ISceneLightDirectional lightDirectional)
            {
                MatrixSet.Update(camera, lightDirectional.Direction);

                lightDirectional.ToShadowSpace = MatrixSet.GetWorldToShadowSpace();
                lightDirectional.ToCascadeOffsetX = MatrixSet.GetToCascadeOffsetX();
                lightDirectional.ToCascadeOffsetY = MatrixSet.GetToCascadeOffsetY();
                lightDirectional.ToCascadeScale = MatrixSet.GetToCascadeScale();

                var vp = MatrixSet.GetWorldToCascadeProj();

                ToShadowMatrix = vp[0];
                LightPosition = MatrixSet.GetLigthPosition();
                FromLightViewProjectionArray = vp;
            }
        }
        /// <inheritdoc/>
        public override IShadowMapDrawer GetEffect()
        {
            return null;
        }
        /// <inheritdoc/>
        public override IBuiltInDrawer GetDrawer(VertexTypes vertexType, bool instanced, bool useTextureAlpha)
        {
            if (instanced)
            {
                return GetDrawerInstanced(vertexType);
            }

            return GetDrawerSingle(vertexType);
        }
        private IBuiltInDrawer GetDrawerSingle(VertexTypes vertexType)
        {
            switch (vertexType)
            {
                case VertexTypes.Unknown:
                    return null;
                case VertexTypes.Billboard:
                    return null;
                case VertexTypes.Font:
                    return null;
                case VertexTypes.CPUParticle:
                    return null;
                case VertexTypes.GPUParticle:
                    return null;
                case VertexTypes.Terrain:
                    return null;
                case VertexTypes.Decal:
                    return null;
                case VertexTypes.Position:
                    return null;
                case VertexTypes.PositionColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColor>();
                case VertexTypes.PositionTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTexture>();
                case VertexTypes.PositionNormalColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColor>();
                case VertexTypes.PositionNormalTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTexture>();
                case VertexTypes.PositionNormalTextureTangent:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangent>();
                case VertexTypes.PositionSkinned:
                    return null;
                case VertexTypes.PositionColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColorSkinned>();
                case VertexTypes.PositionTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinned>();
                case VertexTypes.PositionNormalColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinned>();
                case VertexTypes.PositionNormalTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinned>();
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinned>();
                default:
                    return null;
            }
        }
        private IBuiltInDrawer GetDrawerInstanced(VertexTypes vertexType)
        {
            switch (vertexType)
            {
                case VertexTypes.Unknown:
                    return null;
                case VertexTypes.Billboard:
                    return null;
                case VertexTypes.Font:
                    return null;
                case VertexTypes.CPUParticle:
                    return null;
                case VertexTypes.GPUParticle:
                    return null;
                case VertexTypes.Terrain:
                    return null;
                case VertexTypes.Decal:
                    return null;
                case VertexTypes.Position:
                    return null;
                case VertexTypes.PositionColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColorInstanced>();
                case VertexTypes.PositionTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTextureInstanced>();
                case VertexTypes.PositionNormalColor:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorInstanced>();
                case VertexTypes.PositionNormalTexture:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureInstanced>();
                case VertexTypes.PositionNormalTextureTangent:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentInstanced>();
                case VertexTypes.PositionSkinned:
                    return null;
                case VertexTypes.PositionColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionColorSkinnedInstanced>();
                case VertexTypes.PositionTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionTextureSkinnedInstanced>();
                case VertexTypes.PositionNormalColorSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalColorSkinnedInstanced>();
                case VertexTypes.PositionNormalTextureSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureSkinnedInstanced>();
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return BuiltInShaders.GetDrawer<BuiltInPositionNormalTextureTangentSkinnedInstanced>();
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public override void UpdateGlobals()
        {
            MatrixSet = new ShadowMapCascadeSet(Size, 1, Scene.GameEnvironment.CascadeShadowMapsDistances)
            {
                AntiFlicker = HighResolutionMap
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapCascade)} - LightPosition: {LightPosition} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
