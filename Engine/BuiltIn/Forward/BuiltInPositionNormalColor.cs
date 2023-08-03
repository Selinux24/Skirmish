﻿
namespace Engine.BuiltIn.Forward
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColor : BuiltInDrawer
    {
        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMeshSingle> cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorVs>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMeshSingle.Build(state));
            dc.UpdateConstantBuffer(cbPerMesh);

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialColor.Build(state));
            dc.UpdateConstantBuffer(cbPerMaterial);

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
