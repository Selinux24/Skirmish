using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Shadow map
    /// </summary>
    public class ShadowMapSpot : IShadowMap
    {
        /// <summary>
        /// Game instance
        /// </summary>
        protected Game Game { get; private set; }
        /// <summary>
        /// Viewport
        /// </summary>
        protected Viewport Viewport { get; set; }
        /// <summary>
        /// Depth map
        /// </summary>
        protected EngineDepthStencilView[] DepthMap { get; set; }

        /// <summary>
        /// Deph map texture
        /// </summary>
        public EngineShaderResourceView Texture { get; protected set; }
        /// <summary>
        /// From light view projection
        /// </summary>
        public Matrix[] FromLightViewProjectionArray { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        public ShadowMapSpot(Game game, int width, int height, int arraySize)
        {
            this.Game = game;

            this.Viewport = new Viewport(0, 0, width, height, 0, 1.0f);

            game.Graphics.CreateShadowMapTextures(
                width, height, arraySize,
                out EngineDepthStencilView[] dsv, out EngineShaderResourceView srv);

            this.DepthMap = dsv;
            this.Texture = srv;

            this.FromLightViewProjectionArray = Helper.CreateArray(arraySize, Matrix.Identity);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowMapSpot()
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
            if (light is ISceneLightSpot lightSpot)
            {
                var projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1f, 1f, lightSpot.Radius);

                // View from light to scene center position
                var view = Matrix.LookAtLH(lightSpot.Position, lightSpot.Position + lightSpot.Direction, Vector3.Up);

                FromLightViewProjectionArray = new[] { view * projection };
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
            graphics.SetViewport(this.Viewport);

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
            return DrawerPool.EffectShadowBasic;
        }
    }
}
