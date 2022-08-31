
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow position-texture drawer
    /// </summary>
    public class BuiltInPositionTexture : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionTexture(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex)
        {
            var vertexShader = GetVertexShader<PositionTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
