using System;

namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-texture drawer
    /// </summary>
    public class BuiltInPositionNormalTexture : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSingle> cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialTexture> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTexture(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureVs>();

            cbPerMesh = new EngineConstantBuffer<PerMeshSingle>(graphics, nameof(PositionNormalTextureVs) + "." + nameof(PerMeshSingle));
            cbPerMaterial = new EngineConstantBuffer<PerMaterialTexture>(graphics, nameof(PositionNormalTextureVs) + "." + nameof(PerMaterialTexture));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalTexture()
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
            cbPerMesh.WriteData(PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
