using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangentInstanced : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialTexture> cbPerMaterial;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;
        /// <summary>
        /// Anisotropic sampler
        /// </summary>
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureTangentInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureTangentVsI>();
            SetPixelShader<PositionNormalTextureTangentPs>();

            cbPerMaterial = new EngineConstantBuffer<PerMaterialTexture>(graphics, nameof(BuiltInPositionNormalTextureTangentInstanced) + "." + nameof(PerMaterialTexture));

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalTextureTangentInstanced()
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
                cbPerMaterial?.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureTangentVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var pixelShader = GetPixelShader<PositionNormalTextureTangentPs>();
            pixelShader?.SetDiffuseMap(state.Material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(state.Material.UseAnisotropic ? anisotropic : linear);
            pixelShader?.SetNormalMap(state.Material.Material?.NormalMap);
            pixelShader?.SetNormalSampler(state.Material.UseAnisotropic ? anisotropic : linear);
        }
    }
}
