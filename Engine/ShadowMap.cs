using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Shadow map
    /// </summary>
    public class ShadowMap : IShadowMap, IDisposable
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
        public ShadowMap(Game game, int width, int height, int arraySize)
        {
            this.Game = game;

            this.Viewport = new Viewport(0, 0, width, height, 0, 1.0f);

            EngineDepthStencilView[] dsv;
            EngineShaderResourceView srv;
            game.Graphics.CreateShadowMapTextures(width, height, arraySize, out dsv, out srv);
            this.DepthMap = dsv;
            this.Texture = srv;

            this.FromLightViewProjectionArray = Helper.CreateArray(arraySize, Matrix.Identity);
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.DepthMap);
            Helper.Dispose(this.Texture);
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
    }
}
