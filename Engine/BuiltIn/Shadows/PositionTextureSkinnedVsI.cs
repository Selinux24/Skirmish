using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Skinned position texture instanced vertex shader
    /// </summary>
    public class PositionTextureSkinnedVsI : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionTextureSkinnedVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionTextureSkinnedVsI>("main", ShadowRenderingResources.PositionTextureSkinnedI_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobalConstantBuffer(),
            };

            dc.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetAnimationPaletteResourceView(),
            };

            dc.SetVertexShaderResourceViews(0, rv);
        }
    }
}
