using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Basic position-color drawer
    /// </summary>
    public class BasicPositionColor : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionColorVs>();
            SetPixelShader<BasicPositionColorPs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionColorVs>();

            vertexShader?.WriteCBPerInstance(material, tintColor);
        }
    }
}
