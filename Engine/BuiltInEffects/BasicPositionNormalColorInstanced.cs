using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-color instanced drawer
    /// </summary>
    public class BasicPositionNormalColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalColorVsI>();
            SetPixelShader<BasicPositionNormalColorPs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalColorVsI>();

            vertexShader?.WriteCBPerObject(material, tintColor);
        }
    }
}
