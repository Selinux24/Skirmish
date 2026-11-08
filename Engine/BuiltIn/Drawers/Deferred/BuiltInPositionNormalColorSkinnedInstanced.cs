using Engine.BuiltIn.Drawers.Common;
using Engine.Common;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class BuiltInPositionNormalColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterialColor> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPositionNormalColorSkinnedInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionNormalColorSkinnedVsI>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            dc.UpdateConstantBuffer(cbPerMaterial, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
