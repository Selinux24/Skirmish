using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Cubic shadow map
    /// </summary>
    public class ShadowMapPoint : IShadowMap
    {
        /// <summary>
        /// Game instance
        /// </summary>
        protected Game Game { get; private set; }
        /// <summary>
        /// Viewport
        /// </summary>
        protected Viewport[] Viewports { get; set; }
        /// <summary>
        /// Depth map
        /// </summary>
        protected EngineDepthStencilView[] DepthMap { get; set; }

        /// <summary>
        /// Cube deph map texture
        /// </summary>
        public EngineShaderResourceView Texture { get; protected set; }
        /// <summary>
        /// From light view projection
        /// </summary>
        public Matrix[] FromLightViewProjectionArray { get; set; }

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
        public ShadowMapPoint(Game game, int width, int height, int arraySize)
        {
            this.Game = game;

            this.Viewports = Helper.CreateArray(6, new Viewport(0, 0, width, height, 0, 1.0f));

            game.Graphics.CreateCubicShadowMapTextures(
                width, height, arraySize,
                out EngineDepthStencilView[] dsv, out EngineShaderResourceView srv);

            this.DepthMap = dsv;
            this.Texture = srv;

            this.FromLightViewProjectionArray = Helper.CreateArray(6, Matrix.Identity);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowMapPoint()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.DepthMap != null)
                {
                    for (int i = 0; i < this.DepthMap.Length; i++)
                    {
                        this.DepthMap[i]?.Dispose();
                        this.DepthMap[i] = null;
                    }

                    this.DepthMap = null;
                }

                if (this.Texture != null)
                {
                    this.Texture.Dispose();
                    this.Texture = null;
                }
            }
        }

        /// <summary>
        /// Updates the from light view projection
        /// </summary>
        public void UpdateFromLightViewProjection(Camera camera, ISceneLight light)
        {
            if (light is ISceneLightPoint lightPoint)
            {
                FromLightViewProjectionArray = GetFromPointLightViewProjection(lightPoint);
            }
        }
        /// <summary>
        /// Binds the shadow map data to graphics
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="index">Array index</param>
        public void Bind(Graphics graphics, int index)
        {
            //Set shadow mapper viewport
            graphics.SetViewports(this.Viewports);

            //Set shadow map depth map without render target
            graphics.SetRenderTargets(
                null, false, Color.Transparent,
                this.DepthMap[index], true, false,
                true);
        }
        /// <summary>
        /// Gets the effect to draw this shadow map
        /// </summary>
        /// <returns>Returns an effect</returns>
        public IShadowMapDrawer GetEffect()
        {
            return DrawerPool.EffectShadowPoint;
        }
    }
}
