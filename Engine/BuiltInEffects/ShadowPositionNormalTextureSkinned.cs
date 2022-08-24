using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-texture drawer
    /// </summary>
    public class ShadowPositionNormalTextureSkinned : BuiltInDrawer<ShadowSkinnedPositionNormalTextureVs, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureSkinned(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
