using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position color pixel shader
    /// </summary>
    public class BasicPositionColorPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Shader
        /// </summary>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BasicPositionColorPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionColor_Cso == null;
            var bytes = Resources.Ps_PositionColor_Cso ?? Resources.Ps_PositionColor;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(BasicPositionColorPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(BasicPositionColorPs), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionColorPs()
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
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetPixelShaderConstantBuffer(0, BuiltInShaders.GetPSPerFrameNoLit());
        }
    }
}
