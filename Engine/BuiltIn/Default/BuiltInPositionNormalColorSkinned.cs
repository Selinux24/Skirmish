﻿using System;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColorSkinned : BuiltInDrawer, IDisposable
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSkinned> cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorSkinnedVs>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMesh = new EngineConstantBuffer<PerMeshSkinned>(graphics, nameof(BuiltInPositionNormalColorSkinned) + "." + nameof(PerMeshSkinned));
            cbPerMaterial = new EngineConstantBuffer<PerMaterialColor>(graphics, nameof(BuiltInPositionNormalColorSkinned) + "." + nameof(PerMaterialColor));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInPositionNormalColorSkinned()
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

            var vertexShader = GetVertexShader<PositionNormalColorSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorSkinnedVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
