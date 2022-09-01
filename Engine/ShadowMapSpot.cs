using SharpDX;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.ShadowSpots;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Spot shadow map
    /// </summary>
    public class ShadowMapSpot : ShadowMap
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        public ShadowMapSpot(Scene scene, string name, int width, int height, int arraySize) : base(scene, name, width, height, arraySize)
        {
            var (DepthStencils, ShaderResource) = scene.Game.Graphics.CreateShadowMapTextureArrays(name, width, height, 1, arraySize);

            DepthMap = DepthStencils;
            Texture = ShaderResource;
        }

        /// <inheritdoc/>
        public override void UpdateFromLightViewProjection(Camera camera, ISceneLight light)
        {
            if (light is ISceneLightSpot lightSpot)
            {
                var near = 1f;
                var projection = Matrix.PerspectiveFovLH(lightSpot.FallOffAngleRadians * 2f, 1f, near, lightSpot.Radius);

                var pos = lightSpot.Position;
                var look = lightSpot.Position + (lightSpot.Direction * lightSpot.Radius);
                var view = Matrix.LookAtLH(pos, look, Vector3.Up);

                var vp = view * projection;

                ToShadowMatrix = vp;
                LightPosition = lightSpot.Position;
                FromLightViewProjectionArray = new[] { vp };
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

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapSpot)} - LightPosition: {LightPosition} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
