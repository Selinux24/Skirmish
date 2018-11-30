using SharpDX;

namespace Engine
{
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
        /// <param name="game">Game</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        public ShadowMapSpot(Game game, int width, int height, int arraySize) : base(game, width, height, arraySize)
        {
            game.Graphics.CreateShadowMapTextureArrays(
                width, height, 1, arraySize,
                out EngineDepthStencilView[] dsv, out EngineShaderResourceView srv);

            this.DepthMap = dsv;
            this.Texture = srv;
        }

        /// <summary>
        /// Updates the from light view projection
        /// </summary>
        public override void UpdateFromLightViewProjection(Camera camera, ISceneLight light)
        {
            if (light is ISceneLightSpot lightSpot)
            {
                var near = 1f;
                var projection = Matrix.PerspectiveFovLH(lightSpot.AngleRadians * 2f, 1f, near, lightSpot.Radius);

                var pos = lightSpot.Position;
                var look = lightSpot.Position + (lightSpot.Direction * lightSpot.Radius);
                var view = Matrix.LookAtLH(pos, look, Vector3.Up);

                var vp = view * projection;

                this.ToShadowMatrix = vp;
                this.LightPosition = lightSpot.Position;
                this.FromLightViewProjectionArray = new[] { vp };
            }
        }
        /// <summary>
        /// Gets the effect to draw this shadow map
        /// </summary>
        /// <returns>Returns an effect</returns>
        public override IShadowMapDrawer GetEffect()
        {
            return DrawerPool.EffectShadowBasic;
        }
    }
}
