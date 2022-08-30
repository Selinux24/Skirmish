
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-texture drawer
    /// </summary>
    public class ShadowPositionNormalTexture : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTexture(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex)
        {
            var vertexShader = GetVertexShader<ShadowPositionNormalTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
