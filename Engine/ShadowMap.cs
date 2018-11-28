using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Shadow map
    /// </summary>
    public abstract class ShadowMap : IShadowMap
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
        /// To shadow view*projection matrix
        /// </summary>
        public Matrix ToShadowMatrix { get; set; } = Matrix.Identity;
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 LightPosition { get; set; } = Vector3.Zero;
        /// <summary>
        /// From light view projection
        /// </summary>
        public Matrix[] FromLightViewProjectionArray { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="arraySize">Array size</param>
        protected ShadowMap(Game game, int width, int height, int arraySize)
        {
            this.Game = game;

            this.Viewports = Helper.CreateArray(arraySize, new Viewport(0, 0, width, height, 0, 1.0f));

            this.FromLightViewProjectionArray = Helper.CreateArray(arraySize, Matrix.Identity);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowMap()
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
                for (int i = 0; i < this.DepthMap?.Length; i++)
                {
                    this.DepthMap[i]?.Dispose();
                    this.DepthMap[i] = null;
                }
                this.DepthMap = null;

                this.Texture?.Dispose();
                this.Texture = null;
            }
        }

        /// <summary>
        /// Updates the from light view projection
        /// </summary>
        public abstract void UpdateFromLightViewProjection(Camera camera, ISceneLight light);
        /// <summary>
        /// Gets the effect to draw this shadow map
        /// </summary>
        /// <returns>Returns an effect</returns>
        public abstract IShadowMapDrawer GetEffect();

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
    }
}
