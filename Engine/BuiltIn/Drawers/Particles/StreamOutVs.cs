using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Particles
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Stream-out GPU particles vertex shader
    /// </summary>
    public class StreamOutVs : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamOutVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<StreamOutVs>("main", ForwardRenderingResources.Streamout_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            //No resources
        }
    }
}
