using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
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
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowPositionNormalTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
