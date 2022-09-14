
namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Shadows;
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
            bool skinned = VertexData.IsSkinned(vertexType);

            if (instanced)
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInCascadedPositionSkinnedInstanced>();
                }
                else
                {
                    return BuiltInShaders.GetDrawer<BuiltInCascadedPositionInstanced>();
                }
            }
            else
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInCascadedPositionSkinned>();
                }
                else
                {
                    return BuiltInShaders.GetDrawer<BuiltInCascadedPosition>();
                }
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
