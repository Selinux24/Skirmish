
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-texture drawer
    /// </summary>
    public class ShadowPositionTextureSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionTextureSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionTextureVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowSkinnedPositionTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
