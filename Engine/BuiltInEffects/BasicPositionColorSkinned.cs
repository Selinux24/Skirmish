using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-color drawer
    /// </summary>
    public class BasicPositionColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionColorSkinnedVs>();
            SetPixelShader<BasicPositionColorPs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionColorSkinnedVs>();

            vertexShader?.WriteCBPerInstance(material, tintColor, animation);
        }
    }
}
