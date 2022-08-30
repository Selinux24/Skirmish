using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class BasicPositionNormalColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionNormalColorSkinnedVsI>();
            SetPixelShader<BasicPositionNormalColorPs>();
        }

        /// <inheritdoc/>
        public void Update(MaterialDrawInfo material, Color4 tintColor)
        {
            var vertexShader = GetVertexShader<BasicPositionNormalColorSkinnedVsI>();

            vertexShader?.WriteCBPerObject(material, tintColor);
        }
    }
}
