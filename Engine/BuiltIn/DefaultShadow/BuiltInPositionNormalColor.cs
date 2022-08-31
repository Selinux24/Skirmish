using System;

namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColor : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSingle> cbPerMesh;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorVs>();

            cbPerMesh = new EngineConstantBuffer<PerMeshSingle>(graphics, nameof(BuiltInPositionNormalColor) + "." + nameof(PerMeshSingle));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalColor()
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
            }
        }

        /// <inheritdoc/>
        public override void UpdateMesh(BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
    }
}
