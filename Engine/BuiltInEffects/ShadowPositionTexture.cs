
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow position-texture drawer
    /// </summary>
    public class ShadowPositionTexture : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionTexture(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex)
        {
            var vertexShader = GetVertexShader<ShadowPositionTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
