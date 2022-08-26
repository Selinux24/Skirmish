using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color drawer
    /// </summary>
    public class BasicPositionNormalColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalColorSkinnedVs>();
            SetPixelShader<BasicPositionNormalColorPs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalColorSkinnedVs>();

            vertexShader?.WriteCBPerInstance(material, tintColor, animation);
        }
    }
}
