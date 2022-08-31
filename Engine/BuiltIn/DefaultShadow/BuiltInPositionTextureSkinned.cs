using System;

namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-texture drawer
    /// </summary>
    public class BuiltInPositionTextureSkinned : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSkinned> cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialTexture> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionTextureSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionTextureSkinnedVs>();

            cbPerMesh = new EngineConstantBuffer<PerMeshSkinned>(graphics, nameof(BuiltInPositionTextureSkinned) + "." + nameof(PerMeshSkinned));
            cbPerMaterial = new EngineConstantBuffer<PerMaterialTexture>(graphics, nameof(BuiltInPositionTextureSkinned) + "." + nameof(PerMaterialTexture));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionTextureSkinned()
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
                cbPerMesh?.Dispose();
                cbPerMaterial?.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void UpdateMesh(BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSkinned.Build(state));

            var vertexShader = GetVertexShader<PositionTextureSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionTextureSkinnedVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
