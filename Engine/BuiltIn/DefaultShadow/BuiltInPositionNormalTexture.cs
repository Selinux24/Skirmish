
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow position-normal-texture drawer
    /// </summary>
    public class BuiltInPositionNormalTexture : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTexture(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex)
        {
            var vertexShader = GetVertexShader<PositionNormalTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
