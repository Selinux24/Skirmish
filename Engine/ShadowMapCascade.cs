
namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Cascaded shadow map
    /// </summary>
    public class ShadowMapCascade : ShadowMap
    {
        /// <summary>
        /// Cascade matrix set
        /// </summary>
        protected ShadowMapCascadeSet MatrixSet { get; set; }
        
        /// <summary>
        /// Gets or sets the high resolution map flag
        /// </summary>
        /// <remarks>This property is directly mapped to the AntiFlicker matrix set property</remarks>
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
        /// <param name="game">Game</param>
        /// <param name="size">Map size</param>
        /// <param name="mapCount">Map count</param>
        /// <param name="cascades">Cascade far clip distances</param>
        public ShadowMapCascade(Game game, int size, int mapCount, int arraySize, float[] cascades) : base(game, size, size, cascades.Length)
        {
            game.Graphics.CreateShadowMapTextureArrays(
                size, size, mapCount, arraySize,
                out EngineDepthStencilView[] dsv,
                out EngineShaderResourceView srv);

            DepthMap = dsv;
            Texture = srv;

            MatrixSet = new ShadowMapCascadeSet(size, 1, cascades);
        }

        /// <summary>
        /// Updates the from light view projection
        /// </summary>
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
        /// <summary>
        /// Gets the effect to draw this shadow map
        /// </summary>
        /// <returns>Returns an effect</returns>
        public override IShadowMapDrawer GetEffect()
        {
            return DrawerPool.EffectShadowCascade;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapCascade)} - LightPosition: {LightPosition} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
