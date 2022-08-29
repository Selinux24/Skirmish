using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Basic position-color instanced drawer
    /// </summary>
    public class BasicPositionColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionColorVsI>();
            SetPixelShader<BasicPositionColorPs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionColorVsI>();

            vertexShader?.WriteCBPerObject(material, tintColor);
        }
    }
}
