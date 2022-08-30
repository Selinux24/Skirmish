using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;

    /// <summary>
    /// Skinned position-color instanced drawer
    /// </summary>
    public class BuiltInPositionColorSkinnedInstanced : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorSkinnedVsI>();
            SetPixelShader<PositionColorPs>();

            cbPerMaterial = new EngineConstantBuffer<PerMaterialColor>(graphics, nameof(BuiltInPositionColorSkinnedInstanced) + "." + nameof(PerMaterialColor));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionColorSkinnedInstanced()
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

            var vertexShader = GetVertexShader<PositionColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
