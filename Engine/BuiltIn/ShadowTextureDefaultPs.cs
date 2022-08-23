using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Shadow transparent texture pixel shader
    /// </summary>
    public class ShadowTextureDefaultPs : IBuiltInPixelShader
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
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public ShadowTextureDefaultPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_ShadowTextureDefault_Cso == null;
            var bytes = Resources.Ps_ShadowTextureDefault_Cso ?? Resources.Ps_ShadowTextureDefault;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(ShadowTextureDefaultPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(ShadowTextureDefaultPs), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowTextureDefaultPs()
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
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var rv = new[]
            {
                diffuseMapArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
