using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Forward
{
    /// <summary>
    /// Position color pixel shader
    /// </summary>
    public class PositionColorPs : IShader<EnginePixelShader>
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionColorPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PositionColorPs>("main", ForwardRenderingResources.PositionColor_ps);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderConstantBuffer(0, BuiltInShaders.GetPerFrameConstantBuffer());
        }
    }
}
