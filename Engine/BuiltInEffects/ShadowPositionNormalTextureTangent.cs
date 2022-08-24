using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-texture-tangent drawer
    /// </summary>
    public class ShadowPositionNormalTextureTangent : BuiltInDrawer<ShadowPositionNormalTextureTangentVs, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerInstance(textureIndex);
        }
    }
}
