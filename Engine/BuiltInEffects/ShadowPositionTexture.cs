using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

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
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowPositionTextureVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
