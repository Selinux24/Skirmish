using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-color instanced drawer
    /// </summary>
    public class BasicPositionColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionColorSkinnedVsI>();
            SetPixelShader<BasicPositionColorPs>();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionColorSkinnedVsI>();

            vertexShader?.WriteCBPerObject(material, tintColor);
        }
    }
}
