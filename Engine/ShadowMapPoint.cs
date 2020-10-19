using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Cubic shadow map
    /// </summary>
    public class ShadowMapPoint : ShadowMap
    {
        /// <summary>
        /// Gets from light view * projection matrix cube
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="radius">Light radius</param>
        /// <returns>Returns the from light view * projection matrix cube</returns>
        private static Matrix[] GetFromPointLightViewProjection(ISceneLightPoint light)
        {
            // Orthogonal projection from center
            var projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1f, 0.1f, light.Radius);

            return new Matrix[]
            {
                GetFromPointLightViewProjection(light.Position, Vector3.Right,      Vector3.Up)         * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.Left,       Vector3.Up)         * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.Up,         Vector3.BackwardLH) * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.Down,       Vector3.ForwardLH)  * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.ForwardLH,  Vector3.Up)         * projection,
                GetFromPointLightViewProjection(light.Position, Vector3.BackwardLH, Vector3.Up)         * projection,
            };
        }
        /// <summary>
        /// Gets the point light from light view matrix
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="direction">Direction</param>
        /// <param name="up">Up vector</param>
        /// <returns>Returns the point light from light view matrix</returns>
        private static Matrix GetFromPointLightViewProjection(Vector3 lightPosition, Vector3 direction, Vector3 up)
        {
            // View from light to scene center position
            return Matrix.LookAtLH(lightPosition, lightPosition + direction, up);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        public ShadowMapPoint(Game game, int width, int height, int arraySize) : base(game, width, height, 6)
        {
            game.Graphics.CreateCubicShadowMapTextureArrays(
                width, height, arraySize,
                out EngineDepthStencilView[] dsv, out EngineShaderResourceView srv);

            DepthMap = dsv;
            Texture = srv;
        }

        /// <summary>
        /// Updates the from light view projection
        /// </summary>
        public override void UpdateFromLightViewProjection(Camera camera, ISceneLight light)
        {
            if (light is ISceneLightPoint lightPoint)
            {
                var vp = GetFromPointLightViewProjection(lightPoint);

                ToShadowMatrix = vp[0];
                LightPosition = lightPoint.Position;
                FromLightViewProjectionArray = vp;
            }
        }
        /// <summary>
        /// Gets the effect to draw this shadow map
        /// </summary>
        /// <returns>Returns an effect</returns>
        public override IShadowMapDrawer GetEffect()
        {
            return DrawerPool.EffectShadowPoint;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapPoint)} - LightPosition: {LightPosition} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
