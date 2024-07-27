using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Decals
{
    /// <summary>
    /// Decals geometry shader
    /// </summary>
    public class DecalsGS : IShader<EngineGeometryShader>
    {
        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DecalsGS()
        {
            Shader = BuiltInShaders.CompileGeometryShader<DecalsGS>("main", ForwardRenderingResources.Decal_gs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);
        }
    }
}
