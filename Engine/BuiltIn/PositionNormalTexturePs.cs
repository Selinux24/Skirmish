using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal texture pixel shader
    /// </summary>
    public class PositionNormalTexturePs : IDisposable
    {
        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalTexture_Cso == null;
            var bytes = Resources.Ps_PositionNormalTexture_Cso ?? Resources.Ps_PositionNormalTexture;
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
        ~PositionNormalTexturePs()
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
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetPixelShaderConstantBuffer(0, BuiltInShaders.GetPSPerFrameNoLit());

            var rv = new[]
            {
                BuiltInShaders.GetPSPerFrameLitShadowMapDir(),
                BuiltInShaders.GetPSPerFrameLitShadowMapSpot(),
                BuiltInShaders.GetPSPerFrameLitShadowMapPoint(),
                diffuseMapArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
