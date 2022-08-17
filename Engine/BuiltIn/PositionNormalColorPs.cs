using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal color pixel shader
    /// </summary>
    public class PositionNormalColorPs : IDisposable
    {
        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

        /// <summary>
        /// Directional shadow map resource view
        /// </summary>
        private EngineShaderResourceView shadowMapDir;
        /// <summary>
        /// Spot shadow map resource view
        /// </summary>
        private EngineShaderResourceView shadowMapSpot;
        /// <summary>
        /// Point shadow map resource view
        /// </summary>
        private EngineShaderResourceView shadowMapPoint;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalColorPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalColor_Cso == null;
            var bytes = Resources.Ps_PositionNormalColor_Cso ?? Resources.Ps_PositionNormalColor;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionNormalColorPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionNormalColorPs), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalColorPs()
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
                Shader?.Dispose();
            }
        }

        /// <summary>
        /// Sets the directional shadow map array
        /// </summary>
        /// <param name="shadowMapDir">Directional shadow map array</param>
        public void SetDirShadowMap(EngineShaderResourceView shadowMapDir)
        {
            this.shadowMapDir = shadowMapDir;
        }
        /// <summary>
        /// Sets the spot shadow map array
        /// </summary>
        /// <param name="shadowMapSpot">Spot shadow map array</param>
        public void SetSpotShadowMap(EngineShaderResourceView shadowMapSpot)
        {
            this.shadowMapSpot = shadowMapSpot;
        }
        /// <summary>
        /// Sets the point shadow map array
        /// </summary>
        /// <param name="shadowMapPoint">Point shadow map array</param>
        public void SetPointShadowMap(EngineShaderResourceView shadowMapPoint)
        {
            this.shadowMapPoint = shadowMapPoint;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetPixelShaderConstantBuffer(0, BuiltInShaders.GetVSPerFrame());

            var rv = new[]
            {
                shadowMapDir,
                shadowMapSpot,
                shadowMapPoint,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
