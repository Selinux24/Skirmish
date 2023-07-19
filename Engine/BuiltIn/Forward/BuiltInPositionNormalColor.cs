
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
        public override void UpdateMesh(EngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(dc, PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(dc, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
