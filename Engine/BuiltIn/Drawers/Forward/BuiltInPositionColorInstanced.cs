using Engine.BuiltIn.Drawers.Common;
using Engine.Common;

namespace Engine.BuiltIn.Drawers.Forward
{
    /// <summary>
    /// Basic position-color instanced drawer
    /// </summary>
    public class BuiltInPositionColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per aterial constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionColorInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionColorVsI>();
            SetPixelShader<PositionColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionColorVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
