using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Shadow map
    /// </summary>
    public class ShadowMapCascade : IShadowMap
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
        /// Cascade matrix set
        /// </summary>
        protected ShadowMapCascadeSet MatrixSet { get; set; }

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
        /// <param name="size">Map size</param>
        /// <param name="mapCount">Map count</param>
        /// <param name="cascades">Cascade far clip distances</param>
        public ShadowMapCascade(Game game, int size, int mapCount, int arraySize, float[] cascades)
        {
            this.Game = game;

            this.Viewports = Helper.CreateArray(cascades.Length, new Viewport(0, 0, size, size, 0, 1.0f));

            game.Graphics.CreateShadowMapTextureArrays(
                size, size, mapCount, arraySize,
                out EngineDepthStencilView[] dsv,
                out EngineShaderResourceView srv);

            this.DepthMap = dsv;
            this.Texture = srv;

            this.FromLightViewProjectionArray = Helper.CreateArray(cascades.Length, Matrix.Identity);

            this.MatrixSet = new ShadowMapCascadeSet(size, 1, cascades);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowMapCascade()
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
            if (light is ISceneLightDirectional lightDirectional)
            {
                this.MatrixSet.Update(camera, lightDirectional.Direction);

                this.FromLightViewProjectionArray = this.MatrixSet.GetWorldToCascadeProj();
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
            return DrawerPool.EffectShadowCascade;
        }
    }
}
