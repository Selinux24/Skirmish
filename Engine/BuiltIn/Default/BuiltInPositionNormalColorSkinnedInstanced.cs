using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class BuiltInPositionNormalColorSkinnedInstanced : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorSkinnedVsI>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMaterial = new EngineConstantBuffer<PerMaterialColor>(graphics, nameof(BuiltInPositionNormalColorSkinnedInstanced) + "." + nameof(PerMaterialColor));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalColorSkinnedInstanced()
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
            cbPerMaterial.WriteData(PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
