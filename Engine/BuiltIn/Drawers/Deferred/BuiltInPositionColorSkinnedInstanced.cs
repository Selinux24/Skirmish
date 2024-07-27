using Engine.BuiltIn.Drawers.Common;
using Engine.Common;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Skinned position-color instanced drawer
    /// </summary>
    public class BuiltInPositionColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionColorSkinnedInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionColorSkinnedVsI>();
            SetPixelShader<PositionColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
