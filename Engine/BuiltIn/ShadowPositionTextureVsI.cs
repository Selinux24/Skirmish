using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position texture instanced vertex shader
    /// </summary>
    public class ShadowPositionTextureVsI : IBuiltInVertexShader
    {
        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Shader
        /// </summary>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public ShadowPositionTextureVsI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_ShadowPositionTexture_I_Cso == null;
            var bytes = Resources.Vs_ShadowPositionTexture_I_Cso ?? Resources.Vs_ShadowPositionTexture_I;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(ShadowPositionTextureVsI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(ShadowPositionTextureVsI), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowPositionTextureVsI()
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
        /// Sets the vertex shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var cb = new[]
            {
                BuiltInShaders.GetVSPerFrame(),
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
