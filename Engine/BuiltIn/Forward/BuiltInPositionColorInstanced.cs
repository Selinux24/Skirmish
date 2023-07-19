
namespace Engine.BuiltIn.Forward
{
    using Engine.BuiltIn.Common;
    using Engine.Common;

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
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorVsI>();
            SetPixelShader<PositionColorPs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterialColor>();
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(EngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterialColor.Build(state));
            dc.UpdateConstantBuffer(cbPerMaterial);

            var vertexShader = GetVertexShader<PositionColorVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
