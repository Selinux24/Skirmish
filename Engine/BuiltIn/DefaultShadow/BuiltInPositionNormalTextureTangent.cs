
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow position-normal-texture-tangent drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangent : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureTangentVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex)
        {
            var vertexShader = GetVertexShader<PositionNormalTextureTangentVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
