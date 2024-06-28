
namespace Engine.BuiltIn.Drawers.Deferred
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Common;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-color instanced drawer
    /// </summary>
    public class BuiltInPositionNormalColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per aterial constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionNormalColorInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionNormalColorVsI>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
