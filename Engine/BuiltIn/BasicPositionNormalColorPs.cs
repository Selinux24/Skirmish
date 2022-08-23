using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal color pixel shader
    /// </summary>
    public class BasicPositionNormalColorPs : IBuiltInPixelShader
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
        public BasicPositionNormalColorPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalColor_Cso == null;
            var bytes = Resources.Ps_PositionNormalColor_Cso ?? Resources.Ps_PositionNormalColor;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(BasicPositionNormalColorPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(BasicPositionNormalColorPs), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionNormalColorPs()
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
            var cb = new[]
            {
                BuiltInShaders.GetPSPerFrameLit(),
                BuiltInShaders.GetPSHemispheric(),
                BuiltInShaders.GetPSDirectionals(),
                BuiltInShaders.GetPSSpots(),
                BuiltInShaders.GetPSPoints(),
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetPSPerFrameLitShadowMapDir(),
                BuiltInShaders.GetPSPerFrameLitShadowMapSpot(),
                BuiltInShaders.GetPSPerFrameLitShadowMapPoint(),
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
