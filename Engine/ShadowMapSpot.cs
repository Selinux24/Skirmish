using SharpDX;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltInEffects;
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
            return DrawerPool.EffectShadowBasic;
        }
        /// <inheritdoc/>
        public override IBuiltInDrawer GetDrawer(VertexTypes vertexType, bool useTextureAlpha)
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
                    return BuiltInShaders.GetDrawer<ShadowPositionColor>();
                case VertexTypes.PositionTexture:
                    return BuiltInShaders.GetDrawer<ShadowPositionTexture>();
                case VertexTypes.PositionNormalColor:
                    return BuiltInShaders.GetDrawer<ShadowPositionNormalColor>();
                case VertexTypes.PositionNormalTexture:
                    return BuiltInShaders.GetDrawer<ShadowPositionNormalTexture>();
                case VertexTypes.PositionNormalTextureTangent:
                    return BuiltInShaders.GetDrawer<ShadowPositionNormalTextureTangent>();
                case VertexTypes.PositionSkinned:
                    return null;
                case VertexTypes.PositionColorSkinned:
                    return BuiltInShaders.GetDrawer<ShadowPositionColorSkinned>();
                case VertexTypes.PositionTextureSkinned:
                    return BuiltInShaders.GetDrawer<ShadowPositionTextureSkinned>();
                case VertexTypes.PositionNormalColorSkinned:
                    return BuiltInShaders.GetDrawer<ShadowPositionNormalColorSkinned>();
                case VertexTypes.PositionNormalTextureSkinned:
                    return BuiltInShaders.GetDrawer<ShadowPositionNormalTextureSkinned>();
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return BuiltInShaders.GetDrawer<ShadowPositionNormalTextureTangentSkinned>();
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
