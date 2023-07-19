
namespace Engine.BuiltIn.Deferred
{
    using Engine.BuiltIn.Common;
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
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorVsI>();
            SetPixelShader<PositionNormalColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(dc, PerMaterialColor.Build(state));

            var vertexShader = GetVertexShader<PositionNormalColorVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
