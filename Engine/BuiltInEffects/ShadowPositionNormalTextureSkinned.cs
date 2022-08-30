
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-texture drawer
    /// </summary>
    public class ShadowPositionNormalTextureSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionNormalTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowSkinnedPositionNormalTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
