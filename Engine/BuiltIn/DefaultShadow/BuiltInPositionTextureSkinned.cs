
namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-texture drawer
    /// </summary>
    public class BuiltInPositionTextureSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionTextureSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<SkinnedPositionTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
