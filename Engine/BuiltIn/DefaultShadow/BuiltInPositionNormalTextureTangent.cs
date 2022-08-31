using System;

namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-texture-tangent drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangent : BuiltInDrawer, IDisposable
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
        public BuiltInPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureTangentVs>();

            cbPerMesh = new EngineConstantBuffer<PerMeshSingle>(graphics, nameof(BuiltInPositionNormalTextureTangent) + "." + nameof(PerMeshSingle));
            cbPerMaterial = new EngineConstantBuffer<PerMaterialTexture>(graphics, nameof(BuiltInPositionNormalTextureTangent) + "." + nameof(PerMaterialTexture));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalTextureTangent()
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

            var vertexShader = GetVertexShader<PositionNormalTextureTangentVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialTexture.Build(state));

            var vertexShader = GetVertexShader<PositionNormalTextureTangentVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
