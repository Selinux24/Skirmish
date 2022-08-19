using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal texture tangent pixel shader
    /// </summary>
    public class PositionNormalTextureTangentPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Shader
        /// </summary>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;
        /// <summary>
        /// Normal map resource view
        /// </summary>
        private EngineShaderResourceView normalMapArray;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalTextureTangentPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalTextureTangent_Cso == null;
            var bytes = Resources.Ps_PositionNormalTextureTangent_Cso ?? Resources.Ps_PositionNormalTextureTangent;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionNormalTextureTangentPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionNormalTextureTangentPs), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTextureTangentPs()
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
                Shader = null;
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
        /// Sets the normal map array
        /// </summary>
        /// <param name="normalMapArray">Normal map array</param>
        public void SetNormalMap(EngineShaderResourceView normalMapArray)
        {
            this.normalMapArray = normalMapArray;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var cb = new[]
            {
                 BuiltInShaders.GetPSPerFrameLit(),
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetPSPerFrameLitShadowMapDir(),
                BuiltInShaders.GetPSPerFrameLitShadowMapSpot(),
                BuiltInShaders.GetPSPerFrameLitShadowMapPoint(),
                diffuseMapArray,
                normalMapArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
