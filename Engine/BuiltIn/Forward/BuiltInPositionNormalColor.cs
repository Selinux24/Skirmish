
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
        public BuiltInPositionNormalColor(Game game) : base(game)
        {
            SetVertexShader<PositionNormalColorVs>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMesh = BuiltInShaders.GetConstantBuffer<PerMeshSingle>();
            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMesh(IEngineDeviceContext dc, BuiltInDrawerMeshState state)
        {
            dc.UpdateConstantBuffer(cbPerMesh, PerMeshSingle.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
