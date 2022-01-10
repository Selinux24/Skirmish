using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Scene
        /// </summary>
        protected Scene Scene { get; private set; }
        /// <summary>
        /// Viewport
        /// </summary>
        protected Viewport[] Viewports { get; set; }
        /// <summary>
        /// Depth map
        /// </summary>
        protected IEnumerable<EngineDepthStencilView> DepthMap { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; protected set; }
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
        /// Gets or sets the high resolution map flag (if available)
        /// </summary>
        public virtual bool HighResolutionMap { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        protected ShadowMap(Scene scene, string name, int width, int height, int arraySize)
        {
            Scene = scene;
            Name = name;

            Viewports = Helper.CreateArray(arraySize, new Viewport(0, 0, width, height, 0, 1.0f));

            FromLightViewProjectionArray = Helper.CreateArray(arraySize, Matrix.Identity);
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
                if (DepthMap?.Any() == true)
                {
                    for (int i = 0; i < DepthMap.Count(); i++)
                    {
                        DepthMap.ElementAt(i)?.Dispose();
                    }
                }
                DepthMap = null;

                Texture?.Dispose();
                Texture = null;
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
        /// Update shadow map globals
        /// </summary>
        public abstract void UpdateGlobals();

        /// <summary>
        /// Binds the shadow map data to graphics
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="index">Array index</param>
        public void Bind(Graphics graphics, int index)
        {
            //Set shadow mapper viewport
            graphics.SetViewports(Viewports);

            //Set shadow map depth map without render target
            graphics.SetRenderTargets(
                null, false, Color.Transparent,
                DepthMap.ElementAtOrDefault(index), true, false,
                true);
        }
    }
}
