using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow position-normal-texture-tangent drawer
    /// </summary>
    public class ShadowPositionNormalTextureTangent : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalTextureTangentVs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowPositionNormalTextureTangentVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
