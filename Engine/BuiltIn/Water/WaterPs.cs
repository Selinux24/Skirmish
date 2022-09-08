using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Water
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Water pixel shader
    /// </summary>
    public class WaterPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per water constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerWater;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public WaterPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(WaterPs), "main", ShaderDefaultBasicResources.Water_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~WaterPs()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
        /// Sets per water constant buffer
        /// </summary>
        public void SetPerWaterConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerWater = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                BuiltInShaders.GetDirectionalsConstantBuffer(),
                cbPerWater,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);
        }
    }
}
