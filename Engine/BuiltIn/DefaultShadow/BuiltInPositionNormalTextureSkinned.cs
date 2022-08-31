
namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-texture drawer
    /// </summary>
    public class BuiltInPositionNormalTextureSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionNormalTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<SkinnedPositionNormalTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
